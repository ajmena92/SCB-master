"""Persistencia de asignaciones de beneficio y ruta."""

from __future__ import annotations

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioSqlAsignaciones:
    """Gestiona las referencias de beneficio y ruta del estudiante."""

    _fabrica: FabricaConexionSql

    def asignar_beneficio(self, id_estudiante: int, id_beneficio: int | None) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET id_beneficio=? WHERE id_estudiante=?",
                id_beneficio,
                id_estudiante,
            )

    def asignar_ruta(self, id_estudiante: int, id_ruta: int | None) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET id_ruta=? WHERE id_estudiante=?",
                id_ruta,
                id_estudiante,
            )
