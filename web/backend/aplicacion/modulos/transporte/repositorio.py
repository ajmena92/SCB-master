"""Persistencia SQL del dominio de transporte."""

from __future__ import annotations

from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioRutas(Protocol):
    def listar(self, incluir_inactivas: bool = False) -> list[dict]: ...

    def crear(
        self, codigo: str, descripcion: str, activo: bool, color_hex: str, id_usuario: int, ip: str
    ) -> dict: ...

    def actualizar(
        self,
        id_ruta: int,
        codigo: str,
        descripcion: str,
        activo: bool,
        color_hex: str,
        id_usuario: int,
        ip: str,
    ) -> dict: ...


class RepositorioSqlRutas:
    """Implementación aislada: solo consulta el esquema ``transporte``."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _muchas(cursor: CursorSql) -> list[dict]:
        columnas = [descripcion[0] for descripcion in cursor.description]
        return [dict(zip(columnas, fila)) for fila in cursor.fetchall()]

    @staticmethod
    def _una(cursor: CursorSql) -> dict | None:
        fila = cursor.fetchone()
        if not fila:
            return None
        columnas = [descripcion[0] for descripcion in cursor.description]
        return dict(zip(columnas, fila))

    def listar(self, incluir_inactivas: bool = False) -> list[dict]:
        condiciones = ["LTRIM(RTRIM(r.codigo)) <> N'0000'"]
        if not incluir_inactivas:
            condiciones.append("r.activo = 1")
        filtro = "WHERE " + " AND ".join(condiciones)
        consulta = f"""
            SELECT r.id_ruta, r.codigo, r.descripcion, r.activo, r.color_hex,
                   COUNT(a.id_estudiante) AS estudiantes_asignados
            FROM transporte.ruta AS r
            LEFT JOIN transporte.asignacion_ruta AS a
              ON a.id_ruta = r.id_ruta AND a.activa = 1
            {filtro}
            GROUP BY r.id_ruta, r.codigo, r.descripcion, r.activo, r.color_hex
            ORDER BY r.activo DESC, r.codigo, r.id_ruta
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta)
            return self._muchas(cursor)

    def crear(
        self, codigo: str, descripcion: str, activo: bool, color_hex: str, id_usuario: int, ip: str
    ) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """
                INSERT INTO transporte.ruta
                    (codigo, descripcion, color_hex, activo, creado_por, direccion_ip)
                OUTPUT INSERTED.id_ruta, INSERTED.codigo, INSERTED.descripcion,
                       INSERTED.color_hex, INSERTED.activo, CAST(0 AS int) AS estudiantes_asignados
                VALUES (?, ?, ?, ?, ?, ?)
            """,
                codigo,
                descripcion,
                color_hex,
                activo,
                id_usuario,
                ip or "WEB",
            )
            ruta = self._una(cursor)
            if ruta is None:  # pragma: no cover - SQL Server garantiza OUTPUT
                raise RuntimeError("No se pudo crear la ruta")
            return ruta

    def actualizar(
        self,
        id_ruta: int,
        codigo: str,
        descripcion: str,
        activo: bool,
        color_hex: str,
        id_usuario: int,
        ip: str,
    ) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """
                UPDATE transporte.ruta
                SET codigo = ?, descripcion = ?, color_hex = ?, activo = ?,
                    actualizado_por = ?, direccion_ip = ?, fecha_actualizacion = SYSUTCDATETIME()
                WHERE id_ruta = ?
            """,
                codigo,
                descripcion,
                color_hex,
                activo,
                id_usuario,
                ip or "WEB",
                id_ruta,
            )
            if cursor.rowcount == 0:
                raise ValueError("Ruta no encontrada")
            cursor.execute(
                """
                SELECT r.id_ruta, r.codigo, r.descripcion, r.activo, r.color_hex,
                       COUNT(a.id_estudiante) AS estudiantes_asignados
                FROM transporte.ruta AS r
                LEFT JOIN transporte.asignacion_ruta AS a
                  ON a.id_ruta = r.id_ruta AND a.activa = 1
                WHERE r.id_ruta = ?
                GROUP BY r.id_ruta, r.codigo, r.descripcion, r.activo, r.color_hex
            """,
                id_ruta,
            )
            ruta = self._una(cursor)
            if ruta is None:  # pragma: no cover
                raise RuntimeError("No se pudo leer la ruta actualizada")
            return ruta
