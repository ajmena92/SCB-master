"""Amplia nombres de menu sin truncar el origen institucional."""

import sqlalchemy as sa

from alembic import op

revision = "0003_amplia_nombres_menu"
down_revision = "0002_sesion_cambio_obligatorio"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.alter_column(
        "plantilla_menu",
        "nombre",
        existing_type=sa.String(length=120),
        type_=sa.String(length=180),
        existing_nullable=False,
    )
    op.alter_column(
        "publicacion_menu",
        "nombre",
        existing_type=sa.String(length=120),
        type_=sa.String(length=180),
        existing_nullable=False,
    )


def downgrade() -> None:
    conexion = op.get_bind()
    for tabla in ("plantilla_menu", "publicacion_menu"):
        maximo = conexion.execute(
            sa.text(f"SELECT COALESCE(MAX(length(nombre)), 0) FROM {tabla}")
        ).scalar_one()
        if maximo > 120:
            raise RuntimeError(
                f"No se puede reducir {tabla}.nombre: existen valores mayores de 120 caracteres"
            )
    op.alter_column(
        "publicacion_menu",
        "nombre",
        existing_type=sa.String(length=180),
        type_=sa.String(length=120),
        existing_nullable=False,
    )
    op.alter_column(
        "plantilla_menu",
        "nombre",
        existing_type=sa.String(length=180),
        type_=sa.String(length=120),
        existing_nullable=False,
    )
