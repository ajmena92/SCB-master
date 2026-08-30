"""Alinea nombres fisicos de checks con la metadata ORM.

Revision ID: 0005_normaliza_checks
"""

from alembic import op

revision = "0005_normaliza_checks"
down_revision = "0004_integridad_relacional"
branch_labels = None
depends_on = None


RENOMBRES = (
    (
        "sesion_acceso",
        "ck_sesion_acceso_ck_sesion_acceso_propietario_sesion",
        "ck_sesion_acceso_propietario_sesion",
    ),
    (
        "matricula",
        "ck_matricula_ck_matricula_estado_matricula",
        "ck_matricula_estado_matricula",
    ),
    (
        "componente_menu",
        "ck_componente_menu_ck_componente_menu_orden_componente_menu",
        "ck_componente_menu_orden_componente_menu",
    ),
    (
        "componente_publicado",
        "ck_componente_publicado_ck_componente_publicado_orden_c_9a81",
        "ck_componente_publicado_orden_componente_publicado",
    ),
    ("tarifa", "ck_tarifa_ck_tarifa_tipo_tarifa", "ck_tarifa_tipo_tarifa"),
    (
        "movimiento_tiquete",
        "ck_movimiento_tiquete_ck_movimiento_tiquete_tipo_movimi_9f9c",
        "ck_movimiento_tiquete_tipo_movimiento_tiquete",
    ),
    (
        "movimiento_tiquete",
        "ck_movimiento_tiquete_ck_movimiento_tiquete_saldo_movim_a754",
        "ck_movimiento_tiquete_saldo_movimiento_tiquete",
    ),
    (
        "venta_tiquete",
        "ck_venta_tiquete_ck_venta_tiquete_cantidad_venta_tiquete",
        "ck_venta_tiquete_cantidad_venta_tiquete",
    ),
    (
        "venta_tiquete",
        "ck_venta_tiquete_ck_venta_tiquete_montos_venta_tiquete",
        "ck_venta_tiquete_montos_venta_tiquete",
    ),
    (
        "reserva_comedor",
        "ck_reserva_comedor_ck_reserva_comedor_estado_reserva_comedor",
        "ck_reserva_comedor_estado_reserva_comedor",
    ),
    (
        "autorizacion_comedor",
        "ck_autorizacion_comedor_ck_autorizacion_comedor_decisio_cc63",
        "ck_autorizacion_comedor_decision_autorizacion",
    ),
    (
        "ingreso_comedor",
        "ck_ingreso_comedor_ck_ingreso_comedor_modalidad_ingreso_comedor",
        "ck_ingreso_comedor_modalidad_ingreso_comedor",
    ),
    (
        "lote_importacion",
        "ck_lote_importacion_ck_lote_importacion_estado_lote_importacion",
        "ck_lote_importacion_estado_lote_importacion",
    ),
)


def _renombrar(tabla: str, origen: str, destino: str) -> None:
    op.execute(f"ALTER TABLE {tabla} RENAME CONSTRAINT {origen} TO {destino}")


def upgrade() -> None:
    for tabla, origen, destino in RENOMBRES:
        _renombrar(tabla, origen, destino)


def downgrade() -> None:
    for tabla, origen, destino in reversed(RENOMBRES):
        _renombrar(tabla, destino, origen)
