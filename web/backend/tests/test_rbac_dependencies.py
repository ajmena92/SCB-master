import asyncio

import pytest
from fastapi import HTTPException

from aplicacion.dependencias_v1 import crear_dependencias
from aplicacion.entrada import crear_aplicacion
from aplicacion.nucleo.postgresql import crear_motor
from config import Settings


def test_permiso_canonico_acepta_operador_autorizado() -> None:
    _, _, _, _, exigir_permiso, _ = crear_dependencias(lambda: None)
    dependencia = exigir_permiso("rutas.administrar")

    identidad = {"rol": "operador", "permisos": ["rutas.administrar"]}
    assert asyncio.run(dependencia(identidad)) is identidad


def test_permiso_canonico_rechaza_operador_no_autorizado() -> None:
    _, _, _, _, exigir_permiso, _ = crear_dependencias(lambda: None)
    dependencia = exigir_permiso("administracion.usuarios.editar")

    with pytest.raises(HTTPException, match="No tiene permiso"):
        asyncio.run(dependencia({"rol": "operador", "permisos": ["rutas.administrar"]}))


def test_entrada_modular_no_expone_dependencia_legacy() -> None:
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
    assert all(not ruta.startswith("/api/admin") for ruta in aplicacion.openapi()["paths"])
