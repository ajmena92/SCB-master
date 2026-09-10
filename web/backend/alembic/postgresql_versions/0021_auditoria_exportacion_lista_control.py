"""Registra la salida de listas de control sin almacenar su contenido.

Revision ID: 0021_auditoria_exportacion_lista_control
"""

import sqlalchemy as sa

from alembic import op

revision = "0021_auditoria_exportacion_lista_control"
down_revision = "0020_fotografia_persona"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "evento_exportacion_lista_control",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column(
            "cuenta_administrativa_id",
            sa.Integer(),
            sa.ForeignKey("cuenta_administrativa.id"),
            nullable=False,
        ),
        sa.Column("servicio", sa.String(16), nullable=False),
        sa.Column("formato", sa.String(8), nullable=False),
        sa.Column("fecha_operativa", sa.Date(), nullable=False),
        sa.Column("filtros", sa.Text(), nullable=False, server_default="{}"),
        sa.Column("total_registros", sa.Integer(), nullable=False),
        sa.Column(
            "creado_en", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")
        ),
        sa.CheckConstraint(
            "servicio IN ('comedor','transporte')", name="servicio_exportacion_lista"
        ),
        sa.CheckConstraint("formato IN ('csv','xlsx','pdf')", name="formato_exportacion_lista"),
        sa.CheckConstraint("total_registros >= 0", name="total_exportacion_lista"),
    )
    op.create_index(
        "ix_exportacion_lista_fecha_cuenta",
        "evento_exportacion_lista_control",
        ["fecha_operativa", "cuenta_administrativa_id"],
    )


def downgrade() -> None:
    op.drop_index(
        "ix_exportacion_lista_fecha_cuenta", table_name="evento_exportacion_lista_control"
    )
    op.drop_table("evento_exportacion_lista_control")
