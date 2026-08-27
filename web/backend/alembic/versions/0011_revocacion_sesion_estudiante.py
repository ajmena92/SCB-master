"""Añade trazabilidad de revocación a las sesiones estudiantiles."""
from typing import Sequence, Union

from alembic import op

revision: str = "0011_revocacion_sesion_est"
down_revision: Union[str, None] = "0010_normaliza_estudiante"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

def upgrade() -> None:
    op.execute("IF COL_LENGTH(N'identidad.sesion_estudiante', N'fecha_revocacion') IS NULL ALTER TABLE identidad.sesion_estudiante ADD fecha_revocacion datetime2(3) NULL")

def downgrade() -> None:
    op.execute("IF COL_LENGTH(N'identidad.sesion_estudiante', N'fecha_revocacion') IS NOT NULL ALTER TABLE identidad.sesion_estudiante DROP COLUMN fecha_revocacion")
