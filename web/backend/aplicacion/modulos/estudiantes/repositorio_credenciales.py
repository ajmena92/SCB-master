"""Persistencia de credenciales de estudiantes."""

from __future__ import annotations

from aplicacion.nucleo.base_datos import FabricaConexionSql

from .repositorio_compartido import fila_desde_cursor


class RepositorioSqlCredenciales:
    """Consultas de autenticación y perfil autenticado."""

    _fabrica: FabricaConexionSql

    def buscar_credencial(self, carne: str) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT id_estudiante, carne, nombre, hash_contrasena, debe_cambiar_pin, fecha_expiracion_pin, activo FROM estudiantes.estudiante WHERE carne=? AND (fecha_expiracion_pin IS NULL OR fecha_expiracion_pin > SYSUTCDATETIME())",
                carne.strip(),
            )
            return fila_desde_cursor(cursor)

    def buscar_credencial_por_id(self, id_estudiante: int) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT e.id_estudiante, e.carne, e.nombre, e.primer_apellido,
                e.segundo_apellido, e.cedula, e.seccion, e.turno, e.id_ruta,
                e.id_beneficio, e.hash_contrasena, e.debe_cambiar_pin,
                e.fecha_expiracion_pin, e.activo, r.codigo AS ruta_codigo,
                r.descripcion AS ruta_descripcion, r.color_hex AS ruta_color,
                b.nombre AS tipo_beca,
                CAST(CASE WHEN f.id_estudiante IS NULL THEN 0 ELSE 1 END AS bit) AS tiene_foto
                FROM estudiantes.estudiante e
                LEFT JOIN estudiantes.fotografia f ON f.id_estudiante=e.id_estudiante
                LEFT JOIN transporte.asignacion_ruta ar ON ar.id_estudiante=e.id_estudiante AND ar.activa=1
                LEFT JOIN transporte.ruta r ON r.id_ruta=COALESCE(e.id_ruta, ar.id_ruta)
                LEFT JOIN beneficios.asignacion ba ON ba.id_estudiante=e.id_estudiante
                LEFT JOIN beneficios.tipo_beneficio b ON b.id_beneficio=COALESCE(e.id_beneficio, ba.id_beneficio)
                WHERE e.id_estudiante=?""",
                id_estudiante,
            )
            return fila_desde_cursor(cursor)

    def actualizar_pin(self, id_estudiante: int, hash_pin: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET hash_contrasena=?, debe_cambiar_pin=0, fecha_expiracion_pin=NULL WHERE id_estudiante=? AND activo=1",
                hash_pin,
                id_estudiante,
            )
