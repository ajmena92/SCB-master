"""Composicion unica de la API PostgreSQL."""

from __future__ import annotations

from fastapi import APIRouter, FastAPI
from fastapi.middleware.cors import CORSMiddleware
from sqlalchemy.engine import Engine

from aplicacion.api_administracion import crear_router as router_administracion
from aplicacion.api_autenticacion import crear_router as router_autenticacion
from aplicacion.api_fotos import crear_router as router_fotos
from aplicacion.api_importaciones import crear_router as router_importaciones
from aplicacion.api_maestros import crear_router as router_maestros
from aplicacion.api_menu import crear_router as router_menu
from aplicacion.api_operacion import crear_router as router_operacion
from aplicacion.api_portal import crear_router as router_portal
from aplicacion.api_reportes import crear_router as router_reportes
from aplicacion.casos_administracion import ServicioAdministracion
from aplicacion.casos_catalogos import ServicioCatalogos
from aplicacion.casos_identidad import ServicioIdentidad
from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.casos_portal import ServicioPortal
from aplicacion.casos_reportes import ServicioReportes
from aplicacion.dependencias_v1 import crear_dependencias
from aplicacion.nucleo.postgresql import crear_fabrica_sesiones, crear_motor, dependencia_sesion
from aplicacion.repositorios import RepositorioReportes
from aplicacion.repositorios_administracion import RepositorioAdministracion
from aplicacion.repositorios_catalogos import RepositorioCatalogos
from aplicacion.repositorios_identidad import RepositorioIdentidad
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.repositorios_operacion import RepositorioOperacion
from aplicacion.repositorios_portal import RepositorioPortal
from aplicacion.servicios import ServicioOperacion
from config import Settings


def crear_aplicacion(
    *, motor: Engine | None = None, configuracion: Settings | None = None
) -> FastAPI:
    configuracion = configuracion or Settings.from_environment()
    motor = motor or crear_motor(configuracion.database_url)
    obtener_sesion = dependencia_sesion(crear_fabrica_sesiones(motor))

    async def obtener_identidad(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioIdentidad(
            RepositorioIdentidad(sesion),
            student_max_login_attempts=configuracion.student_max_login_attempts,
            student_lock_minutes=configuracion.student_lock_minutes,
            admin_max_login_attempts=configuracion.admin_max_login_attempts,
            admin_lock_minutes=configuracion.admin_lock_minutes,
        )

    async def obtener_catalogos(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioCatalogos(RepositorioCatalogos(sesion))

    async def obtener_operacion(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioOperacion(RepositorioOperacion(sesion), configuracion.carnet_qr_clave)

    async def obtener_importacion(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioImportacion(RepositorioImportacion(sesion))

    async def obtener_reportes(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioReportes(RepositorioReportes(sesion))

    async def obtener_portal(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioPortal(RepositorioPortal(sesion), configuracion.carnet_qr_clave)

    async def obtener_administracion(sesion=__import__("fastapi").Depends(obtener_sesion)):
        return ServicioAdministracion(RepositorioAdministracion(sesion))

    (
        actual,
        portal_operativo,
        administrativo,
        administrador,
        exigir_permiso,
        exigir_alguno,
    ) = crear_dependencias(obtener_identidad)

    aplicacion = FastAPI(title="SCB Plataforma Web", version="1.0.0")
    aplicacion.add_middleware(
        CORSMiddleware,
        allow_origins=[configuracion.cors_origin],
        allow_credentials=True,
        allow_methods=["GET", "POST", "PUT", "DELETE"],
        allow_headers=["Authorization", "Content-Type"],
    )
    api = APIRouter(prefix="/api/v1")

    @api.get("/salud")
    async def salud() -> dict[str, str]:
        return {"estado": "ok", "baseDatos": "postgresql"}

    api.include_router(router_autenticacion(obtener_identidad, actual))
    api.include_router(router_administracion(obtener_administracion, actual, administrador))
    api.include_router(router_maestros(obtener_catalogos, exigir_permiso, exigir_alguno))
    api.include_router(router_fotos(obtener_catalogos, exigir_permiso))
    api.include_router(router_importaciones(obtener_importacion, exigir_permiso))
    api.include_router(router_menu(obtener_catalogos, exigir_permiso))
    api.include_router(
        router_operacion(obtener_operacion, portal_operativo, exigir_permiso, exigir_alguno)
    )
    api.include_router(router_reportes(obtener_reportes, exigir_permiso))
    api.include_router(router_portal(obtener_portal, portal_operativo))
    aplicacion.include_router(api)
    return aplicacion
