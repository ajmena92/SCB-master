"""Persistencia de asignaciones de beneficio y ruta."""

from __future__ import annotations

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioSqlAsignaciones:
    """Gestiona las referencias de beneficio y ruta del estudiante."""

    _fabrica: FabricaConexionSql

    def asignar_beneficio(self, id_estudiante: int, id_beneficio: int | None) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "MERGE beneficios.asignacion AS destino USING (SELECT ? AS id_estudiante) AS origen ON destino.id_estudiante=origen.id_estudiante "
                "WHEN MATCHED THEN UPDATE SET id_beneficio=?, actualizado_por=1, fecha_actualizacion=SYSUTCDATETIME() "
                "WHEN NOT MATCHED THEN INSERT (id_estudiante,id_beneficio,creado_por) VALUES (?, ?, 1);",
                id_estudiante,
                id_beneficio,
                id_estudiante,
                id_beneficio,
            )

    def actualizar_estado_comedor(self, id_estudiante: int, id_estado_comedor: int) -> None:
        if id_estado_comedor not in {1, 2}:
            raise ValueError("El estado de comedor no es válido")
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """UPDATE comedor.persona
                SET id_estado_comedor=?, actualizado_en=SYSUTCDATETIME()
                WHERE id_estudiante=? AND tipo_persona='estudiante'""",
                id_estado_comedor,
                id_estudiante,
            )
            if cursor.rowcount != 1:
                raise ValueError("El estudiante no está habilitado para el comedor")

    def asignar_ruta(self, id_estudiante: int, id_ruta: int | None) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "MERGE transporte.asignacion_ruta AS destino USING (SELECT ? AS id_estudiante) AS origen ON destino.id_estudiante=origen.id_estudiante "
                "WHEN MATCHED THEN UPDATE SET id_ruta=?, activa=CASE WHEN ? IS NULL THEN 0 ELSE 1 END "
                "WHEN NOT MATCHED AND ? IS NOT NULL THEN INSERT (id_estudiante,id_ruta) VALUES (?,?);",
                id_estudiante,
                id_ruta,
                id_ruta,
                id_ruta,
                id_estudiante,
                id_ruta,
            )
