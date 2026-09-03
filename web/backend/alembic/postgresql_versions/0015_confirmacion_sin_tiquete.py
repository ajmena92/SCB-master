"""Registra confirmaciones de comedor sin tiquete y crea el horario único."""

from alembic import op
import sqlalchemy as sa


revision = "0016_confirmacion_sin_tiquete"
down_revision = "0015_elimina_codigo_persona"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.add_column(
        "reserva_comedor",
        sa.Column("sin_tiquete", sa.Boolean(), nullable=False, server_default=sa.false()),
    )
    op.execute(
        """INSERT INTO horario_reserva(turno, hora_limite) VALUES ('general', '09:40')
        ON CONFLICT (turno) DO NOTHING"""
    )
    op.alter_column("reserva_comedor", "sin_tiquete", server_default=None)


def downgrade() -> None:
    op.drop_column("reserva_comedor", "sin_tiquete")
