"""Completa el expediente anual y la trazabilidad de reinicios de PIN.

Revision ID: 0012_expediente_personas
"""

import sqlalchemy as sa
from alembic import op

revision = "0012_expediente_personas"
down_revision = "0011_ciclo_menu_panea"
branch_labels = None
depends_on = None


def upgrade() -> None:
    # La plataforma opera un unico horario diurno. Se normalizan importaciones
    # previas antes de impedir cualquier valor historico numerico.
    op.execute("UPDATE matricula SET turno = 'diurno' WHERE turno <> 'diurno'")
    op.create_check_constraint("turno_matricula_diurno", "matricula", "turno = 'diurno'")

    op.create_table(
        "evento_credencial_portal",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("cuenta_administrativa_id", sa.Integer(), nullable=True),
        sa.Column("tipo", sa.String(24), nullable=False),
        sa.Column(
            "creado_en",
            sa.DateTime(timezone=True),
            nullable=False,
            server_default=sa.text("CURRENT_TIMESTAMP"),
        ),
        sa.CheckConstraint(
            "tipo IN ('creacion','reinicio_individual','reinicio_masivo')",
            name="tipo_evento_credencial_portal",
        ),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.ForeignKeyConstraint(["cuenta_administrativa_id"], ["cuenta_administrativa.id"]),
    )
    op.create_index(
        "ix_evento_credencial_portal_persona_fecha",
        "evento_credencial_portal",
        ["persona_id", "creado_en"],
    )


def downgrade() -> None:
    op.drop_index("ix_evento_credencial_portal_persona_fecha", table_name="evento_credencial_portal")
    op.drop_table("evento_credencial_portal")
    op.drop_constraint("turno_matricula_diurno", "matricula", type_="check")
