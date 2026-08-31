"""Agrega trazabilidad operativa a la captura web de comedor.

Revision ID: 0006_captura_comedor
"""

import sqlalchemy as sa

from alembic import op

revision = "0006_captura_comedor"
down_revision = "0005_normaliza_checks"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column(
        "ingreso_comedor",
        sa.Column(
            "marca_transporte_existente", sa.Boolean(), nullable=False, server_default=sa.false()
        ),
    )
    op.add_column("ingreso_comedor", sa.Column("advertencia", sa.String(120), nullable=True))
    op.create_table(
        "evento_operacion_comedor",
        sa.Column("id", sa.Integer(), nullable=False),
        sa.Column("fecha_evento", sa.DateTime(timezone=True), nullable=False),
        sa.Column("fecha_operativa", sa.Date(), nullable=False),
        sa.Column("persona_id", sa.Integer(), nullable=True),
        sa.Column("codigo_capturado", sa.String(40), nullable=False),
        sa.Column("resultado", sa.String(24), nullable=False),
        sa.Column("motivo", sa.String(240), nullable=True),
        sa.Column("operador_id", sa.Integer(), nullable=False),
        sa.Column("advertencia", sa.Boolean(), nullable=False),
        sa.Column("duracion_ms", sa.Integer(), nullable=True),
        sa.CheckConstraint(
            "resultado IN ('aceptado','duplicado','no_encontrado','sin_reserva',"
            "'sin_tiquete','rechazado','error')",
            name="ck_evento_operacion_comedor_resultado_evento_operacion_comedor",
        ),
        sa.CheckConstraint(
            "duracion_ms IS NULL OR duracion_ms >= 0",
            name="ck_evento_operacion_comedor_duracion_evento_comedor",
        ),
        sa.ForeignKeyConstraint(
            ["operador_id"],
            ["cuenta_administrativa.id"],
            name="fk_evento_operacion_comedor_operador_id_cuenta_administrativa",
        ),
        sa.ForeignKeyConstraint(
            ["persona_id"],
            ["persona.id"],
            ondelete="SET NULL",
            name="fk_evento_operacion_comedor_persona_id_persona",
        ),
        sa.PrimaryKeyConstraint("id", name="pk_evento_operacion_comedor"),
    )
    op.create_index(
        "ix_evento_operacion_comedor_fecha_operativa",
        "evento_operacion_comedor",
        ["fecha_operativa"],
    )
    op.create_index(
        "ix_evento_operacion_comedor_fecha_evento",
        "evento_operacion_comedor",
        ["fecha_evento"],
    )
    op.alter_column("ingreso_comedor", "marca_transporte_existente", server_default=None)


def downgrade() -> None:
    op.drop_index("ix_evento_operacion_comedor_fecha_evento", table_name="evento_operacion_comedor")
    op.drop_index(
        "ix_evento_operacion_comedor_fecha_operativa", table_name="evento_operacion_comedor"
    )
    op.drop_table("evento_operacion_comedor")
    op.drop_column("ingreso_comedor", "advertencia")
    op.drop_column("ingreso_comedor", "marca_transporte_existente")
