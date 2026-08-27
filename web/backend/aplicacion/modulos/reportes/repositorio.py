"""Persistencia de consultas de reportes, limitada a esquemas web canónicos."""

from __future__ import annotations

from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioReportes(Protocol):
    def estudiantes(self) -> list[dict]: ...
    def transporte(self) -> list[dict]: ...
    def resumen(self) -> dict: ...


class RepositorioSqlReportes:
    """Consultas de solo lectura; no accede a dbo, Seguridad ni WinForms."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _filas(cursor: CursorSql) -> list[dict]:
        columnas = [descripcion[0] for descripcion in cursor.description]
        return [dict(zip(columnas, fila)) for fila in cursor.fetchall()]

    def estudiantes(self) -> list[dict]:
        consulta = """
            SELECT id_estudiante, carne,
                   CONCAT(nombre, N' ', primer_apellido, N' ',
                          ISNULL(segundo_apellido, N'')) AS nombre_completo,
                   seccion, activo
            FROM estudiantes.estudiante
            ORDER BY primer_apellido, nombre, id_estudiante
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta)
            return self._filas(cursor)

    def transporte(self) -> list[dict]:
        consulta = """
            SELECT r.id_ruta, r.codigo, r.descripcion, r.activo,
                   COUNT(a.id_estudiante) AS estudiantes_asignados
            FROM transporte.ruta AS r
            LEFT JOIN transporte.asignacion_ruta AS a
              ON a.id_ruta = r.id_ruta AND a.activa = 1
            GROUP BY r.id_ruta, r.codigo, r.descripcion, r.activo
            ORDER BY r.activo DESC, r.codigo, r.id_ruta
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta)
            return self._filas(cursor)

    def resumen(self) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT COUNT(*) estudiantes FROM estudiantes.estudiante WHERE activo=1")
            estudiantes = cursor.fetchone()
            cursor.execute("SELECT COUNT(*) confirmaciones FROM asistencia.marca WHERE estado='confirmada'")
            confirmaciones = cursor.fetchone()
            cursor.execute("SELECT COUNT(*) cancelaciones FROM asistencia.marca WHERE estado='cancelada'")
            cancelaciones = cursor.fetchone()
        return {"estudiantes": int(str(estudiantes[0])) if estudiantes else 0,
                "confirmaciones": int(str(confirmaciones[0])) if confirmaciones else 0,
                "cancelaciones": int(str(cancelaciones[0])) if cancelaciones else 0}
