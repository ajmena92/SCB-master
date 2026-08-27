"""Persistencia de reinicio y generación masiva de PIN."""

from __future__ import annotations

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioSqlPines:
    """Gestiona hashes, vencimientos y selección de estudiantes para PIN."""

    _fabrica: FabricaConexionSql

    def reiniciar_pin(self, id_estudiante: int, hash_contrasena: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET hash_contrasena=?, debe_cambiar_pin=1, fecha_expiracion_pin=DATEADD(day, 1, SYSUTCDATETIME()) WHERE id_estudiante=?",
                hash_contrasena,
                id_estudiante,
            )

    def listar_para_generacion_pines(self) -> list[dict]:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT id_estudiante,carne,nombre,primer_apellido,segundo_apellido,cedula,seccion,turno FROM estudiantes.estudiante WHERE activo=1 ORDER BY primer_apellido,nombre"
            )
            return [
                dict(zip((col[0] for col in cursor.description), fila))
                for fila in cursor.fetchall()
            ]

    def actualizar_pines_seccion(self, seccion: str | None, hashes: dict[int, str]) -> None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            for id_estudiante, hash_pin in hashes.items():
                cursor.execute(
                    "UPDATE estudiantes.estudiante SET hash_contrasena=?, debe_cambiar_pin=1, fecha_expiracion_pin=DATEADD(day, 1, SYSUTCDATETIME()) WHERE id_estudiante=? AND activo=1 AND ((seccion=? ) OR (seccion IS NULL AND ? IS NULL))",
                    hash_pin,
                    id_estudiante,
                    seccion,
                    seccion,
                )
