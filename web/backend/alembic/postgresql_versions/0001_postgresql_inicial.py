"""Modelo inicial PostgreSQL congelado y ajeno a la metadata futura.

Revision ID: 0001_postgresql_inicial
"""

import sqlalchemy as sa

from alembic import op

revision = "0001_postgresql_inicial"
down_revision = None
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.create_table(
        "persona",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("codigo", sa.String(10), nullable=False),
        sa.Column("cedula", sa.String(32), nullable=True),
        sa.Column("nombres", sa.String(180), nullable=False),
        sa.Column("tipo", sa.String(12), nullable=False),
        sa.Column("activo", sa.Boolean(), nullable=False),
        sa.CheckConstraint("tipo IN ('estudiante','profesor')", name="tipo_persona"),
        sa.UniqueConstraint("cedula"),
    )
    op.create_index("ix_persona_codigo", "persona", ["codigo"], unique=True)
    op.create_table(
        "cuenta_administrativa",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("usuario", sa.String(80), nullable=False),
        sa.Column("contrasena_hash", sa.String(255), nullable=False),
        sa.Column("rol", sa.String(16), nullable=False),
        sa.Column("activo", sa.Boolean(), nullable=False),
        sa.CheckConstraint("rol IN ('administrador','operador')", name="rol_administrativo"),
        sa.UniqueConstraint("usuario"),
    )
    op.create_table(
        "anio_lectivo",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("anio", sa.Integer(), nullable=False),
        sa.Column("vigente", sa.Boolean(), nullable=False),
        sa.CheckConstraint("anio >= 2000 AND anio <= 2200", name="rango_anio"),
        sa.UniqueConstraint("anio"),
    )
    op.create_index(
        "uq_anio_lectivo_vigente",
        "anio_lectivo",
        ["vigente"],
        unique=True,
        postgresql_where=sa.text("vigente"),
    )
    op.create_table(
        "ruta",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("nombre", sa.String(120), nullable=False),
        sa.Column("activo", sa.Boolean(), nullable=False),
        sa.UniqueConstraint("nombre"),
    )
    op.create_table(
        "horario_reserva",
        sa.Column("turno", sa.String(24), primary_key=True),
        sa.Column("hora_limite", sa.String(5), nullable=False),
    )
    op.create_table(
        "plantilla_menu",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("nombre", sa.String(120), nullable=False),
        sa.Column("activo", sa.Boolean(), nullable=False),
        sa.UniqueConstraint("nombre"),
    )
    op.create_table(
        "publicacion_menu",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("nombre", sa.String(120), nullable=False),
        sa.UniqueConstraint("fecha"),
    )
    op.create_table(
        "tarifa",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("tipo_persona", sa.String(12), nullable=False),
        sa.Column("monto", sa.Numeric(10, 2), nullable=False),
        sa.Column("fecha_inicio", sa.Date(), nullable=False),
        sa.Column("fecha_fin", sa.Date(), nullable=True),
        sa.CheckConstraint("monto >= 0", name="monto_tarifa"),
        sa.CheckConstraint(
            "fecha_fin IS NULL OR fecha_fin >= fecha_inicio", name="vigencia_tarifa"
        ),
    )
    op.create_table(
        "lote_importacion",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("huella", sa.String(64), nullable=False),
        sa.Column("estado", sa.String(16), nullable=False),
        sa.Column("resumen", sa.String(1000), nullable=False),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False),
        sa.UniqueConstraint("huella"),
    )
    op.create_table(
        "credencial_portal",
        sa.Column("persona_id", sa.Integer(), primary_key=True),
        sa.Column("pin_hash", sa.String(255), nullable=False),
        sa.Column("cambio_obligatorio", sa.Boolean(), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
    )
    op.create_table(
        "sesion_acceso",
        sa.Column("token_hash", sa.String(64), primary_key=True),
        sa.Column("tipo", sa.String(16), nullable=False),
        sa.Column("persona_id", sa.Integer(), nullable=True),
        sa.Column("cuenta_id", sa.Integer(), nullable=True),
        sa.Column("expira_en", sa.DateTime(timezone=True), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.ForeignKeyConstraint(["cuenta_id"], ["cuenta_administrativa.id"]),
    )
    op.create_table(
        "matricula",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("anio_lectivo_id", sa.Integer(), nullable=False),
        sa.Column("seccion", sa.String(40), nullable=False),
        sa.Column("turno", sa.String(24), nullable=False),
        sa.Column("becado", sa.Boolean(), nullable=False),
        sa.Column("estado", sa.String(16), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.ForeignKeyConstraint(["anio_lectivo_id"], ["anio_lectivo.id"]),
        sa.UniqueConstraint("persona_id", "anio_lectivo_id"),
    )
    op.create_index("ix_matricula_persona_id", "matricula", ["persona_id"])
    op.create_index("ix_matricula_anio_lectivo_id", "matricula", ["anio_lectivo_id"])
    op.create_table(
        "asignacion_ruta",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("matricula_id", sa.Integer(), nullable=False),
        sa.Column("ruta_id", sa.Integer(), nullable=False),
        sa.Column("fecha_inicio", sa.Date(), nullable=False),
        sa.Column("fecha_fin", sa.Date(), nullable=True),
        sa.CheckConstraint("fecha_fin IS NULL OR fecha_fin >= fecha_inicio", name="vigencia_ruta"),
        sa.ForeignKeyConstraint(["matricula_id"], ["matricula.id"]),
        sa.ForeignKeyConstraint(["ruta_id"], ["ruta.id"]),
    )
    op.create_index("ix_asignacion_ruta_matricula_id", "asignacion_ruta", ["matricula_id"])
    op.create_table(
        "componente_menu",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("plantilla_id", sa.Integer(), nullable=False),
        sa.Column("nombre", sa.String(180), nullable=False),
        sa.Column("orden", sa.Integer(), nullable=False),
        sa.ForeignKeyConstraint(["plantilla_id"], ["plantilla_menu.id"], ondelete="CASCADE"),
        sa.UniqueConstraint("plantilla_id", "orden"),
    )
    op.create_table(
        "componente_publicado",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("publicacion_id", sa.Integer(), nullable=False),
        sa.Column("nombre", sa.String(180), nullable=False),
        sa.Column("orden", sa.Integer(), nullable=False),
        sa.ForeignKeyConstraint(["publicacion_id"], ["publicacion_menu.id"], ondelete="CASCADE"),
        sa.UniqueConstraint("publicacion_id", "orden"),
    )
    op.create_table(
        "cuenta_tiquete",
        sa.Column("persona_id", sa.Integer(), primary_key=True),
        sa.Column("saldo", sa.Integer(), nullable=False),
        sa.Column("reservados", sa.Integer(), nullable=False),
        sa.CheckConstraint("saldo >= 0", name="saldo_no_negativo"),
        sa.CheckConstraint("reservados >= 0", name="reservados_no_negativo"),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
    )
    op.create_table(
        "movimiento_tiquete",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("tipo", sa.String(20), nullable=False),
        sa.Column("cantidad", sa.Integer(), nullable=False),
        sa.Column("saldo_resultante", sa.Integer(), nullable=False),
        sa.Column("referencia", sa.String(80), nullable=True),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
    )
    op.create_index("ix_movimiento_tiquete_persona_id", "movimiento_tiquete", ["persona_id"])
    op.create_table(
        "venta_tiquete",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("tarifa_id", sa.Integer(), nullable=False),
        sa.Column("cantidad", sa.Integer(), nullable=False),
        sa.Column("tarifa_aplicada", sa.Numeric(10, 2), nullable=False),
        sa.Column("total", sa.Numeric(12, 2), nullable=False),
        sa.Column("medio_pago", sa.String(30), nullable=False),
        sa.Column("operador_id", sa.Integer(), nullable=False),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.ForeignKeyConstraint(["tarifa_id"], ["tarifa.id"]),
        sa.ForeignKeyConstraint(["operador_id"], ["cuenta_administrativa.id"]),
    )
    op.create_table(
        "reserva_comedor",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("estado", sa.String(16), nullable=False),
        sa.Column("tiquete_inmovilizado", sa.Boolean(), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.UniqueConstraint("persona_id", "fecha"),
    )
    op.create_table(
        "autorizacion_comedor",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("decision", sa.String(12), nullable=False),
        sa.Column("motivo", sa.String(240), nullable=True),
        sa.Column("operador_id", sa.Integer(), nullable=False),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.ForeignKeyConstraint(["operador_id"], ["cuenta_administrativa.id"]),
        sa.UniqueConstraint("persona_id", "fecha"),
    )
    op.create_table(
        "ingreso_comedor",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("persona_id", sa.Integer(), nullable=False),
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("reserva_id", sa.Integer(), nullable=True),
        sa.Column("autorizacion_id", sa.Integer(), nullable=True),
        sa.Column("modalidad", sa.String(24), nullable=False),
        sa.Column("consumio_tiquete", sa.Boolean(), nullable=False),
        sa.Column("operador_id", sa.Integer(), nullable=False),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False),
        sa.ForeignKeyConstraint(["persona_id"], ["persona.id"]),
        sa.ForeignKeyConstraint(["reserva_id"], ["reserva_comedor.id"]),
        sa.ForeignKeyConstraint(["autorizacion_id"], ["autorizacion_comedor.id"]),
        sa.ForeignKeyConstraint(["operador_id"], ["cuenta_administrativa.id"]),
        sa.UniqueConstraint("persona_id", "fecha"),
    )
    op.create_table(
        "marca_transporte",
        sa.Column("id", sa.Integer(), primary_key=True),
        sa.Column("matricula_id", sa.Integer(), nullable=False),
        sa.Column("ruta_id", sa.Integer(), nullable=False),
        sa.Column("fecha", sa.Date(), nullable=False),
        sa.Column("operador_id", sa.Integer(), nullable=False),
        sa.Column("creado_en", sa.DateTime(timezone=True), nullable=False),
        sa.ForeignKeyConstraint(["matricula_id"], ["matricula.id"]),
        sa.ForeignKeyConstraint(["ruta_id"], ["ruta.id"]),
        sa.ForeignKeyConstraint(["operador_id"], ["cuenta_administrativa.id"]),
        sa.UniqueConstraint("matricula_id", "fecha"),
    )
    op.execute(
        "INSERT INTO tarifa (tipo_persona, monto, fecha_inicio, fecha_fin) "
        "VALUES ('estudiante', 700, DATE '2026-01-01', NULL), "
        "('profesor', 1000, DATE '2026-01-01', NULL)"
    )


def downgrade() -> None:
    for tabla in (
        "marca_transporte",
        "ingreso_comedor",
        "autorizacion_comedor",
        "reserva_comedor",
        "venta_tiquete",
        "movimiento_tiquete",
        "cuenta_tiquete",
        "componente_publicado",
        "componente_menu",
        "asignacion_ruta",
        "matricula",
        "sesion_acceso",
        "credencial_portal",
        "lote_importacion",
        "tarifa",
        "publicacion_menu",
        "plantilla_menu",
        "horario_reserva",
        "ruta",
        "anio_lectivo",
        "cuenta_administrativa",
        "persona",
    ):
        op.drop_table(tabla)
