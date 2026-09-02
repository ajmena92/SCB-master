"""Endpoints canónicos del portal de estudiantes y profesores."""

from datetime import date

from fastapi import APIRouter, Depends, HTTPException, Response


def crear_router(obtener_servicio, portal_operativo) -> APIRouter:
    router = APIRouter(prefix="/portal")

    def persona_portal(identidad):
        if identidad["tipo"] != "portal":
            raise HTTPException(403, "Se requiere una sesión de portal")
        return identidad["persona"]

    @router.get("/carnet")
    async def carnet(
        fecha: date | None = None,
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.carnet(persona_portal(identidad), fecha or date.today())

    @router.get("/carnet/foto")
    async def foto_carnet(
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        foto = servicio.foto_carnet(persona_portal(identidad))
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto.contenido, media_type=foto.tipo_contenido)

    @router.get("/estado")
    async def estado(
        fecha: date | None = None,
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.estado(persona_portal(identidad), fecha or date.today())

    return router
