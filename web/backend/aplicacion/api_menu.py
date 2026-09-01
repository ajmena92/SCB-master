"""Adaptador HTTP del menu."""

from fastapi import APIRouter, Depends

from aplicacion.esquemas import PlantillaEntrada, PublicacionEntrada


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
        "/menu/plantillas/{plantilla_id}",
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def actualizar(
        plantilla_id: int, datos: PlantillaEntrada, servicio=Depends(obtener_servicio)
    ):
        return servicio.actualizar_plantilla(plantilla_id, datos)

    @router.get("/menu/publicaciones")
    async def publicaciones(servicio=Depends(obtener_servicio)):
        return servicio.listar_publicaciones()

    @router.post(
        "/menu/publicaciones",
        status_code=201,
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def publicar(datos: PublicacionEntrada, servicio=Depends(obtener_servicio)):
        return servicio.publicar(datos)

    return router
