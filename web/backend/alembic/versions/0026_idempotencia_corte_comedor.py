"""Endurece la idempotencia de recargas del comedor."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op


revision: str = "0026_idempotencia_corte_comedor"
down_revision: Union[str, None] = "0025_sustituciones_menu"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    conexion = op.get_bind()
    conexion.execute(
        sa.text(
            """
            IF OBJECT_ID(N'comedor.movimiento_tiquetes', N'U') IS NULL
                THROW 50028, 'No existe comedor.movimiento_tiquetes para endurecer idempotencia', 1;
            IF COL_LENGTH(N'comedor.movimiento_tiquetes', N'huella_idempotencia') IS NULL
                ALTER TABLE comedor.movimiento_tiquetes ADD huella_idempotencia VARBINARY(32) NULL;
            UPDATE m
            SET huella_idempotencia = HASHBYTES(
                'SHA2_256',
                CONCAT(m.id_cuenta, '|', m.tipo, '|', m.cantidad, '|', ISNULL(m.concepto, ''), '|', ISNULL(m.creado_por, 0))
            )
            FROM comedor.movimiento_tiquetes m
            WHERE m.huella_idempotencia IS NULL AND m.tipo = 'recarga';
            """
        )
    )


def downgrade() -> None:
    raise RuntimeError("La migración de idempotencia no admite reversión destructiva")
