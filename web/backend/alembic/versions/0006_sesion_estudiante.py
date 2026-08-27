"""Crea sesiones independientes para estudiantes."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0006_sesion_estudiante"
down_revision: Union[str, None] = "0005_pin_estudiantes"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "sesion_estudiante",
        sa.Column("id_sesion", sa.String(100), primary_key=True),
        sa.Column(
            "id_usuario",
            sa.Integer,
            sa.ForeignKey("estudiantes.estudiante.id_estudiante"),
            nullable=False,
        ),
        sa.Column("secreto_hash", sa.String(64), nullable=False),
        sa.Column("expira_en", sa.DateTime, nullable=False),
        sa.Column("csrf_hash", sa.String(64), nullable=True),
        sa.Column("revocada", sa.Boolean, nullable=False, server_default=sa.false()),
        schema="identidad",
    )


def downgrade() -> None:
    op.drop_table("sesion_estudiante", schema="identidad")
