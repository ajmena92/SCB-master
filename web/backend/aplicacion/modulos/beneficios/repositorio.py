"""Persistencia aislada del esquema beneficios."""

from __future__ import annotations

from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioBeneficios(Protocol):
    def listar(self, incluir_inactivos: bool = False) -> list[dict]: ...

    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict: ...

    def actualizar(self, id_beneficio: int, datos: dict, id_usuario: int, ip: str) -> dict: ...

    def asignacion(self, id_estudiante: int) -> dict: ...

    def asignar(
        self, id_estudiante: int, id_beneficio: int | None, id_usuario: int, ip: str
    ) -> dict: ...


class RepositorioSqlBeneficios:
    """Implementación SQL que solo accede a tablas del esquema beneficios."""

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

    def listar(self, incluir_inactivos: bool = False) -> list[dict]:
        filtro = "" if incluir_inactivos else "WHERE activo = 1"
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                f"SELECT id_beneficio, nombre, descripcion, dias_permitidos, activo "
                f"FROM beneficios.tipo_beneficio {filtro} ORDER BY activo DESC, nombre"
            )
            return self._filas(cursor)

    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """INSERT INTO beneficios.tipo_beneficio
                (nombre, descripcion, dias_permitidos, activo, creado_por, direccion_ip)
                OUTPUT INSERTED.id_beneficio, INSERTED.nombre, INSERTED.descripcion,
                    INSERTED.dias_permitidos, INSERTED.activo
                VALUES (?, ?, ?, ?, ?, ?)""",
                datos["nombre"],
                datos.get("descripcion"),
                datos["dias_permitidos"],
                datos["activo"],
                id_usuario,
                ip or "WEB",
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo crear el beneficio")
            return resultado

    def actualizar(self, id_beneficio: int, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """UPDATE beneficios.tipo_beneficio SET nombre=?, descripcion=?, dias_permitidos=?,
                activo=?, actualizado_por=?, direccion_ip=?, fecha_actualizacion=SYSUTCDATETIME()
                WHERE id_beneficio=?""",
                datos["nombre"],
                datos.get("descripcion"),
                datos["dias_permitidos"],
                datos["activo"],
                id_usuario,
                ip or "WEB",
                id_beneficio,
            )
            if cursor.rowcount == 0:
                raise ValueError("Beneficio no encontrado")
            cursor.execute(
                """SELECT id_beneficio, nombre, descripcion, dias_permitidos, activo
                FROM beneficios.tipo_beneficio WHERE id_beneficio=?""",
                id_beneficio,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo leer el beneficio actualizado")
            return resultado

    def asignacion(self, id_estudiante: int) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT id_estudiante, id_beneficio FROM beneficios.asignacion
                WHERE id_estudiante=?""",
                id_estudiante,
            )
            resultado = self._fila(cursor)
            return resultado or {"id_estudiante": id_estudiante, "id_beneficio": None}

    def asignar(
        self, id_estudiante: int, id_beneficio: int | None, id_usuario: int, ip: str
    ) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """MERGE beneficios.asignacion AS destino USING (SELECT ? AS id_estudiante) AS origen
                ON destino.id_estudiante=origen.id_estudiante
                WHEN MATCHED THEN UPDATE SET id_beneficio=?, actualizado_por=?, direccion_ip=?,
                    fecha_actualizacion=SYSUTCDATETIME()
                WHEN NOT MATCHED THEN INSERT (id_estudiante, id_beneficio, creado_por, direccion_ip)
                    VALUES (?, ?, ?, ?);""",
                id_estudiante,
                id_beneficio,
                id_usuario,
                ip or "WEB",
                id_estudiante,
                id_beneficio,
                id_usuario,
                ip or "WEB",
            )
            cursor.execute(
                "SELECT id_estudiante, id_beneficio FROM beneficios.asignacion WHERE id_estudiante=?",
                id_estudiante,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo guardar la asignación")
            return resultado
