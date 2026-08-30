"""Adaptador HTTP del menu."""

from fastapi import APIRouter, Depends

from aplicacion.esquemas import PlantillaEntrada, PublicacionEntrada


def crear_router(obtener_servicio, administrador) -> APIRouter:
    router = APIRouter()

    @router.get("/menu/plantillas", dependencies=[Depends(administrador)])
    async def plantillas(servicio=Depends(obtener_servicio)):
        return servicio.listar_plantillas()

    @router.post("/menu/plantillas", status_code=201, dependencies=[Depends(administrador)])
    async def crear(datos: PlantillaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_plantilla(datos)

    @router.get("/menu/publicaciones")
    async def publicaciones(servicio=Depends(obtener_servicio)):
        return servicio.listar_publicaciones()

    @router.post("/menu/publicaciones", status_code=201, dependencies=[Depends(administrador)])
    async def publicar(datos: PublicacionEntrada, servicio=Depends(obtener_servicio)):
        return servicio.publicar(datos)

    return router
