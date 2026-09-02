"""Recupera el calendario institucional del menú.

El modelo preserva el contrato previo: una fecha única y su estado habilitado.
"""

import sqlalchemy as sa
from alembic import op

revision = "0009_calendario_menu"
down_revision = "0008_usuarios_administrativos"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "calendario_menu",
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("habilitado", sa.Boolean(), nullable=False, server_default=sa.true()),
        sa.PrimaryKeyConstraint("fecha", name="pk_calendario_menu"),
    )
    op.alter_column("calendario_menu", "habilitado", server_default=None)


def downgrade() -> None:
    op.drop_table("calendario_menu")
