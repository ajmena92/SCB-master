from aplicacion.entrada import crear_aplicacion
from aplicacion.nucleo.postgresql import crear_motor
from config import Settings


def test_entrada_modular_publica_salud_y_no_rutas_menu_legacy() -> None:
    aplicacion = crear_aplicacion(
        motor=crear_motor("sqlite://"),
        configuracion=Settings(
            database_url="postgresql://no-usada",
            cors_origin="http://localhost:5173",
            cookie_secure=False,
            csrf_secret="csrf-pruebas",
            carnet_qr_clave="qr-pruebas",
        ),
    )
    rutas = set(aplicacion.openapi()["paths"])
    assert "/api/v1/salud" in rutas
    assert not any(ruta.startswith("/api/admin") for ruta in rutas)
