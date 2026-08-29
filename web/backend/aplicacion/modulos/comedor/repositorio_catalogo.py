"""Persistencia del catálogo y las cuentas de tiquetes."""

from __future__ import annotations

import hashlib

from aplicacion.nucleo.base_datos import FabricaConexionSql

from .errores import IdempotenciaIncompatible
from .repositorio_base import RepositorioSqlComedorBase


class RepositorioSqlCatalogoComedor(RepositorioSqlComedorBase):
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def personas(
        self, tipo_persona: str | None = None, incluir_inactivas: bool = False
    ) -> list[dict]:
        filtro = "WHERE 1=1"
        parametros: list[object] = []
        if not incluir_inactivas:
                filtro += " AND p.activo=1"
        if tipo_persona:
            filtro += " AND p.tipo_persona=?"
            parametros.append(tipo_persona)
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT p.id_persona, p.tipo_persona, p.id_estudiante, p.id_usuario,
                p.codigo_barras, p.nombre_completo, p.colegio, p.id_estado_comedor,
                ec.descripcion AS beneficio_comedor, p.activo
                FROM comedor.persona p
                INNER JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=p.id_estado_comedor """
                + filtro
                + " ORDER BY nombre_completo, id_persona",
                *parametros,
            )
            columnas = [col[0] for col in cursor.description or ()]
            return [dict(zip(columnas, fila)) for fila in cursor.fetchall()]

    def persona_por_estudiante(self, id_estudiante: int) -> int:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT id_persona FROM comedor.persona
                WHERE id_estudiante=? AND tipo_persona='estudiante' AND activo=1""",
                id_estudiante,
            )
            fila = cursor.fetchone()
            if fila is None:
                raise ValueError("El estudiante no está habilitado para el comedor")
            return int(fila[0])

    def persona_por_usuario(self, id_usuario: int) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT p.id_persona, p.tipo_persona, p.id_estudiante, p.id_usuario,
                p.codigo_barras, p.nombre_completo, p.colegio, p.id_estado_comedor,
                ec.descripcion AS beneficio_comedor, p.activo
                FROM comedor.persona p
                INNER JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=p.id_estado_comedor
                WHERE p.id_usuario=? AND p.tipo_persona='profesor'""",
                id_usuario,
            )
            persona = self._fila(cursor)
            if persona is None:
                raise ValueError("El usuario no está registrado como profesor del comedor")
            if not persona["activo"]:
                raise ValueError("El profesor no está habilitado para el comedor")
            return persona

    def crear_profesor(self, id_usuario: int, nombre: str, colegio: str | None) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """IF NOT EXISTS (SELECT 1 FROM identidad.usuario WHERE id_usuario=?)
                    THROW 51000, 'El usuario no existe', 1;
                INSERT INTO comedor.persona
                (tipo_persona,id_usuario,codigo_barras,nombre_completo,colegio,
                 id_estado_comedor,activo,creado_en,actualizado_en)
                OUTPUT INSERTED.id_persona,INSERTED.tipo_persona,INSERTED.id_usuario,
                INSERTED.codigo_barras,INSERTED.nombre_completo,INSERTED.colegio,
                INSERTED.id_estado_comedor,INSERTED.activo
                VALUES ('profesor',?,'P-' + CONVERT(varchar(20),?),?,?,
                        2,1,SYSUTCDATETIME(),SYSUTCDATETIME())""",
                id_usuario,
                id_usuario,
                id_usuario,
                nombre.strip(),
                colegio.strip() if colegio else None,
            )
            persona = self._fila(cursor)
            if persona is None:
                raise RuntimeError("No se pudo registrar el profesor")
            cursor.execute(
                """INSERT INTO comedor.cuenta_tiquetes
                (id_persona,saldo,reservados,actualizado_en)
                VALUES (?,0,0,SYSUTCDATETIME())""",
                persona["id_persona"],
            )
            return persona

    def cuenta(self, id_persona: int) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            persona = self._persona(cursor, id_persona)
            cuenta = self._cuenta(cursor, id_persona)
            cuenta["disponibles"] = int(cuenta["saldo"]) - int(cuenta["reservados"])
            cuenta["tipo_persona"] = persona["tipo_persona"]
            return cuenta

    def movimientos(self, id_persona: int, limite: int) -> list[dict]:
        if not 1 <= limite <= 100:
            raise ValueError("El límite de movimientos debe estar entre 1 y 100")
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            self._persona(cursor, id_persona)
            cuenta = self._cuenta(cursor, id_persona)
            cursor.execute(
                """SELECT id_movimiento, id_cuenta, tipo, cantidad, saldo_anterior,
                saldo_nuevo, reservados_anterior, reservados_nuevo, clave_idempotencia,
                concepto, creado_por, creado_en
                FROM comedor.movimiento_tiquetes
                WHERE id_cuenta=? ORDER BY id_movimiento DESC
                OFFSET 0 ROWS FETCH NEXT ? ROWS ONLY""",
                cuenta["id_cuenta"],
                limite,
            )
            filas = cursor.fetchall()
            columnas = [columna[0] for columna in cursor.description or ()]
            return [dict(zip(columnas, fila)) for fila in filas]

    def recargar(
        self, id_persona: int, cantidad: int, concepto: str | None, clave: str, usuario: int
    ) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            persona = self._persona(cursor, id_persona)
            if persona["id_estado_comedor"] == 1:
                raise ValueError("Las personas becadas no compran ni reciben tiquetes")
            cuenta = self._cuenta(cursor, id_persona)
            huella = hashlib.sha256(
                f"{cuenta['id_cuenta']}|recarga|{cantidad}|{concepto or ''}|{usuario}".encode()
            ).digest()
            cursor.execute(
                """SELECT id_movimiento, id_cuenta, tipo, cantidad, saldo_anterior,
                saldo_nuevo, reservados_anterior, reservados_nuevo, clave_idempotencia,
                concepto, creado_por, creado_en, huella_idempotencia FROM comedor.movimiento_tiquetes
                WHERE clave_idempotencia=?""",
                clave,
            )
            existente = self._fila(cursor)
            if existente is not None:
                huella_existente = existente.pop("huella_idempotencia", None)
                if huella_existente is not None:
                    coincide = bytes(huella_existente) == huella
                else:
                    coincide = (
                        int(existente["id_cuenta"]) == int(cuenta["id_cuenta"])
                        and int(existente["cantidad"]) == cantidad
                        and (existente["concepto"] or "") == (concepto or "")
                        and int(existente["creado_por"] or 0) == int(usuario)
                    )
                if not coincide:
                    raise IdempotenciaIncompatible(
                        "La clave de idempotencia ya fue utilizada con otros datos"
                    )
                return existente
            saldo = int(cuenta["saldo"])
            reservados = int(cuenta["reservados"])
            nuevo = saldo + cantidad
            cursor.execute(
                """UPDATE comedor.cuenta_tiquetes SET saldo=?,actualizado_en=SYSUTCDATETIME()
                WHERE id_cuenta=?""",
                nuevo,
                cuenta["id_cuenta"],
            )
            cursor.execute(
                """INSERT INTO comedor.movimiento_tiquetes
                (id_cuenta,tipo,cantidad,saldo_anterior,saldo_nuevo,reservados_anterior,
                 reservados_nuevo,clave_idempotencia,huella_idempotencia,concepto,creado_por,creado_en)
                OUTPUT INSERTED.id_movimiento,INSERTED.id_cuenta,INSERTED.tipo,
                INSERTED.cantidad,INSERTED.saldo_anterior,INSERTED.saldo_nuevo,
                INSERTED.reservados_anterior,INSERTED.reservados_nuevo,
                INSERTED.clave_idempotencia,INSERTED.concepto,INSERTED.creado_por,INSERTED.creado_en
                VALUES (?, 'recarga', ?, ?, ?, ?, ?, ?, ?, ?, ?, SYSUTCDATETIME())""",
                cuenta["id_cuenta"],
                cantidad,
                saldo,
                nuevo,
                reservados,
                reservados,
                clave,
                huella,
                concepto,
                usuario,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo registrar la recarga")
            return resultado
