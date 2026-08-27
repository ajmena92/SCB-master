from typing import cast

from aplicacion.entrada import DependenciasAplicacion, crear_aplicacion
from aplicacion.nucleo.base_datos import FabricaConexionSql


def test_entrada_modular_publica_salud_y_no_rutas_menu_legacy() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    rutas = set(aplicacion.openapi()["paths"])
    assert "/api/v1/salud" in rutas
    assert not any(ruta.startswith("/api/admin") for ruta in rutas)
