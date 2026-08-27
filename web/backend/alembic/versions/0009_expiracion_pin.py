"""Añade vencimiento explícito para PINes temporales."""
from typing import Sequence, Union

from alembic import op

revision: str = "0009_expiracion_pin"
down_revision: Union[str, None] = "0008_sesion_administrativa"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

def upgrade() -> None:
    op.execute("IF COL_LENGTH(N'estudiantes.estudiante', N'fecha_expiracion_pin') IS NULL ALTER TABLE estudiantes.estudiante ADD fecha_expiracion_pin datetime2(3) NULL")

def downgrade() -> None:
    op.execute("IF COL_LENGTH(N'estudiantes.estudiante', N'fecha_expiracion_pin') IS NOT NULL ALTER TABLE estudiantes.estudiante DROP COLUMN fecha_expiracion_pin")
