"""Contratos de composición de los casos de uso operativos."""

from types import SimpleNamespace

from aplicacion.servicios_operacion_captura import ServicioOperacionCaptura


def test_captura_delega_el_ingreso_al_servicio_de_comedor() -> None:
    repo = SimpleNamespace()
    comedor = SimpleNamespace(personas=SimpleNamespace())

    servicio = ServicioOperacionCaptura(repo, comedor, "clave")

    assert servicio.repo is repo
    assert servicio.comedor is comedor
