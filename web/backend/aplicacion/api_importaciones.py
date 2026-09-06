"""Adaptador HTTP de importacion JSON o Excel multipart."""

from fastapi import APIRouter, Depends, HTTPException, Request
from pydantic import ValidationError
from starlette.concurrency import run_in_threadpool
from starlette.datastructures import UploadFile

from aplicacion.esquemas import ConfirmacionImportacion, ImportacionEntrada

MAXIMO_IMPORTACION_BYTES = 12 * 1024 * 1024


def crear_router(obtener_servicio, exigir_permiso) -> APIRouter:
    router = APIRouter(
        prefix="/importaciones",
        dependencies=[Depends(exigir_permiso("importaciones.administrar"))],
    )

    @router.post(
        "/previsualizar",
        openapi_extra={
            "requestBody": {"content": {"application/json": {}, "multipart/form-data": {}}}
        },
    )
    async def previsualizar(request: Request, servicio=Depends(obtener_servicio)):
        if request.headers.get("content-type", "").startswith("multipart/form-data"):
            longitud = request.headers.get("content-length")
            # Content-Length incluye cabeceras y separadores multipart, además
            # del archivo. El límite exacto se aplica al contenido leído abajo.
            if longitud is not None and int(longitud) > MAXIMO_IMPORTACION_BYTES + 64 * 1024:
                raise HTTPException(413, "El archivo supera el limite de 12 MiB")
            formulario = await request.form()
            archivo = formulario.get("archivo")
            valor_anio = formulario.get("anio")
            if isinstance(valor_anio, UploadFile):
                raise HTTPException(422, "El año es invalido")
            anio = int(str(valor_anio or 0))
            if (
                not isinstance(archivo, UploadFile)
                or not archivo.filename
                or not archivo.filename.lower().endswith(".xlsx")
            ):
                raise HTTPException(422, "Se requiere archivo .xlsx")
            contenido = await archivo.read(MAXIMO_IMPORTACION_BYTES + 1)
            if len(contenido) > MAXIMO_IMPORTACION_BYTES:
                raise HTTPException(413, "El archivo supera el limite de 12 MiB")
            datos = await run_in_threadpool(servicio.desde_excel, contenido, anio)
        else:
            try:
                datos = ImportacionEntrada.model_validate(await request.json())
            except ValidationError as error:
                raise HTTPException(422, detail=error.errors()) from error

        def construir_respuesta():
            return {
                **servicio.previsualizar(datos),
                "datos": datos.model_dump(mode="json", by_alias=True),
            }

        return await run_in_threadpool(construir_respuesta)

    @router.post("/confirmar")
    def confirmar(datos: ConfirmacionImportacion, servicio=Depends(obtener_servicio)):
        entrada = ImportacionEntrada(anio=datos.anio, filas=datos.filas)
        return servicio.confirmar(entrada, datos.huella)

    return router
