"""Adaptador HTTP del menú y su calendario institucional."""

from datetime import date
from fastapi import APIRouter, Depends, HTTPException

from aplicacion.esquemas import (
    CalendarioMenuEntrada,
    PlantillaEntrada,
    SustitucionMenuEntrada,
)


def crear_router(obtener_servicio, exigir_permiso) -> APIRouter:
    router = APIRouter()

    @router.get("/menu/plantillas", dependencies=[Depends(exigir_permiso("menu.administrar"))])
    async def plantillas(servicio=Depends(obtener_servicio)):
        return servicio.listar_plantillas()

    @router.post(
        "/menu/plantillas",
        status_code=201,
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def crear(datos: PlantillaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_plantilla(datos)

    @router.put(
        "/menu/plantillas/{semana}/{dia}",
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def actualizar(
        semana: int, dia: int, datos: PlantillaEntrada, servicio=Depends(obtener_servicio)
    ):
        return servicio.actualizar_plantilla(semana, dia, datos)

    @router.get("/menu/calendario", dependencies=[Depends(exigir_permiso("menu.administrar"))])
    async def calendario(desde: date, hasta: date, servicio=Depends(obtener_servicio)):
        if hasta < desde:
            raise HTTPException(422, "El rango del calendario es inválido")
        return servicio.listar_calendario(desde, hasta)

    @router.put("/menu/calendario", dependencies=[Depends(exigir_permiso("menu.administrar"))])
    async def actualizar_calendario(datos: CalendarioMenuEntrada, servicio=Depends(obtener_servicio)):
        return servicio.actualizar_calendario(datos)

    @router.put(
        "/menu/sustituciones/{fecha}",
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def actualizar_sustitucion(
        fecha: date, datos: SustitucionMenuEntrada, servicio=Depends(obtener_servicio)
    ):
        if fecha != datos.fecha:
            raise HTTPException(422, "La fecha de sustitución no coincide con el cuerpo")
        return servicio.guardar_sustitucion(datos)

    @router.get(
        "/menu/sustituciones",
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def sustituciones(servicio=Depends(obtener_servicio)):
        return servicio.listar_sustituciones()

    return router
