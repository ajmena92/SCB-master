"""Registra diferencias detectadas durante el corte definitivo del comedor."""

from typing import Sequence, Union

import sqlalchemy as sa
from alembic import op

revision: str = "0031_reconciliacion_corte_comedor"
down_revision: Union[str, None] = "0030_politicas_y_auditoria_operacion"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.get_bind().execute(sa.text("""
        IF OBJECT_ID(N'comedor.reconciliacion_migracion', N'U') IS NULL
        BEGIN
            CREATE TABLE comedor.reconciliacion_migracion(
                id_reconciliacion BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_comedor_reconciliacion PRIMARY KEY,
                tipo VARCHAR(50) NOT NULL,
                clave VARCHAR(200) NOT NULL,
                detalle NVARCHAR(1000) NOT NULL,
                origen VARCHAR(40) NOT NULL CONSTRAINT DF_comedor_reconciliacion_origen DEFAULT 'corte_web',
                resuelto BIT NOT NULL CONSTRAINT DF_comedor_reconciliacion_resuelto DEFAULT 0,
                creado_en DATETIME2(3) NOT NULL CONSTRAINT DF_comedor_reconciliacion_creado DEFAULT SYSUTCDATETIME(),
                resuelto_en DATETIME2(3) NULL,
                resuelto_por INT NULL
            );
            CREATE UNIQUE INDEX UX_comedor_reconciliacion_clave ON comedor.reconciliacion_migracion(tipo, clave);
        END;
    """))


def downgrade() -> None:
    raise RuntimeError("La reconciliación del corte no admite reversión destructiva")
