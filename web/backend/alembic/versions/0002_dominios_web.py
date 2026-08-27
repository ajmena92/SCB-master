"""Crea las tablas mínimas de los dominios web canónicos."""

from typing import Sequence, Union

import sqlalchemy as sa

from alembic import op

revision: str = "0002_dominios_web"
down_revision: Union[str, None] = "0001_identidad_usuario"
branch_labels: Union[str, Sequence[str], None] = None
depends_on: Union[str, Sequence[str], None] = None

_ESQUEMAS = (
    "transporte",
    "estudiantes",
    "asistencia",
    "beneficios",
    "cuentas",
    "reportes",
    "importaciones",
    "auditoria",
    "menu",
    "comedor",
    "soporte",
)


def _esquemas() -> None:
    for esquema in _ESQUEMAS:
        op.execute(f"IF SCHEMA_ID(N'{esquema}') IS NULL EXEC(N'CREATE SCHEMA {esquema}')")


def upgrade() -> None:
    _esquemas()
    op.create_table(
        "ruta",
        sa.Column("id_ruta", sa.Integer, primary_key=True),
        sa.Column("nombre", sa.String(120), nullable=False),
        sa.Column("activa", sa.Boolean, nullable=False, server_default=sa.true()),
        schema="transporte",
    )
    op.create_table(
        "estudiante",
        sa.Column("id_estudiante", sa.Integer, primary_key=True),
        sa.Column("identificacion", sa.String(30), nullable=False),
        sa.Column("nombre_completo", sa.String(200), nullable=False),
        sa.Column("id_ruta", sa.Integer, sa.ForeignKey("transporte.ruta.id_ruta")),
        sa.UniqueConstraint("identificacion", name="uq_estudiante_identificacion"),
        schema="estudiantes",
    )
    op.create_table(
        "marca",
        sa.Column("id_marca", sa.Integer, primary_key=True),
        sa.Column(
            "id_estudiante",
            sa.Integer,
            sa.ForeignKey("estudiantes.estudiante.id_estudiante"),
            nullable=False,
        ),
        sa.Column("fecha_hora", sa.DateTime, nullable=False),
        sa.Column("estado", sa.String(30), nullable=False),
        schema="asistencia",
    )
    op.create_table(
        "beneficio",
        sa.Column("id_beneficio", sa.Integer, primary_key=True),
        sa.Column("nombre", sa.String(120), nullable=False),
        sa.Column("activo", sa.Boolean, nullable=False, server_default=sa.true()),
        schema="beneficios",
    )
    op.create_table(
        "beneficio_estudiante",
        sa.Column(
            "id_beneficio",
            sa.Integer,
            sa.ForeignKey("beneficios.beneficio.id_beneficio"),
            primary_key=True,
        ),
        sa.Column(
            "id_estudiante",
            sa.Integer,
            sa.ForeignKey("estudiantes.estudiante.id_estudiante"),
            primary_key=True,
        ),
        schema="beneficios",
    )
    op.create_table(
        "cuenta_estudiante",
        sa.Column("id_cuenta", sa.Integer, primary_key=True),
        sa.Column(
            "id_estudiante",
            sa.Integer,
            sa.ForeignKey("estudiantes.estudiante.id_estudiante"),
            nullable=False,
        ),
        sa.Column("saldo", sa.Integer, nullable=False, server_default="0"),
        sa.UniqueConstraint("id_estudiante", name="uq_cuenta_estudiante_estudiante"),
        schema="cuentas",
    )
    op.create_table(
        "reporte",
        sa.Column("id_reporte", sa.Integer, primary_key=True),
        sa.Column("tipo", sa.String(80), nullable=False),
        sa.Column("fecha_generacion", sa.DateTime, nullable=False),
        schema="reportes",
    )
    op.create_table(
        "lote",
        sa.Column("id_lote", sa.Integer, primary_key=True),
        sa.Column("nombre_archivo", sa.String(255), nullable=False),
        sa.Column("estado", sa.String(30), nullable=False),
        sa.Column("fecha_creacion", sa.DateTime, nullable=False),
        schema="importaciones",
    )
    op.create_table(
        "evento",
        sa.Column("id_evento", sa.Integer, primary_key=True),
        sa.Column("accion", sa.String(100), nullable=False),
        sa.Column("id_usuario", sa.Integer, sa.ForeignKey("identidad.usuario.id_usuario")),
        sa.Column("fecha_hora", sa.DateTime, nullable=False),
        schema="auditoria",
    )
    op.create_table(
        "menu",
        sa.Column("id_menu", sa.Integer, primary_key=True),
        sa.Column("fecha", sa.DateTime, nullable=False),
        sa.Column("descripcion", sa.String(500), nullable=False),
        schema="menu",
    )
    op.create_table(
        "servicio",
        sa.Column("id_servicio", sa.Integer, primary_key=True),
        sa.Column("nombre", sa.String(100), nullable=False),
        sa.Column("activo", sa.Boolean, nullable=False, server_default=sa.true()),
        schema="comedor",
    )
    op.create_table(
        "solicitud",
        sa.Column("id_solicitud", sa.Integer, primary_key=True),
        sa.Column("asunto", sa.String(200), nullable=False),
        sa.Column("estado", sa.String(30), nullable=False),
        schema="soporte",
    )


def downgrade() -> None:
    for tabla, esquema in (
        ("solicitud", "soporte"),
        ("servicio", "comedor"),
        ("menu", "menu"),
        ("evento", "auditoria"),
        ("lote", "importaciones"),
        ("reporte", "reportes"),
        ("cuenta_estudiante", "cuentas"),
        ("beneficio_estudiante", "beneficios"),
        ("beneficio", "beneficios"),
        ("marca", "asistencia"),
        ("estudiante", "estudiantes"),
        ("ruta", "transporte"),
    ):
        op.drop_table(tabla, schema=esquema)
