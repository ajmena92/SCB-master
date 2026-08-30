"""Persistencia transaccional de reservas e ingresos."""

from __future__ import annotations

import secrets
from datetime import date

from aplicacion.nucleo.base_datos import FabricaConexionSql

from .errores import (
    CarnetNoReconocido,
    HoraLimiteExcedida,
    HorarioNoConfigurado,
    IngresoDuplicado,
    PersonaInactiva,
    SinMarcaTransporte,
    SinReserva,
    TiqueteAgotado,
)
from .reglas_horario import esta_dentro_de_hora_limite, normalizar_horario
from .repositorio_base import RepositorioSqlComedorBase


class RepositorioSqlOperacionComedor(RepositorioSqlComedorBase):
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def reservar(self, id_persona: int, fecha: date, usuario: int | None) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            persona = self._persona(cursor, id_persona)
            cursor.execute(
                """SELECT id_reserva,id_persona,fecha,estado,requiere_tiquete,modalidad
                FROM comedor.reserva WITH (UPDLOCK, ROWLOCK)
                WHERE id_persona=? AND fecha=?""",
                id_persona,
                fecha,
            )
            existente = self._fila(cursor)
            requiere = persona["id_estado_comedor"] == 2
            modalidad = "tiquete" if requiere else "beca"
            if existente and existente["estado"] in ("consumida", "reservada"):
                return existente
            cuenta = None
            reservados_antes = 0
            if requiere:
                cuenta = self._cuenta(cursor, id_persona)
                if int(cuenta["saldo"]) - int(cuenta["reservados"]) < 1:
                    raise ValueError("No hay tiquetes disponibles para reservar el comedor")
                reservados_antes = int(cuenta["reservados"])
                cursor.execute(
                    """UPDATE comedor.cuenta_tiquetes SET reservados=reservados+1,
                    actualizado_en=SYSUTCDATETIME()
                    WHERE id_cuenta=? AND saldo-reservados>=1""",
                    cuenta["id_cuenta"],
                )
                if cursor.rowcount != 1:
                    raise ValueError("No hay tiquetes disponibles para reservar el comedor")
            if existente:
                id_reserva = existente["id_reserva"]
                cursor.execute(
                    """UPDATE comedor.reserva SET estado='reservada',requiere_tiquete=?,
                    modalidad=?,registrada_por=?,actualizada_en=SYSUTCDATETIME()
                    WHERE id_reserva=?""",
                    requiere,
                    modalidad,
                    usuario,
                    id_reserva,
                )
            else:
                cursor.execute(
                    """INSERT INTO comedor.reserva
                    (id_persona,fecha,estado,requiere_tiquete,modalidad,registrada_por,creado_en,actualizada_en)
                    OUTPUT INSERTED.id_reserva
                    VALUES (?,?,'reservada',?,?,?,SYSUTCDATETIME(),SYSUTCDATETIME())""",
                    id_persona,
                    fecha,
                    requiere,
                    modalidad,
                    usuario,
                )
                fila = cursor.fetchone()
                if fila is None:
                    raise RuntimeError("No se pudo crear la reserva")
                id_reserva = fila[0]
            if requiere and cuenta is not None:
                cursor.execute(
                    """INSERT INTO comedor.movimiento_tiquetes
                    (id_cuenta,tipo,cantidad,saldo_anterior,saldo_nuevo,reservados_anterior,
                     reservados_nuevo,clave_idempotencia,concepto,creado_por,creado_en)
                    VALUES (?, 'reserva', 1, ?, ?, ?, ?, ?, N'Reserva de comedor', ?, SYSUTCDATETIME())""",
                    cuenta["id_cuenta"],
                    int(cuenta["saldo"]),
                    int(cuenta["saldo"]),
                    reservados_antes,
                    reservados_antes + 1,
                    f"reserva:{id_reserva}:{secrets.token_hex(16)}",
                    usuario,
                )
            cursor.execute(
                """SELECT id_reserva,id_persona,fecha,estado,requiere_tiquete,modalidad
                FROM comedor.reserva WHERE id_reserva=?""",
                id_reserva,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo leer la reserva creada")
            return resultado

    def cancelar(self, id_persona: int, fecha: date, usuario: int | None) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            self._persona(cursor, id_persona)
            cursor.execute(
                """SELECT id_reserva,id_persona,fecha,estado,requiere_tiquete,modalidad
                FROM comedor.reserva WITH (UPDLOCK, ROWLOCK)
                WHERE id_persona=? AND fecha=?""",
                id_persona,
                fecha,
            )
            reserva = self._fila(cursor)
            if reserva is None:
                raise ValueError("La reserva no existe")
            if reserva["estado"] == "consumida":
                raise ValueError("El ingreso ya fue registrado y no se puede cancelar")
            if reserva["estado"] == "cancelada":
                return reserva
            if bool(reserva["requiere_tiquete"]):
                cuenta = self._cuenta(cursor, id_persona)
                cursor.execute(
                    """UPDATE comedor.cuenta_tiquetes SET reservados=reservados-1,
                    actualizado_en=SYSUTCDATETIME()
                    WHERE id_cuenta=? AND reservados>=1""",
                    cuenta["id_cuenta"],
                )
                if cursor.rowcount != 1:
                    raise ValueError("La reserva no tiene un tiquete reservado")
                cursor.execute(
                    """INSERT INTO comedor.movimiento_tiquetes
                    (id_cuenta,tipo,cantidad,saldo_anterior,saldo_nuevo,reservados_anterior,
                     reservados_nuevo,clave_idempotencia,concepto,creado_por,creado_en)
                    VALUES (?, 'liberacion', 1, ?, ?, ?, ?, ?, N'Cancelación de reserva', ?, SYSUTCDATETIME())""",
                    cuenta["id_cuenta"],
                    int(cuenta["saldo"]),
                    int(cuenta["saldo"]),
                    int(cuenta["reservados"]),
                    int(cuenta["reservados"]) - 1,
                    f"liberacion:{reserva['id_reserva']}:{secrets.token_hex(16)}",
                    usuario,
                )
            cursor.execute(
                """UPDATE comedor.reserva SET estado='cancelada',registrada_por=?,
                actualizada_en=SYSUTCDATETIME() WHERE id_reserva=?""",
                usuario,
                reserva["id_reserva"],
            )
            cursor.execute(
                """SELECT id_reserva,id_persona,fecha,estado,requiere_tiquete,modalidad
                FROM comedor.reserva WHERE id_reserva=?""",
                reserva["id_reserva"],
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo leer la reserva cancelada")
            return resultado

    def reserva_por_persona_fecha(self, id_persona: int, fecha: date) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT id_reserva,id_persona,fecha,estado,requiere_tiquete,modalidad
                FROM comedor.reserva WHERE id_persona=? AND fecha=?""",
                id_persona,
                fecha,
            )
            return self._fila(cursor)

    def ingresar(
        self,
        codigo_barras: str,
        fecha: date,
        usuario: int | None,
        terminal_id: str | None = None,
    ) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT GETDATE(), CAST(GETDATE() AS date), CAST(GETDATE() AS time(0))")
            reloj = cursor.fetchone()
            if reloj is None:
                raise RuntimeError("No se pudo obtener la hora del servidor")
            momento_servidor, fecha_servidor, hora_servidor = reloj
            if fecha != fecha_servidor:
                raise ValueError("La fecha de operación no coincide con la fecha del servidor")
            cursor.execute(
                """SELECT p.id_persona,p.tipo_persona,p.id_estado_comedor,ec.descripcion AS beneficio_comedor,p.activo
                FROM comedor.persona p WITH (UPDLOCK, ROWLOCK)
                INNER JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=p.id_estado_comedor
                WHERE p.codigo_barras=?""",
                codigo_barras.strip(),
            )
            persona = self._fila(cursor)
            if persona is None:
                raise CarnetNoReconocido("El código de barras no está registrado")
            if not persona["activo"]:
                raise PersonaInactiva("La persona no está habilitada para el comedor")
            cursor.execute(
                """SELECT id_ingreso FROM comedor.ingreso WITH (UPDLOCK, ROWLOCK)
                WHERE id_persona=? AND fecha=?""",
                persona["id_persona"],
                fecha,
            )
            if self._fila(cursor) is not None:
                raise IngresoDuplicado("El ingreso al comedor ya fue registrado para esta fecha")
            cursor.execute(
                """SELECT minutos_aviso_previo,permitir_marca_tardia,
                permitir_sin_marca_transporte FROM comedor.parametro WHERE id_parametro=1"""
            )
            politica = cursor.fetchone() or (15, 0, 1)
            permitir_tardia = bool(politica[1])
            permitir_sin_transporte = bool(politica[2])
            cursor.execute(
                """SELECT LOWER(LTRIM(RTRIM(e.turno)))
                FROM comedor.persona p
                LEFT JOIN estudiantes.estudiante e ON e.id_estudiante=p.id_estudiante
                WHERE p.id_persona=?""",
                persona["id_persona"],
            )
            turno_fila = cursor.fetchone()
            turno = normalizar_horario(str(turno_fila[0]) if turno_fila and turno_fila[0] else None)
            codigo = turno
            if persona["tipo_persona"] == "profesor":
                cursor.execute(
                    """SELECT TOP 1 codigo FROM comedor.horario_operacion
                    WHERE activo=1 AND hora_limite>=?
                    ORDER BY hora_limite""",
                    hora_servidor,
                )
                fila_horario = cursor.fetchone()
                codigo = str(fila_horario[0]) if fila_horario else None
            if codigo is None:
                raise HorarioNoConfigurado("La persona no tiene un horario de comedor configurado")
            cursor.execute(
                """SELECT hora_limite FROM comedor.horario_operacion
                WHERE codigo=? AND activo=1""",
                codigo,
            )
            horario = cursor.fetchone()
            if horario is None:
                raise HorarioNoConfigurado("El horario de comedor no está configurado")
            tardio = not esta_dentro_de_hora_limite(hora_servidor, horario[0])
            advertencias: list[str] = []
            if tardio and not permitir_tardia:
                raise HoraLimiteExcedida("La hora límite de marcación del comedor ya pasó")
            if tardio:
                advertencias.append("marca_tardia")
            marca_transporte = False
            if persona["tipo_persona"] == "estudiante":
                cursor.execute(
                    """SELECT CASE WHEN EXISTS(
                        SELECT 1 FROM transporte.uso_diario u
                        WHERE u.id_estudiante=? AND u.fecha=?
                    ) THEN 1 ELSE 0 END""",
                    persona["id_estudiante"],
                    fecha_servidor,
                )
                marca_transporte = bool((cursor.fetchone() or (0,))[0])
                if not marca_transporte and not permitir_sin_transporte:
                    raise SinMarcaTransporte("La persona no tiene marca diaria de transporte")
                if not marca_transporte:
                    advertencias.append("sin_marca_transporte")
            cursor.execute(
                """SELECT id_reserva,estado,requiere_tiquete,modalidad
                FROM comedor.reserva WITH (UPDLOCK, ROWLOCK)
                WHERE id_persona=? AND fecha=?""",
                persona["id_persona"],
                fecha,
            )
            reserva = self._fila(cursor)
            if reserva is None or reserva["estado"] != "reservada":
                raise SinReserva("La persona no tiene una reserva activa para esta fecha")
            modalidad = "tiquete" if bool(reserva["requiere_tiquete"]) else "beca"
            if modalidad == "tiquete":
                cuenta = self._cuenta(cursor, int(persona["id_persona"]))
                if int(cuenta["reservados"]) < 1 or int(cuenta["saldo"]) < 1:
                    raise TiqueteAgotado("La reserva no tiene un tiquete disponible")
                cursor.execute(
                    """UPDATE comedor.cuenta_tiquetes SET saldo=saldo-1,reservados=reservados-1,
                    actualizado_en=SYSUTCDATETIME()
                    WHERE id_cuenta=? AND saldo>=1 AND reservados>=1""",
                    cuenta["id_cuenta"],
                )
                if cursor.rowcount != 1:
                    raise TiqueteAgotado("La reserva no tiene un tiquete disponible")
                cursor.execute(
                    """INSERT INTO comedor.movimiento_tiquetes
                    (id_cuenta,tipo,cantidad,saldo_anterior,saldo_nuevo,reservados_anterior,
                     reservados_nuevo,clave_idempotencia,concepto,creado_por,creado_en)
                    VALUES (?, 'consumo', 1, ?, ?, ?, ?, ?, N'Ingreso al comedor', ?, SYSUTCDATETIME())""",
                    cuenta["id_cuenta"],
                    int(cuenta["saldo"]),
                    int(cuenta["saldo"]) - 1,
                    int(cuenta["reservados"]),
                    int(cuenta["reservados"]) - 1,
                    f"ingreso:{persona['id_persona']}:{fecha.isoformat()}",
                    usuario,
                )
            cursor.execute(
                """UPDATE comedor.reserva SET estado='consumida',
                actualizada_en=SYSUTCDATETIME() WHERE id_reserva=?""",
                reserva["id_reserva"],
            )
            cursor.execute(
                """INSERT INTO comedor.ingreso
                (id_persona,fecha,modalidad,codigo_horario,hora_marca,
                 marca_transporte_existente,registrado_por,creado_en,
                 hora_limite_aplicada,resultado,advertencias,permitir_marca_tardia,
                 permitir_sin_marca_transporte)
                OUTPUT INSERTED.id_ingreso,INSERTED.id_persona,INSERTED.fecha,
                INSERTED.modalidad,INSERTED.codigo_horario,INSERTED.hora_marca,
                INSERTED.marca_transporte_existente,INSERTED.registrado_por,
                INSERTED.resultado,INSERTED.hora_limite_aplicada,INSERTED.advertencias
                VALUES (?,?,?,?,?,?,?,SYSUTCDATETIME(),?,?,?,?,?)""",
                persona["id_persona"],
                fecha,
                modalidad,
                codigo,
                momento_servidor,
                marca_transporte,
                usuario,
                horario[0],
                "tardio" if tardio else "registrado",
                ";".join(advertencias) or None,
                permitir_tardia,
                permitir_sin_transporte,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo registrar el ingreso")
            cursor.execute(
                """INSERT INTO comedor.auditoria_ingreso
                (id_ingreso,id_persona,fecha,codigo_resultado,detalle,advertencias,
                 hora_servidor,registrado_por,terminal_id)
                VALUES (?,?,?, ?,?,?, ?,?,?)""",
                resultado["id_ingreso"],
                persona["id_persona"],
                fecha,
                resultado.get("resultado") or ("tardio" if tardio else "registrado"),
                "Ingreso registrado desde kiosco",
                ";".join(advertencias) or None,
                momento_servidor,
                usuario,
                terminal_id,
            )
            resultado["resultado"] = resultado.get("resultado") or (
                "tardio" if tardio else "registrado"
            )
            resultado["nombre_completo"] = persona["nombre_completo"]
            resultado["hora_limite"] = str(horario[0])
            resultado["advertencias"] = advertencias
            return resultado
