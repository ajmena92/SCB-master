"""Añade cola durable para las confirmaciones de importación.

Revision ID: 0019_trabajos_importacion
"""

import sqlalchemy as sa

from alembic import op

revision = "0019_trabajos_importacion"
down_revision = "0018_control_intentos_autenticacion"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "trabajo_importacion",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("huella", sa.String(64), nullable=False, unique=True),
        sa.Column(
            "cuenta_solicitante_id",
            sa.Integer(),
            sa.ForeignKey("cuenta_administrativa.id"),
            nullable=False,
        ),
        sa.Column("entrada_json", sa.Text(), nullable=False),
        sa.Column("resumen_json", sa.String(1000), nullable=False),
        sa.Column("estado", sa.String(16), nullable=False, server_default="pendiente"),
        sa.Column("error", sa.String(300), nullable=True),
        sa.Column("resultado_cifrado", sa.Text(), nullable=True),
        sa.Column("resultado_entregado", sa.Boolean(), nullable=False, server_default=sa.false()),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.Column("iniciado_en", sa.DateTime(timezone=True), nullable=True),
        sa.Column("finalizado_en", sa.DateTime(timezone=True), nullable=True),
        sa.CheckConstraint(
            "estado IN ('pendiente','ejecutando','completado','fallido','cancelado')",
            name="estado_trabajo_importacion",
        ),
    )
    op.create_index(
        "ix_trabajo_importacion_estado_creado",
        "trabajo_importacion",
        ["estado", "creado_en"],
    )


def downgrade() -> None:
    op.drop_index("ix_trabajo_importacion_estado_creado", table_name="trabajo_importacion")
    op.drop_table("trabajo_importacion")
