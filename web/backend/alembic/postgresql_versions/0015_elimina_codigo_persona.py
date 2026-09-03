"""Retira el código legado de identidad de persona.

Revision ID: 0015_elimina_codigo_persona
"""

from alembic import op


revision = "0015_elimina_codigo_persona"
down_revision = "0014_indicadores_comedor"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.execute(
        """
        DO $$
        BEGIN
            IF EXISTS (SELECT 1 FROM persona WHERE cedula IS NULL OR btrim(cedula) = '') THEN
                RAISE EXCEPTION 'No se puede eliminar persona.codigo: existen personas sin cédula válida';
            END IF;
        END $$;
        """
    )
    op.drop_index("ix_persona_codigo", table_name="persona")
    op.drop_column("persona", "codigo")


def downgrade() -> None:
    raise RuntimeError("La eliminación de persona.codigo es irreversible")
