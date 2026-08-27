"""Persistencia de fotografías de estudiantes."""

from __future__ import annotations

from typing import cast

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioSqlFotos:
    """Acceso exclusivo a la tabla de fotografías."""

    _fabrica: FabricaConexionSql

    def obtener_foto(self, id_estudiante: int) -> tuple[bytes, str] | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT contenido,tipo_contenido FROM estudiantes.fotografia WHERE id_estudiante=?",
                id_estudiante,
            )
            fila = cursor.fetchone()
        return (bytes(cast(bytes, fila[0])), str(fila[1])) if fila else None

    def guardar_foto(self, id_estudiante: int, contenido: bytes, tipo: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "MERGE estudiantes.fotografia AS t USING (SELECT ? id_estudiante) s ON t.id_estudiante=s.id_estudiante "
                "WHEN MATCHED THEN UPDATE SET contenido=?,tipo_contenido=? "
                "WHEN NOT MATCHED THEN INSERT(id_estudiante,contenido,tipo_contenido) VALUES(?,?,?);",
                id_estudiante,
                contenido,
                tipo,
                id_estudiante,
                contenido,
                tipo,
            )

    def eliminar_foto(self, id_estudiante: int) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "DELETE FROM estudiantes.fotografia WHERE id_estudiante=?", id_estudiante
            )
