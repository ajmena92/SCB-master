"""Conserva el cambio obligatorio de PIN en la sesion."""

import sqlalchemy as sa

from alembic import op

revision = "0002_sesion_cambio_obligatorio"
down_revision = "0001_postgresql_inicial"
branch_labels = None
depends_on = None


def _columnas() -> set[str]:
    return {c["name"] for c in sa.inspect(op.get_bind()).get_columns("sesion_acceso")}


def upgrade() -> None:
    if "cambio_obligatorio" not in _columnas():
        op.add_column(
            "sesion_acceso",
            sa.Column(
                "cambio_obligatorio",
                sa.Boolean(),
                nullable=False,
                server_default=sa.false(),
            ),
        )
    op.execute(sa.delete(sa.table("sesion_acceso")))


def downgrade() -> None:
    if "cambio_obligatorio" in _columnas():
        op.drop_column("sesion_acceso", "cambio_obligatorio")
