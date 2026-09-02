"""Adaptador HTTP de fotografías privadas de personas."""

from fastapi import APIRouter, Depends, File, HTTPException, Response, UploadFile

from aplicacion.fotografias import FotografiaInvalida, preparar_fotografia

MAXIMO_FOTO_BYTES = 5_000_000
TIPOS_FOTO = {"image/jpeg", "image/png"}


def crear_router(obtener_servicio, exigir_permiso) -> APIRouter:
    router = APIRouter()

    @router.get(
        "/personas/{persona_id}/foto",
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def consultar_foto(persona_id: int, servicio=Depends(obtener_servicio)):
        foto = servicio.obtener_foto_persona(persona_id)
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto.contenido, media_type=foto.tipo_contenido)

    @router.post(
        "/personas/{persona_id}/foto",
        status_code=204,
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def cargar_foto(
        persona_id: int,
        archivo: UploadFile = File(...),
        servicio=Depends(obtener_servicio),
    ):
        if archivo.content_type not in TIPOS_FOTO:
            raise HTTPException(415, "La fotografía debe ser JPEG o PNG")
        contenido = await archivo.read(MAXIMO_FOTO_BYTES + 1)
        if not contenido or len(contenido) > MAXIMO_FOTO_BYTES:
            raise HTTPException(413, "La fotografía supera el tamaño máximo permitido")
        try:
            fotografia = preparar_fotografia(contenido)
        except FotografiaInvalida as exc:
            raise HTTPException(422, str(exc)) from exc
        servicio.guardar_foto_persona(persona_id, fotografia, "image/jpeg")

    @router.delete(
        "/personas/{persona_id}/foto",
        status_code=204,
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def eliminar_foto(persona_id: int, servicio=Depends(obtener_servicio)):
        servicio.eliminar_foto_persona(persona_id)

    return router
