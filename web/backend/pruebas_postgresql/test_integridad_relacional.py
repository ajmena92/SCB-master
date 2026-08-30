"""Verifica que el modelo declare las invariantes estructurales del corte."""

from sqlalchemy import CheckConstraint

from aplicacion.modelos.maestros import AsignacionRuta, Matricula, SesionAcceso
from aplicacion.modelos.menu import ComponenteMenu, ComponentePublicado
from aplicacion.modelos.operacion import IngresoComedor, ReservaComedor, Tarifa


def _restricciones(tabla) -> set[str]:
    return {
        restriccion.name
        for restriccion in tabla.__table__.constraints
        if isinstance(restriccion, CheckConstraint) and restriccion.name
    }


def _indices(tabla) -> set[str]:
    return {indice.name for indice in tabla.__table__.indexes if indice.name}


def test_invariantes_de_identidad_y_matricula() -> None:
    assert "ck_sesion_acceso_propietario_sesion" in _restricciones(SesionAcceso)
    assert "ck_matricula_estado_matricula" in _restricciones(Matricula)
    assert "uq_asignacion_ruta_matricula_activa" in _indices(AsignacionRuta)


def test_invariantes_de_menu_y_operacion() -> None:
    assert "ck_componente_menu_orden_componente_menu" in _restricciones(ComponenteMenu)
    assert "ck_componente_publicado_orden_componente_publicado" in _restricciones(
        ComponentePublicado
    )
    assert "ck_tarifa_tipo_tarifa" in _restricciones(Tarifa)
    assert "ck_reserva_comedor_estado_reserva_comedor" in _restricciones(ReservaComedor)
    assert "ck_ingreso_comedor_modalidad_ingreso_comedor" in _restricciones(IngresoComedor)
