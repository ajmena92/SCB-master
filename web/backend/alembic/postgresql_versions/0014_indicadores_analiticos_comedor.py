"""Persistencia de indicadores diarios calculados con pandas."""

import sqlalchemy as sa
from alembic import op

revision = "0014_indicadores_comedor"
down_revision = "0013_referencia_publica_persona"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "indicador_analitico_comedor",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), sa.ForeignKey("persona.id"), nullable=False),
        sa.Column("fecha_corte", sa.Date(), nullable=False),
        sa.Column("dias_observados", sa.Integer(), nullable=False),
        sa.Column("dias_presentes", sa.Integer(), nullable=False),
        sa.Column("porcentaje_asistencia", sa.Numeric(5, 2), nullable=False),
        sa.Column("consumos_comedor", sa.Integer(), nullable=False, server_default="0"),
        sa.Column("consumos_tiquete", sa.Integer(), nullable=False, server_default="0"),
        sa.Column("senal", sa.String(64), nullable=False),
        sa.Column("generado_en", sa.DateTime(timezone=True), nullable=False, server_default=sa.text("now()")),
        sa.UniqueConstraint("persona_id", "fecha_corte"),
        sa.CheckConstraint("dias_observados >= 0 AND dias_presentes >= 0", name="conteos_indicador_no_negativos"),
        sa.CheckConstraint("porcentaje_asistencia >= 0 AND porcentaje_asistencia <= 100", name="porcentaje_indicador_valido"),
    )
    op.create_index("ix_indicador_analitico_corte_senal", "indicador_analitico_comedor", ["fecha_corte", "senal"])


def downgrade() -> None:
    op.drop_index("ix_indicador_analitico_corte_senal", table_name="indicador_analitico_comedor")
    op.drop_table("indicador_analitico_comedor")
