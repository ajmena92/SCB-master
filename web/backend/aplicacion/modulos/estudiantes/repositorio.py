"""Contratos y repositorio compuesto del dominio de estudiantes."""

from __future__ import annotations

from typing import Protocol, cast

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioEstudiantes(Protocol):
    """Contrato agregado que consumen los casos de uso y adaptadores HTTP."""

    def listar(self, pagina: int, tamano: int, buscar: str) -> tuple[list[dict], int]: ...
    def buscar_por_id(self, id_estudiante: int) -> dict | None: ...
    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict: ...
    def actualizar(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict: ...
    def obtener_foto(self, id_estudiante: int) -> tuple[bytes, str] | None: ...
    def guardar_foto(self, id_estudiante: int, contenido: bytes, tipo: str) -> None: ...
    def eliminar_foto(self, id_estudiante: int) -> None: ...
    def perfil_detallado(self, id_estudiante: int) -> dict: ...
    def secciones(self, turno: str | None = None) -> list[dict]: ...
    def asignar_beneficio(self, id_estudiante: int, id_beneficio: int | None) -> None: ...
    def actualizar_estado_comedor(self, id_estudiante: int, id_estado_comedor: int) -> None: ...
    def asignar_ruta(self, id_estudiante: int, id_ruta: int | None) -> None: ...
    def reiniciar_pin(self, id_estudiante: int, hash_contrasena: str) -> None: ...
    def listar_para_generacion_pines(self) -> list[dict]: ...
    def actualizar_pines_seccion(self, seccion: str | None, hashes: dict[int, str]) -> None: ...
    def buscar_credencial(self, carne: str) -> dict | None: ...
    def buscar_credencial_por_id(self, id_estudiante: int) -> dict | None: ...
    def actualizar_pin(self, id_estudiante: int, hash_pin: str) -> None: ...


class RepositorioSqlEstudiantes:
    """CRUD y consultas básicas de estudiantes."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _fila(cursor: CursorSql) -> dict | None:
        fila = cursor.fetchone()
        if fila is None:
            return None
        return dict(zip((col[0] for col in cursor.description), fila))

    @classmethod
    def _filas(cls, cursor: CursorSql) -> list[dict]:
        return [
            dict(zip((col[0] for col in cursor.description), fila)) for fila in cursor.fetchall()
        ]

    def listar(self, pagina: int, tamano: int, buscar: str) -> tuple[list[dict], int]:
        termino = f"%{buscar.strip()}%"
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT e.id_estudiante, e.carne, e.nombre, e.primer_apellido,
                e.segundo_apellido, e.cedula, e.seccion, e.turno,
                ba.id_beneficio, e.debe_cambiar_pin, e.activo,
                r.codigo AS ruta_codigo, r.descripcion AS ruta_descripcion,
                cp.id_estado_comedor, ec.descripcion AS beneficio_comedor,
                CAST(CASE WHEN f.id_estudiante IS NULL THEN 0 ELSE 1 END AS bit) AS tiene_foto
                FROM estudiantes.estudiante e
                LEFT JOIN estudiantes.fotografia f ON f.id_estudiante=e.id_estudiante
                LEFT JOIN transporte.asignacion_ruta ar ON ar.id_estudiante=e.id_estudiante AND ar.activa=1
                LEFT JOIN transporte.ruta r ON r.id_ruta=ar.id_ruta
                LEFT JOIN beneficios.asignacion ba ON ba.id_estudiante=e.id_estudiante
                LEFT JOIN comedor.persona cp ON cp.id_estudiante=e.id_estudiante
                    AND cp.tipo_persona='estudiante'
                LEFT JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=cp.id_estado_comedor
                WHERE e.activo = 1 AND (? = N'' OR CONCAT(e.nombre, N' ', e.primer_apellido,
                N' ', ISNULL(e.segundo_apellido, N''), N' ', e.carne) LIKE ?)
                ORDER BY e.primer_apellido, e.nombre, e.id_estudiante
                OFFSET ? ROWS FETCH NEXT ? ROWS ONLY""",
                buscar.strip(),
                termino,
                (pagina - 1) * tamano,
                tamano,
            )
            elementos = self._filas(cursor)
            cursor.execute(
                """SELECT COUNT(*) FROM estudiantes.estudiante
                WHERE activo = 1 AND (? = N'' OR CONCAT(nombre, N' ', primer_apellido,
                N' ', ISNULL(segundo_apellido, N''), N' ', carne) LIKE ?)""",
                buscar.strip(),
                termino,
            )
            fila = cursor.fetchone()
            return elementos, cast(int, fila[0]) if fila else 0

    def buscar_por_id(self, id_estudiante: int) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT e.id_estudiante, e.carne, e.nombre, e.primer_apellido,
                e.segundo_apellido, e.cedula, e.seccion, e.turno, e.debe_cambiar_pin,
                e.activo, cp.id_estado_comedor, ec.descripcion AS beneficio_comedor,
                CAST(CASE WHEN f.id_estudiante IS NULL THEN 0 ELSE 1 END AS bit) AS tiene_foto
                FROM estudiantes.estudiante e
                LEFT JOIN estudiantes.fotografia f ON f.id_estudiante=e.id_estudiante
                LEFT JOIN comedor.persona cp ON cp.id_estudiante=e.id_estudiante
                    AND cp.tipo_persona='estudiante'
                LEFT JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=cp.id_estado_comedor
                WHERE e.id_estudiante = ?""",
                id_estudiante,
            )
            return self._fila(cursor)

    def perfil_detallado(self, id_estudiante: int) -> dict:
        resultado = self.buscar_por_id(id_estudiante)
        if resultado is None:
            raise ValueError("Estudiante no encontrado")
        return resultado

    def secciones(self, turno: str | None = None) -> list[dict]:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT seccion, COUNT(*) FROM estudiantes.estudiante WHERE activo=1 AND (? IS NULL OR turno=?) GROUP BY seccion ORDER BY seccion",
                turno,
                turno,
            )
            return [
                {
                    "seccion": fila[0],
                    "etiqueta": str(fila[0]) if fila[0] is not None else "__SIN_SECCION__",
                    "total": int(str(fila[1])),
                }
                for fila in cursor.fetchall()
            ]

    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """INSERT INTO estudiantes.estudiante
                (carne, nombre, primer_apellido, segundo_apellido, cedula, seccion, activo, creado_por, direccion_ip)
                OUTPUT INSERTED.id_estudiante, INSERTED.carne, INSERTED.nombre, INSERTED.primer_apellido,
                INSERTED.segundo_apellido, INSERTED.cedula, INSERTED.seccion, INSERTED.activo
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)""",
                datos["carne"],
                datos["nombre"],
                datos["primer_apellido"],
                datos["segundo_apellido"],
                datos["cedula"],
                datos["seccion"],
                datos["activo"],
                id_usuario,
                ip,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo crear el estudiante")
            cursor.execute(
                """INSERT INTO comedor.persona
                (tipo_persona,id_estudiante,codigo_barras,nombre_completo,id_estado_comedor,activo,
                 creado_en,actualizado_en)
                VALUES ('estudiante',?,?,?,2,?,SYSUTCDATETIME(),SYSUTCDATETIME())""",
                resultado["id_estudiante"],
                f"E-{resultado['carne']}",
                " ".join(
                    str(resultado[campo] or "")
                    for campo in ("nombre", "primer_apellido", "segundo_apellido")
                ).strip(),
                bool(resultado["activo"]),
            )
            cursor.execute(
                """INSERT INTO comedor.cuenta_tiquetes(id_persona,saldo,reservados,actualizado_en)
                SELECT id_persona,0,0,SYSUTCDATETIME() FROM comedor.persona
                WHERE id_estudiante=?""",
                resultado["id_estudiante"],
            )
            return resultado

    def actualizar(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """UPDATE estudiantes.estudiante SET carne=?, nombre=?, primer_apellido=?,
                segundo_apellido=?, cedula=?, seccion=?, activo=?, actualizado_por=?, direccion_ip=?,
                fecha_actualizacion=SYSUTCDATETIME() WHERE id_estudiante=?""",
                datos["carne"],
                datos["nombre"],
                datos["primer_apellido"],
                datos["segundo_apellido"],
                datos["cedula"],
                datos["seccion"],
                datos["activo"],
                id_usuario,
                ip,
                id_estudiante,
            )
            if cursor.rowcount == 0:
                raise ValueError("Estudiante no encontrado")
            cursor.execute(
                """UPDATE comedor.persona SET nombre_completo=?,activo=?,actualizado_en=SYSUTCDATETIME()
                WHERE id_estudiante=? AND tipo_persona='estudiante'""",
                " ".join(
                    str(datos[campo] or "")
                    for campo in ("nombre", "primer_apellido", "segundo_apellido")
                ).strip(),
                bool(datos["activo"]),
                id_estudiante,
            )
            cursor.execute(
                "SELECT id_estudiante, carne, nombre, primer_apellido, segundo_apellido, cedula, seccion, activo FROM estudiantes.estudiante WHERE id_estudiante=?",
                id_estudiante,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo leer el estudiante actualizado")
            return resultado
