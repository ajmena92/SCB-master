"""Ensamblador mínimo de la plataforma web canónica."""

from __future__ import annotations

from fastapi import FastAPI

from aplicacion.composicion import incluir_modulos
from aplicacion.dependencias import (
    DependenciasAplicacion,
    crear_dependencias_modulos,
)
from aplicacion.middleware import configurar_cors
from aplicacion.modulos.salud.repositorio import RepositorioSalud
from aplicacion.nucleo.base_datos import FabricaConexionSql
from aplicacion.salud import registrar_rutas_salud
from config import Settings


def crear_aplicacion(dependencias: DependenciasAplicacion | None = None) -> FastAPI:
    """Crea la aplicación y delega la composición de módulos e infraestructura."""

    configuracion = Settings.from_environment() if dependencias is None else None
    if dependencias is None:
        assert configuracion is not None
        dependencias = DependenciasAplicacion(
            FabricaConexionSql(configuracion.sql_connection_string), configuracion.cookie_secure
        )

    aplicacion = FastAPI(title="Plataforma web modular")
    registrar_rutas_salud(aplicacion, RepositorioSalud(dependencias.fabrica_sql))
    configurar_cors(aplicacion, configuracion)
    incluir_modulos(aplicacion, **crear_dependencias_modulos(dependencias, configuracion))
    return aplicacion
