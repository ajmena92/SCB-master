from typing import cast

from aplicacion.entrada import DependenciasAplicacion, crear_aplicacion
from aplicacion.modulos.identidad.servicio import ServicioPermisos
from aplicacion.nucleo.base_datos import FabricaConexionSql


def test_permiso_canonico_acepta_operador_autorizado() -> None:
    assert ServicioPermisos.tiene(frozenset({"rutas.administrar"}), "rutas.administrar")


def test_permiso_canonico_rechaza_operador_no_autorizado() -> None:
    assert not ServicioPermisos.tiene(
        frozenset({"rutas.administrar"}), "administracion.usuarios.editar"
    )


def test_entrada_modular_no_expone_dependencia_legacy() -> None:
    aplicacion = crear_aplicacion(
        DependenciasAplicacion(cast(FabricaConexionSql, object()), cookies_seguras=False)
    )
    assert all(not ruta.startswith("/api/admin") for ruta in aplicacion.openapi()["paths"])
