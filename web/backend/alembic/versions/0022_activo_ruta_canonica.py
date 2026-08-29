"""Completa el campo activo del catálogo canónico de rutas."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0022_activo_ruta"
down_revision: Union[str, None] = "0021_reconciliacion_fisica"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    columnas = {
        fila[0]
        for fila in conexion.execute(
            sa.text(
                "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS "
                "WHERE TABLE_SCHEMA='transporte' AND TABLE_NAME='ruta'"
            )
        )
    }
    if "activo" not in columnas:
        conexion.execute(
            sa.text(
                "ALTER TABLE transporte.ruta ADD activo bit NOT NULL "
                "CONSTRAINT DF_transporte_ruta_activo_0022 DEFAULT 1"
            )
        )


def downgrade() -> None:
    raise RuntimeError("La migración de rutas no admite reversión destructiva")
