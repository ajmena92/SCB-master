from typing import cast

from fastapi import FastAPI

from aplicacion.entrada import DependenciasAplicacion, crear_aplicacion
from aplicacion.nucleo.base_datos import FabricaConexionSql


def test_entrada_publica_rutas_canonicas_y_no_legacy() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    rutas = set(aplicacion.openapi()["paths"])
    assert {
        "/api/v1/salud",
        "/api/v1/autenticacion",
        "/api/v1/transporte/rutas",
        "/api/v1/asistencia/marcas",
        "/api/v1/asistencia/marcas/{id_marca}/correccion",
        "/api/v1/parametros",
        "/api/v1/calendario",
        "/api/v1/profesores/menu",
        "/api/v1/profesores/carnet",
        "/api/v1/profesores/asistencia/hoy",
        "/api/v1/profesores/asistencia/{accion}",
    } <= rutas
    assert not any(
        "/admin/" in ruta or ruta.startswith("/api/admin") or "legacy" in ruta for ruta in rutas
    )
    assert isinstance(aplicacion, FastAPI)


def test_entrada_no_publica_beneficios_parciales_depreciados() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    rutas = set(aplicacion.openapi()["paths"])

    assert not any(ruta.startswith("/api/v1/beneficios") for ruta in rutas)


def test_entrada_publica_administracion_usa_rutas_canonicas() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    rutas = set(aplicacion.openapi()["paths"])

    assert {
        "/api/v1/administracion/usuarios",
        "/api/v1/administracion/usuarios/{id_usuario}",
        "/api/v1/administracion/roles",
        "/api/v1/administracion/permisos",
    } <= rutas
    assert not any(ruta.startswith("/api/admin") for ruta in rutas)
