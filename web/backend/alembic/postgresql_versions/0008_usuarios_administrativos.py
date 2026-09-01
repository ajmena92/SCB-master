"""Vincula cuentas con profesores y agrega permisos administrativos.

Revision ID: 0008_usuarios_administrativos
"""

import sqlalchemy as sa

from alembic import op

revision = "0008_usuarios_administrativos"
down_revision = "0007_colores_rutas"
branch_labels = None
depends_on = None

PERMISOS_CONGELADOS = (
    ("dashboard.leer", "Consultar inicio", "Indicadores generales del sistema", "Inicio"),
    ("comedor.operar", "Operar comedor", "Capturas y autorizaciones de comedor", "Comedor"),
    (
        "transporte.operar",
        "Operar transporte",
        "Consultar rutas y capturar marcas de transporte",
        "Rutas y transporte",
    ),
    (
        "rutas.administrar",
        "Administrar rutas",
        "Crear, editar y asignar rutas",
        "Rutas y transporte",
    ),
    ("personas.administrar", "Administrar personas", "Gestionar personas y matriculas", "Personas"),
    ("menu.administrar", "Administrar menu", "Gestionar el menu del dia", "Menu"),
    ("tiquetes.operar", "Operar tiquetes", "Consultar y vender tiquetes", "Tiquetes"),
    ("tarifas.administrar", "Administrar tarifas", "Crear tarifas de tiquetes", "Tiquetes"),
    ("reportes.leer", "Consultar reportes", "Consultar y exportar reportes", "Reportes"),
    (
        "importaciones.administrar",
        "Administrar anos e importacion",
        "Gestionar anos lectivos e importaciones",
        "Anos e importacion",
    ),
)


def upgrade() -> None:
    op.add_column("cuenta_administrativa", sa.Column("persona_id", sa.Integer(), nullable=True))
    op.add_column(
        "cuenta_administrativa",
        sa.Column(
            "cambio_contrasena_obligatorio",
            sa.Boolean(),
            nullable=False,
            server_default=sa.false(),
        ),
    )
    op.add_column(
        "cuenta_administrativa",
        sa.Column("vinculacion_pendiente", sa.Boolean(), nullable=False, server_default=sa.true()),
    )
    op.create_foreign_key(
        "fk_cuenta_administrativa_persona",
        "cuenta_administrativa",
        "persona",
        ["persona_id"],
        ["id"],
        ondelete="RESTRICT",
    )
    op.create_unique_constraint(
        "uq_cuenta_administrativa_persona_id", "cuenta_administrativa", ["persona_id"]
    )
    op.create_check_constraint(
        "estado_vinculacion_cuenta",
        "cuenta_administrativa",
        "(persona_id IS NULL AND vinculacion_pendiente) OR "
        "(persona_id IS NOT NULL AND NOT vinculacion_pendiente)",
    )
    op.execute("UPDATE cuenta_administrativa SET usuario = lower(trim(usuario))")
    op.create_index(
        "uq_cuenta_administrativa_usuario_lower",
        "cuenta_administrativa",
        [sa.text("lower(usuario)")],
        unique=True,
    )
    op.create_index("ix_cuenta_administrativa_persona_id", "cuenta_administrativa", ["persona_id"])

    op.create_table(
        "permiso_administrativo",
        sa.Column("clave", sa.String(80), primary_key=True),
        sa.Column("nombre", sa.String(120), nullable=False),
        sa.Column("descripcion", sa.String(300), nullable=False),
        sa.Column("modulo", sa.String(80), nullable=False),
    )
    op.create_index("ix_permiso_administrativo_modulo", "permiso_administrativo", ["modulo"])
    op.create_table(
        "cuenta_permiso",
        sa.Column("cuenta_id", sa.Integer(), primary_key=True),
        sa.Column("permiso_clave", sa.String(80), primary_key=True),
        sa.ForeignKeyConstraint(["cuenta_id"], ["cuenta_administrativa.id"], ondelete="CASCADE"),
        sa.ForeignKeyConstraint(
            ["permiso_clave"], ["permiso_administrativo.clave"], ondelete="CASCADE"
        ),
    )
    tabla = sa.table(
        "permiso_administrativo",
        sa.column("clave", sa.String),
        sa.column("nombre", sa.String),
        sa.column("descripcion", sa.String),
        sa.column("modulo", sa.String),
    )
    op.bulk_insert(
        tabla,
        [
            {"clave": clave, "nombre": nombre, "descripcion": descripcion, "modulo": modulo}
            for clave, nombre, descripcion, modulo in PERMISOS_CONGELADOS
        ],
    )

    op.execute(
        """
        CREATE FUNCTION validar_profesor_cuenta_administrativa() RETURNS trigger AS $$
        BEGIN
          IF NEW.persona_id IS NOT NULL AND NOT EXISTS (
            SELECT 1 FROM persona WHERE id = NEW.persona_id AND tipo = 'profesor' AND activo
          ) THEN
            RAISE EXCEPTION 'la cuenta administrativa requiere un profesor activo';
          END IF;
          RETURN NEW;
        END;
        $$ LANGUAGE plpgsql
        """
    )
    op.execute(
        """
        CREATE TRIGGER validar_profesor_cuenta_administrativa
        BEFORE INSERT OR UPDATE OF persona_id ON cuenta_administrativa
        FOR EACH ROW EXECUTE FUNCTION validar_profesor_cuenta_administrativa()
        """
    )
    op.execute(
        """
        CREATE FUNCTION proteger_profesor_cuenta_administrativa() RETURNS trigger AS $$
        BEGIN
          IF (NEW.tipo <> 'profesor' OR NOT NEW.activo) AND EXISTS (
            SELECT 1 FROM cuenta_administrativa WHERE persona_id = NEW.id
          ) THEN
            RAISE EXCEPTION 'un profesor con cuenta administrativa debe permanecer activo';
          END IF;
          RETURN NEW;
        END;
        $$ LANGUAGE plpgsql
        """
    )
    op.execute(
        """
        CREATE TRIGGER proteger_profesor_cuenta_administrativa
        BEFORE UPDATE OF tipo, activo ON persona
        FOR EACH ROW EXECUTE FUNCTION proteger_profesor_cuenta_administrativa()
        """
    )
    op.execute("DELETE FROM sesion_acceso WHERE tipo = 'administracion'")


def downgrade() -> None:
    op.execute("DROP TRIGGER IF EXISTS proteger_profesor_cuenta_administrativa ON persona")
    op.execute("DROP FUNCTION IF EXISTS proteger_profesor_cuenta_administrativa()")
    op.execute(
        "DROP TRIGGER IF EXISTS validar_profesor_cuenta_administrativa ON cuenta_administrativa"
    )
    op.execute("DROP FUNCTION IF EXISTS validar_profesor_cuenta_administrativa()")
    op.drop_table("cuenta_permiso")
    op.drop_index("ix_permiso_administrativo_modulo", table_name="permiso_administrativo")
    op.drop_table("permiso_administrativo")
    op.drop_index("ix_cuenta_administrativa_persona_id", table_name="cuenta_administrativa")
    op.drop_index("uq_cuenta_administrativa_usuario_lower", table_name="cuenta_administrativa")
    op.drop_constraint("estado_vinculacion_cuenta", "cuenta_administrativa", type_="check")
    op.drop_constraint(
        "uq_cuenta_administrativa_persona_id", "cuenta_administrativa", type_="unique"
    )
    op.drop_constraint(
        "fk_cuenta_administrativa_persona", "cuenta_administrativa", type_="foreignkey"
    )
    op.drop_column("cuenta_administrativa", "vinculacion_pendiente")
    op.drop_column("cuenta_administrativa", "cambio_contrasena_obligatorio")
    op.drop_column("cuenta_administrativa", "persona_id")
