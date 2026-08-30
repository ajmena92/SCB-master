"""Modelo inicial PostgreSQL.

Revision ID: 0001_postgresql_inicial
"""

import aplicacion.modelos  # noqa: F401
from alembic import op
from aplicacion.nucleo.modelos_base import BaseDeclarativa

revision = "0001_postgresql_inicial"
down_revision = None
branch_labels = None
depends_on = None


def upgrade() -> None:
    BaseDeclarativa.metadata.create_all(bind=op.get_bind(), checkfirst=False)
    op.execute(
        "INSERT INTO tarifa (tipo_persona, monto, fecha_inicio, fecha_fin) "
        "VALUES ('estudiante', 700, DATE '2026-01-01', NULL), "
        "('profesor', 1000, DATE '2026-01-01', NULL)"
    )


def downgrade() -> None:
    BaseDeclarativa.metadata.drop_all(bind=op.get_bind(), checkfirst=False)
