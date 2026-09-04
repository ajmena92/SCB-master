from fastapi import FastAPI

from aplicacion.entrada import crear_aplicacion
from aplicacion.nucleo.postgresql import crear_motor
from config import Settings


def crear_aplicacion_pruebas() -> FastAPI:
    return crear_aplicacion(
        motor=crear_motor("sqlite://"),
        configuracion=Settings(
            database_url="postgresql://no-usada",
            cors_origin="http://localhost:5173",
            cookie_secure=False,
            csrf_secret="csrf-pruebas",
            carnet_qr_clave="qr-pruebas",
        ),
    )


def test_entrada_publica_rutas_canonicas_y_no_legacy() -> None:
    aplicacion = crear_aplicacion_pruebas()
    rutas = set(aplicacion.openapi()["paths"])
    assert {
        "/api/v1/salud",
        "/api/v1/autenticacion/administracion",
        "/api/v1/autenticacion/portal",
        "/api/v1/rutas",
        "/api/v1/comedor/operacion",
        "/api/v1/parametros-operativos/institucion",
        "/api/v1/menu/calendario",
        "/api/v1/portal/carnet",
        "/api/v1/portal/estado",
        "/api/v1/sesion",
    } <= rutas
    assert not any(
        "/admin/" in ruta or ruta.startswith("/api/admin") or "legacy" in ruta for ruta in rutas
    )
    assert isinstance(aplicacion, FastAPI)


def test_entrada_no_publica_beneficios_parciales_depreciados() -> None:
    aplicacion = crear_aplicacion_pruebas()
    rutas = set(aplicacion.openapi()["paths"])

    assert not any(ruta.startswith("/api/v1/beneficios") for ruta in rutas)


def test_entrada_publica_administracion_usa_rutas_canonicas() -> None:
    aplicacion = crear_aplicacion_pruebas()
    rutas = set(aplicacion.openapi()["paths"])

    assert {
        "/api/v1/administracion/cuentas",
        "/api/v1/administracion/cuentas/{cuenta_id}",
        "/api/v1/administracion/permisos",
    } <= rutas
    assert not any(ruta.startswith("/api/admin") for ruta in rutas)
