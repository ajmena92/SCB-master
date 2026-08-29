"""Primitivas SQL compartidas por los repositorios de comedor."""

from aplicacion.nucleo.base_datos import CursorSql


class RepositorioSqlComedorBase:
    @staticmethod
    def _fila(cursor: CursorSql) -> dict | None:
        fila = cursor.fetchone()
        if fila is None:
            return None
        return dict(zip((col[0] for col in cursor.description or ()), fila))

    @staticmethod
    def _persona(cursor: CursorSql, id_persona: int) -> dict:
        cursor.execute(
            """SELECT id_persona, tipo_persona, id_estudiante, id_usuario,
            codigo_barras, nombre_completo, colegio, p.id_estado_comedor,
            ec.descripcion AS beneficio_comedor, p.activo
            FROM comedor.persona p WITH (UPDLOCK, ROWLOCK)
            INNER JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=p.id_estado_comedor
            WHERE p.id_persona=?""",
            id_persona,
        )
        persona = RepositorioSqlComedorBase._fila(cursor)
        if persona is None:
            raise ValueError("La persona no existe en el catálogo del comedor")
        if not persona["activo"]:
            raise ValueError("La persona no está habilitada para el comedor")
        return persona

    def persona(self, id_persona: int) -> dict:
        """Obtiene una persona habilitada para validar reglas de negocio."""
        with self._fabrica.conexion() as conexion:
            return self._persona(conexion.cursor(), id_persona)

    @staticmethod
    def _cuenta(cursor: CursorSql, id_persona: int) -> dict:
        cursor.execute(
            """SELECT id_cuenta, id_persona, saldo, reservados, actualizado_en
            FROM comedor.cuenta_tiquetes WITH (UPDLOCK, ROWLOCK) WHERE id_persona=?""",
            id_persona,
        )
        cuenta = RepositorioSqlComedorBase._fila(cursor)
        if cuenta is None:
            raise ValueError("La persona no tiene cuenta de tiquetes")
        return cuenta
