"""Agrega identidad institucional editable para reportes."""

import sqlalchemy as sa
from alembic import op


revision = "0017_configuracion_institucional"
down_revision = "0016_confirmacion_sin_tiquete"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "configuracion_institucional",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("nombre_colegio", sa.String(180), nullable=False),
        sa.Column("subtitulo_reportes", sa.String(220), nullable=False),
    )
    op.execute("INSERT INTO configuracion_institucional (id, nombre_colegio, subtitulo_reportes) VALUES (1, 'Colegio Técnico Profesional de Platanares', 'Comedor estudiantil')")


def downgrade() -> None:
    op.drop_table("configuracion_institucional")
