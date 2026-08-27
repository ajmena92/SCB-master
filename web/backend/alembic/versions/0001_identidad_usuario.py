"""Crea la tabla inicial canónica de identidad."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0001_identidad_usuario"
down_revision: Union[str, None] = None
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.execute("IF SCHEMA_ID(N'identidad') IS NULL EXEC(N'CREATE SCHEMA identidad')")
    op.create_table(
        "usuario",
        sa.Column("id_usuario", sa.Integer(), autoincrement=True, nullable=False),
        sa.Column("nombre_usuario", sa.String(length=100), nullable=False),
        sa.Column("hash_contrasena", sa.String(length=255), nullable=False),
        sa.Column("activo", sa.Boolean(), nullable=False, server_default=sa.true()),
        sa.Column("fecha_creacion", sa.DateTime(), nullable=False,
                  server_default=sa.text("SYSUTCDATETIME()")),
        sa.Column("fecha_actualizacion", sa.DateTime(), nullable=False,
                  server_default=sa.text("SYSUTCDATETIME()")),
        sa.PrimaryKeyConstraint("id_usuario", name="pk_usuario"),
        sa.UniqueConstraint("nombre_usuario", name="uq_usuario_nombre_usuario"),
        schema="identidad",
    )


def downgrade() -> None:
    op.drop_table("usuario", schema="identidad")
