"""Crea el almacenamiento privado de fotografías del padrón."""

import sqlalchemy as sa

from alembic import op

revision = "0020_fotografia_persona"
down_revision = "0019_trabajos_importacion"
branch_labels = None
depends_on = None


def upgrade() -> None:
    inspector = sa.inspect(op.get_bind())
    if inspector.has_table("fotografia_persona"):
        columnas = {columna["name"] for columna in inspector.get_columns("fotografia_persona")}
        requeridas = {"id", "persona_id", "contenido", "tipo_contenido"}
        faltantes = requeridas - columnas
        if faltantes:
            raise RuntimeError(
                "fotografia_persona ya existe, pero no coincide con el esquema esperado: "
                + ", ".join(sorted(faltantes))
            )
        # Algunas restauraciones históricas ya contenían esta tabla, pero no
        # registraron la revisión Alembic. Su estructura es compatible y la
        # revisión puede continuar sin intentar crearla de nuevo.
        return
    op.create_table(
        "fotografia_persona",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column(
            "persona_id",
            sa.Integer(),
            sa.ForeignKey("persona.id", ondelete="CASCADE"),
            nullable=False,
            unique=True,
        ),
        sa.Column("contenido", sa.LargeBinary(), nullable=False),
        sa.Column("tipo_contenido", sa.String(length=80), nullable=False),
    )


def downgrade() -> None:
    op.drop_table("fotografia_persona")
