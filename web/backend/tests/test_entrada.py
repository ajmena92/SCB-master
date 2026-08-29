from typing import cast

from fastapi import FastAPI

from aplicacion.composicion import crear_enrutador_aplicacion
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


def test_entrada_publica_rutas_canonicas_de_beneficios() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    rutas = set(aplicacion.openapi()["paths"])

    assert {
        "/api/v1/beneficios",
        "/api/v1/beneficios/{id_beneficio}",
        "/api/v1/beneficios/estudiantes/{id_estudiante}",
    } <= rutas
    assert not any("/admin/beneficios" in ruta for ruta in rutas)


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


def test_composicion_de_beneficios_exige_permisos_canonicos() -> None:
    permisos_solicitados: list[str] = []

    def exigir_permiso(permiso: str):
        permisos_solicitados.append(permiso)

        def dependencia() -> dict[str, object]:
            return {}

        return dependencia

    crear_enrutador_aplicacion(
        dependencias_beneficios={
            "obtener_repositorio": lambda: iter(()),
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": lambda: {},
            "obtener_ip": lambda _request: "WEB",
        }
    )

    assert permisos_solicitados.count("beneficios.leer") == 2
    assert permisos_solicitados.count("beneficios.editar") == 3
