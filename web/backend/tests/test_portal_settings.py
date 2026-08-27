from typing import cast

from aplicacion.entrada import DependenciasAplicacion, crear_aplicacion
from aplicacion.nucleo.base_datos import FabricaConexionSql


def test_configuracion_modular_no_expone_rutas_de_portal_legacy() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    rutas = set(aplicacion.openapi()["paths"])
    assert "/api/v1/administracion/usuarios" in rutas
    assert not any("portal" in ruta or ruta.startswith("/api/admin") for ruta in rutas)
