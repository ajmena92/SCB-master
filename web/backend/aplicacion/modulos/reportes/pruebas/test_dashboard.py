from fastapi.routing import APIRoute

from aplicacion.modulos.reportes.dashboard import crear_enrutador_dashboard


def test_dashboard_expone_ruta_canonica_y_permiso() -> None:
    permisos = []

    def exigir(permiso):
        permisos.append(permiso)
        return lambda: None

    router = crear_enrutador_dashboard(lambda: iter(()), exigir)
    assert isinstance(router.routes[0], APIRoute)
    assert router.routes[0].path == "/dashboard"
    assert permisos == ["reportes.dashboard.leer"]
