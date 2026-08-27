"""Ensamblador de las rutas del dominio de estudiantes."""

from __future__ import annotations

from collections.abc import Callable, Iterator
from datetime import date

from fastapi import APIRouter

from aplicacion.modulos.identidad.servicio import ServicioIdentidad

from .administracion import crear_enrutador_administracion
from .portal import crear_enrutador_portal


def crear_enrutador_operaciones(
    obtener_repositorio: Callable[[], Iterator],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
    obtener_identidad: Callable[[], ServicioIdentidad] | None = None,
    obtener_identidad_estudiante: Callable[[], ServicioIdentidad] | None = None,
    obtener_menu: Callable[[], Iterator] | None = None,
    obtener_asistencia: Callable[[], Iterator] | None = None,
    cookies_seguras: bool = True,
    duracion_sesion_estudiante: int = 31536000,
    obtener_fecha_local: Callable[[], date] | None = None,
) -> APIRouter:
    """Compone las rutas públicas del portal y la administración estudiantil."""
    enrutador = APIRouter(prefix="/estudiantes", tags=["estudiantes-administracion"])
    enrutador.include_router(
        crear_enrutador_portal(
            obtener_repositorio=obtener_repositorio,
            obtener_identidad=obtener_identidad,
            obtener_identidad_estudiante=obtener_identidad_estudiante,
            obtener_menu=obtener_menu,
            obtener_asistencia=obtener_asistencia,
            cookies_seguras=cookies_seguras,
            duracion_sesion_estudiante=duracion_sesion_estudiante,
            exigir_csrf=exigir_csrf,
            obtener_fecha_local=obtener_fecha_local,
        )
    )
    enrutador.include_router(
        crear_enrutador_administracion(
            obtener_repositorio=obtener_repositorio,
            exigir_permiso=exigir_permiso,
            exigir_csrf=exigir_csrf,
        )
    )
    return enrutador
