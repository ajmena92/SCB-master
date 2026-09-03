"""Persiste los límites de autenticación en PostgreSQL.

Revision ID: 0018_control_intentos_autenticacion
"""

import sqlalchemy as sa
from alembic import op

revision = "0018_control_intentos_autenticacion"
down_revision = "0017_configuracion_institucional"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "intento_autenticacion",
        sa.Column("identificador_hash", sa.String(64), primary_key=True),
        sa.Column("intentos_fallidos", sa.Integer(), nullable=False, server_default="0"),
        sa.Column("bloqueado_hasta", sa.DateTime(timezone=True), nullable=True),
        sa.Column(
            "actualizado_en",
            sa.DateTime(timezone=True),
            nullable=False,
            server_default=sa.text("now()"),
        ),
        sa.CheckConstraint("intentos_fallidos >= 0", name="intentos_fallidos_no_negativos"),
    )
    op.create_index(
        "ix_intento_autenticacion_bloqueado_hasta",
        "intento_autenticacion",
        ["bloqueado_hasta"],
    )


def downgrade() -> None:
    op.drop_index("ix_intento_autenticacion_bloqueado_hasta", table_name="intento_autenticacion")
    op.drop_table("intento_autenticacion")
