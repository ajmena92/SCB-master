"""Crea parámetros operativos y calendario canónicos."""
from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0003_parametros_calendario"
down_revision: Union[str, None] = "0002_dominios_web"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table("parametro", sa.Column("id_parametro", sa.Integer, primary_key=True),
                    sa.Column("minutos_aviso_previo", sa.Integer, nullable=False),
                    sa.Column("actualizado_en", sa.DateTime, nullable=False,
                              server_default=sa.text("SYSUTCDATETIME()")), schema="comedor")
    op.create_table("calendario", sa.Column("id_calendario", sa.Integer, primary_key=True),
                    sa.Column("fecha", sa.DateTime, nullable=False),
                    sa.Column("habilitado", sa.Boolean, nullable=False, server_default=sa.true()),
                    sa.UniqueConstraint("fecha", name="uq_calendario_fecha"), schema="menu")


def downgrade() -> None:
    op.drop_table("calendario", schema="menu")
    op.drop_table("parametro", schema="comedor")
