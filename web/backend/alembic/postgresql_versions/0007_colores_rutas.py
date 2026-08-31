"""Consolida codigo, descripcion y color de carnet en las rutas PostgreSQL.

Revision ID: 0007_colores_rutas
"""

import sqlalchemy as sa

from alembic import op

revision = "0007_colores_rutas"
down_revision = "0006_captura_comedor"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column("ruta", sa.Column("codigo", sa.String(50), nullable=True))
    op.add_column("ruta", sa.Column("descripcion", sa.String(500), nullable=True))
    op.add_column(
        "ruta",
        sa.Column("color_hex", sa.String(7), nullable=True, server_default="#CBD5E1"),
    )
    op.execute(
        """
        UPDATE ruta
        SET codigo = CASE WHEN nombre = 'SIN RUTA' THEN '0'
                          ELSE split_part(nombre, '-', 1) END,
            descripcion = CASE WHEN nombre = 'SIN RUTA' THEN 'Sin ruta asignada'
                               WHEN strpos(nombre, '-') > 0
                               THEN substr(nombre, strpos(nombre, '-') + 1)
                               ELSE nombre END
        """
    )
    colores = {
        "5369": "#EF4444",
        "5370": "#F472B6",
        "5371": "#D946EF",
        "1115306": "#F59E0B",
        "1115307": "#FACC15",
        "1115308": "#38BDF8",
        "1115309": "#FB8C6A",
        "1115311": "#A78BFA",
        "1115336": "#4ADE80",
        "0": "#FFFFFF",
    }
    for codigo, color in colores.items():
        op.execute(
            sa.text("UPDATE ruta SET color_hex=:color WHERE codigo=:codigo").bindparams(
                color=color, codigo=codigo
            )
        )
    op.alter_column("ruta", "codigo", nullable=False)
    op.alter_column("ruta", "descripcion", nullable=False)
    op.alter_column("ruta", "color_hex", nullable=False)
    op.create_unique_constraint("uq_ruta_codigo", "ruta", ["codigo"])
    op.create_check_constraint("ck_ruta_color_hex_ruta", "ruta", "color_hex ~ '^#[0-9A-Fa-f]{6}$'")


def downgrade() -> None:
    op.drop_constraint("ck_ruta_color_hex_ruta", "ruta", type_="check")
    op.drop_constraint("uq_ruta_codigo", "ruta", type_="unique")
    op.drop_column("ruta", "color_hex")
    op.drop_column("ruta", "descripcion")
    op.drop_column("ruta", "codigo")
