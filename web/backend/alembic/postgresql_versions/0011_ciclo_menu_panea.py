"""Configura la fecha ancla institucional de la semana 1 del ciclo PANEA."""

import sqlalchemy as sa
from alembic import op

revision = "0011_ciclo_menu_panea"
down_revision = "0010_menu_calendario_canonico"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "configuracion_ciclo_menu",
        sa.Column("id", sa.SmallInteger(), primary_key=True),
        sa.Column("inicio_ciclo_menu", sa.Date(), nullable=False),
        sa.CheckConstraint("id = 1", name="configuracion_ciclo_menu_unica"),
        sa.CheckConstraint("EXTRACT(ISODOW FROM inicio_ciclo_menu) = 1", name="inicio_ciclo_menu_lunes"),
    )
    # Evidencia operativa del comedor: 24–28 de agosto de 2026 fue Semana 4.
    # El lunes 16 de marzo de 2026 es una Semana 1 equivalente del ciclo de cinco semanas.
    op.execute(
        "INSERT INTO configuracion_ciclo_menu(id, inicio_ciclo_menu) VALUES (1, DATE '2026-03-16')"
    )


def downgrade() -> None:
    op.drop_table("configuracion_ciclo_menu")
