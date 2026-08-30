"""Refuerza integridad relacional e indices operativos.

Revision ID: 0004_integridad_relacional
"""

import sqlalchemy as sa

from alembic import op

revision = "0004_integridad_relacional"
down_revision = "0003_amplia_nombres_menu"
branch_labels = None
depends_on = None


def upgrade() -> None:
    op.drop_constraint(
        "fk_credencial_portal_persona_id_persona", "credencial_portal", type_="foreignkey"
    )
    op.create_foreign_key(
        "fk_credencial_portal_persona_id_persona",
        "credencial_portal",
        "persona",
        ["persona_id"],
        ["id"],
        ondelete="CASCADE",
    )
    for columna, destino in (("persona_id", "persona"), ("cuenta_id", "cuenta_administrativa")):
        nombre = f"fk_sesion_acceso_{columna}_{destino}"
        op.drop_constraint(nombre, "sesion_acceso", type_="foreignkey")
        op.create_foreign_key(
            nombre,
            "sesion_acceso",
            destino,
            [columna],
            ["id"],
            ondelete="CASCADE",
        )

    restricciones = (
        (
            "ck_sesion_acceso_propietario_sesion",
            "sesion_acceso",
            "(tipo = 'portal' AND persona_id IS NOT NULL AND cuenta_id IS NULL) OR "
            "(tipo = 'administracion' AND cuenta_id IS NOT NULL AND persona_id IS NULL)",
        ),
        (
            "ck_matricula_estado_matricula",
            "matricula",
            "estado IN ('activo','retirado','graduado','trasladado')",
        ),
        ("ck_componente_menu_orden_componente_menu", "componente_menu", "orden > 0"),
        (
            "ck_componente_publicado_orden_componente_publicado",
            "componente_publicado",
            "orden > 0",
        ),
        (
            "ck_tarifa_tipo_tarifa",
            "tarifa",
            "tipo_persona IN ('estudiante','profesor')",
        ),
        (
            "ck_movimiento_tiquete_tipo_movimiento_tiquete",
            "movimiento_tiquete",
            "tipo IN ('venta','reserva','liberacion','consumo','ajuste')",
        ),
        (
            "ck_movimiento_tiquete_saldo_movimiento_tiquete",
            "movimiento_tiquete",
            "saldo_resultante >= 0",
        ),
        ("ck_venta_tiquete_cantidad_venta_tiquete", "venta_tiquete", "cantidad > 0"),
        (
            "ck_venta_tiquete_montos_venta_tiquete",
            "venta_tiquete",
            "tarifa_aplicada >= 0 AND total >= 0",
        ),
        (
            "ck_reserva_comedor_estado_reserva_comedor",
            "reserva_comedor",
            "estado IN ('reservada','cancelada','consumida')",
        ),
        (
            "ck_autorizacion_comedor_decision_autorizacion",
            "autorizacion_comedor",
            "decision IN ('aprobada','rechazada')",
        ),
        (
            "ck_ingreso_comedor_modalidad_ingreso_comedor",
            "ingreso_comedor",
            "modalidad IN ('reserva','autorizacion','directo_profesor')",
        ),
        (
            "ck_lote_importacion_estado_lote_importacion",
            "lote_importacion",
            "estado IN ('pendiente','validado','confirmado','fallido')",
        ),
    )
    for nombre, tabla, expresion in restricciones:
        op.create_check_constraint(nombre, tabla, expresion)

    indices = (
        ("ix_sesion_acceso_persona_id", "sesion_acceso", ["persona_id"]),
        ("ix_sesion_acceso_cuenta_id", "sesion_acceso", ["cuenta_id"]),
        ("ix_sesion_acceso_expira_en", "sesion_acceso", ["expira_en"]),
        ("ix_asignacion_ruta_ruta_id", "asignacion_ruta", ["ruta_id"]),
        ("ix_tarifa_vigencia", "tarifa", ["tipo_persona", "fecha_inicio", "fecha_fin"]),
        ("ix_movimiento_tiquete_creado_en", "movimiento_tiquete", ["creado_en"]),
        ("ix_venta_tiquete_creado_en", "venta_tiquete", ["creado_en"]),
        ("ix_reserva_comedor_fecha", "reserva_comedor", ["fecha"]),
        ("ix_autorizacion_comedor_fecha", "autorizacion_comedor", ["fecha"]),
        ("ix_ingreso_comedor_fecha", "ingreso_comedor", ["fecha"]),
        ("ix_marca_transporte_fecha", "marca_transporte", ["fecha"]),
        ("ix_lote_importacion_creado_en", "lote_importacion", ["creado_en"]),
    )
    for nombre, tabla, columnas in indices:
        op.create_index(nombre, tabla, columnas)
    op.create_index(
        "uq_asignacion_ruta_matricula_activa",
        "asignacion_ruta",
        ["matricula_id"],
        unique=True,
        postgresql_where=sa.text("fecha_fin IS NULL"),
    )

    op.execute(
        """
        CREATE FUNCTION validar_matricula_estudiante() RETURNS trigger
        LANGUAGE plpgsql AS $$
        BEGIN
            IF NOT EXISTS (
                SELECT 1 FROM persona
                WHERE id = NEW.persona_id AND tipo = 'estudiante'
            ) THEN
                RAISE EXCEPTION 'La matricula solo admite personas estudiantes'
                    USING ERRCODE = '23514';
            END IF;
            RETURN NEW;
        END;
        $$;
        CREATE TRIGGER trg_matricula_persona_estudiante
        BEFORE INSERT OR UPDATE OF persona_id ON matricula
        FOR EACH ROW EXECUTE FUNCTION validar_matricula_estudiante();

        CREATE FUNCTION proteger_tipo_persona_matriculada() RETURNS trigger
        LANGUAGE plpgsql AS $$
        BEGIN
            IF NEW.tipo <> 'estudiante' AND EXISTS (
                SELECT 1 FROM matricula WHERE persona_id = NEW.id
            ) THEN
                RAISE EXCEPTION 'No se puede cambiar el tipo de una persona matriculada'
                    USING ERRCODE = '23514';
            END IF;
            RETURN NEW;
        END;
        $$;
        CREATE TRIGGER trg_persona_protege_tipo_matriculado
        BEFORE UPDATE OF tipo ON persona
        FOR EACH ROW EXECUTE FUNCTION proteger_tipo_persona_matriculada();
        """
    )


def downgrade() -> None:
    op.execute(
        """
        DROP TRIGGER IF EXISTS trg_persona_protege_tipo_matriculado ON persona;
        DROP FUNCTION IF EXISTS proteger_tipo_persona_matriculada();
        DROP TRIGGER IF EXISTS trg_matricula_persona_estudiante ON matricula;
        DROP FUNCTION IF EXISTS validar_matricula_estudiante();
        """
    )
    for nombre, tabla in (
        ("uq_asignacion_ruta_matricula_activa", "asignacion_ruta"),
        ("ix_lote_importacion_creado_en", "lote_importacion"),
        ("ix_marca_transporte_fecha", "marca_transporte"),
        ("ix_ingreso_comedor_fecha", "ingreso_comedor"),
        ("ix_autorizacion_comedor_fecha", "autorizacion_comedor"),
        ("ix_reserva_comedor_fecha", "reserva_comedor"),
        ("ix_venta_tiquete_creado_en", "venta_tiquete"),
        ("ix_movimiento_tiquete_creado_en", "movimiento_tiquete"),
        ("ix_tarifa_vigencia", "tarifa"),
        ("ix_asignacion_ruta_ruta_id", "asignacion_ruta"),
        ("ix_sesion_acceso_expira_en", "sesion_acceso"),
        ("ix_sesion_acceso_cuenta_id", "sesion_acceso"),
        ("ix_sesion_acceso_persona_id", "sesion_acceso"),
    ):
        op.drop_index(nombre, table_name=tabla)

    for nombre, tabla in (
        ("ck_lote_importacion_estado_lote_importacion", "lote_importacion"),
        ("ck_ingreso_comedor_modalidad_ingreso_comedor", "ingreso_comedor"),
        ("ck_autorizacion_comedor_decision_autorizacion", "autorizacion_comedor"),
        ("ck_reserva_comedor_estado_reserva_comedor", "reserva_comedor"),
        ("ck_venta_tiquete_montos_venta_tiquete", "venta_tiquete"),
        ("ck_venta_tiquete_cantidad_venta_tiquete", "venta_tiquete"),
        ("ck_movimiento_tiquete_saldo_movimiento_tiquete", "movimiento_tiquete"),
        ("ck_movimiento_tiquete_tipo_movimiento_tiquete", "movimiento_tiquete"),
        ("ck_tarifa_tipo_tarifa", "tarifa"),
        ("ck_componente_publicado_orden_componente_publicado", "componente_publicado"),
        ("ck_componente_menu_orden_componente_menu", "componente_menu"),
        ("ck_matricula_estado_matricula", "matricula"),
        ("ck_sesion_acceso_propietario_sesion", "sesion_acceso"),
    ):
        op.drop_constraint(nombre, tabla, type_="check")

    for columna, destino in (("persona_id", "persona"), ("cuenta_id", "cuenta_administrativa")):
        nombre = f"fk_sesion_acceso_{columna}_{destino}"
        op.drop_constraint(nombre, "sesion_acceso", type_="foreignkey")
        op.create_foreign_key(nombre, "sesion_acceso", destino, [columna], ["id"])
    op.drop_constraint(
        "fk_credencial_portal_persona_id_persona", "credencial_portal", type_="foreignkey"
    )
    op.create_foreign_key(
        "fk_credencial_portal_persona_id_persona",
        "credencial_portal",
        "persona",
        ["persona_id"],
        ["id"],
    )
