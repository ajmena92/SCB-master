"""Agrega una referencia pública opaca para enlaces de expedientes.

Revision ID: 0013_referencia_publica_persona
"""

import sqlalchemy as sa
from alembic import op

revision = "0013_referencia_publica_persona"
down_revision = "0012_expediente_personas"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column("persona", sa.Column("referencia_publica", sa.String(32), nullable=True))
    op.execute(
        "UPDATE persona SET referencia_publica = md5(random()::text || clock_timestamp()::text || id::text) "
        "WHERE referencia_publica IS NULL"
    )
    op.alter_column("persona", "referencia_publica", nullable=False)
    op.create_index("ix_persona_referencia_publica", "persona", ["referencia_publica"], unique=True)


def downgrade() -> None:
    op.drop_index("ix_persona_referencia_publica", table_name="persona")
    op.drop_column("persona", "referencia_publica")
