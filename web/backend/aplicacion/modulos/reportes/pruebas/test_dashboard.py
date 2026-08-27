from fastapi.routing import APIRoute as RutaAPI

from aplicacion.modulos.reportes.dashboard import crear_enrutador_dashboard


def test_dashboard_expone_ruta_canonica_y_permiso() -> None:
    permisos = []

    def exigir(permiso):
        permisos.append(permiso)
        return lambda: None

    enrutador = crear_enrutador_dashboard(lambda: iter(()), exigir)
    rutas = getattr(enrutador, "routes")
    assert isinstance(rutas[0], RutaAPI)
    assert rutas[0].path == "/dashboard"
    assert permisos == ["reportes.dashboard.leer"]
