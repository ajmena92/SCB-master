"""Crea almacenamiento canónico de fotografías y resumen operativo."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0004_fotos_dashboard"
down_revision: Union[str, None] = "0003_parametros_calendario"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None


def upgrade() -> None:
    op.create_table(
        "fotografia",
        sa.Column("id_fotografia", sa.Integer, primary_key=True),
        sa.Column(
            "id_estudiante",
            sa.Integer,
            sa.ForeignKey("estudiantes.estudiante.id_estudiante"),
            nullable=False,
        ),
        sa.Column("contenido", sa.LargeBinary, nullable=False),
        sa.Column("tipo_contenido", sa.String(80), nullable=False),
        sa.UniqueConstraint("id_estudiante", name="uq_fotografia_estudiante"),
        schema="estudiantes",
    )
    op.create_table(
        "resumen",
        sa.Column("id_resumen", sa.Integer, primary_key=True),
        sa.Column("fecha", sa.DateTime, nullable=False),
        sa.Column("estudiantes", sa.Integer, nullable=False, server_default="0"),
        sa.Column("confirmaciones", sa.Integer, nullable=False, server_default="0"),
        sa.Column("cancelaciones", sa.Integer, nullable=False, server_default="0"),
        sa.UniqueConstraint("fecha", name="uq_resumen_fecha"),
        schema="reportes",
    )


def downgrade() -> None:
    op.drop_table("resumen", schema="reportes")
    op.drop_table("fotografia", schema="estudiantes")
