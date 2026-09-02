"""Restituye la planificación semanal, sustituciones y publicaciones del menú.

Revision ID: 0010_menu_calendario_canonico
"""

import sqlalchemy as sa
from alembic import op

revision = "0010_menu_calendario_canonico"
down_revision = "0009_calendario_menu"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.alter_column("plantilla_menu", "nombre", new_column_name="titulo")
    op.drop_constraint("uq_plantilla_menu_nombre", "plantilla_menu", type_="unique")
    op.add_column("plantilla_menu", sa.Column("semana", sa.SmallInteger(), nullable=True))
    op.add_column("plantilla_menu", sa.Column("dia", sa.SmallInteger(), nullable=True))
    op.add_column("plantilla_menu", sa.Column("observaciones", sa.Text(), nullable=True))
    op.execute(
        """
        WITH ordenadas AS (
          SELECT id, row_number() OVER (ORDER BY id) AS posicion
          FROM plantilla_menu
        )
        UPDATE plantilla_menu p
        SET semana = ((o.posicion - 1) / 5)::integer + 1,
            dia = ((o.posicion - 1) % 5) + 1
        FROM ordenadas o
        WHERE p.id = o.id AND (p.semana IS NULL OR p.dia IS NULL)
        """
    )
    op.alter_column("plantilla_menu", "semana", nullable=False)
    op.alter_column("plantilla_menu", "dia", nullable=False)
    op.create_unique_constraint("uq_plantilla_menu_semana_dia", "plantilla_menu", ["semana", "dia"])
    op.create_check_constraint("semana_plantilla_menu", "plantilla_menu", "semana BETWEEN 1 AND 5")
    op.create_check_constraint("dia_plantilla_menu", "plantilla_menu", "dia BETWEEN 1 AND 5")
    op.add_column("componente_menu", sa.Column("tipo", sa.String(40), nullable=False, server_default="Principal"))
    op.alter_column("componente_menu", "tipo", server_default=None)

    op.alter_column("publicacion_menu", "nombre", new_column_name="titulo")
    op.add_column("publicacion_menu", sa.Column("observaciones", sa.Text(), nullable=True))
    op.add_column("publicacion_menu", sa.Column("origen", sa.String(20), nullable=False, server_default="plantilla"))
    op.alter_column("publicacion_menu", "origen", server_default=None)
    op.add_column("componente_publicado", sa.Column("tipo", sa.String(40), nullable=False, server_default="Principal"))
    op.alter_column("componente_publicado", "tipo", server_default=None)
    op.add_column("calendario_menu", sa.Column("motivo", sa.String(300), nullable=True))

    op.create_table(
        "sustitucion_menu",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("titulo", sa.String(180), nullable=False),
        sa.Column("observaciones", sa.Text(), nullable=True),
        sa.UniqueConstraint("fecha", name="uq_sustitucion_menu_fecha"),
    )
    op.create_table(
        "componente_sustitucion_menu",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("sustitucion_id", sa.Integer(), nullable=False),
        sa.Column("nombre", sa.String(180), nullable=False),
        sa.Column("tipo", sa.String(40), nullable=False),
        sa.Column("orden", sa.Integer(), nullable=False),
        sa.ForeignKeyConstraint(["sustitucion_id"], ["sustitucion_menu.id"], ondelete="CASCADE"),
        sa.UniqueConstraint("sustitucion_id", "orden"),
        sa.CheckConstraint("orden > 0", name="orden_componente_sustitucion_menu"),
    )


def downgrade() -> None:
    raise RuntimeError("La normalización de menú requiere restaurar desde respaldo")
