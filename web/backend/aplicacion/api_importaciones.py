"""Adaptador HTTP de importacion JSON o Excel multipart."""

from fastapi import APIRouter, Depends, HTTPException, Request

from aplicacion.esquemas import ConfirmacionImportacion, ImportacionEntrada


def crear_router(obtener_servicio, administrador) -> APIRouter:
    router = APIRouter(prefix="/importaciones", dependencies=[Depends(administrador)])

    @router.post(
        "/previsualizar",
        openapi_extra={
            "requestBody": {"content": {"application/json": {}, "multipart/form-data": {}}}
        },
    )
    async def previsualizar(request: Request, servicio=Depends(obtener_servicio)):
        if request.headers.get("content-type", "").startswith("multipart/form-data"):
            formulario = await request.form()
            archivo = formulario.get("archivo")
            anio = int(formulario.get("anio", 0))
            if archivo is None or not str(getattr(archivo, "filename", "")).lower().endswith(
                ".xlsx"
            ):
                raise HTTPException(422, "Se requiere archivo .xlsx")
            datos = servicio.desde_excel(await archivo.read(), anio)
        else:
            datos = ImportacionEntrada.model_validate(await request.json())
        return {
            **servicio.previsualizar(datos),
            "datos": datos.model_dump(mode="json", by_alias=True),
        }

    @router.post("/confirmar")
    async def confirmar(datos: ConfirmacionImportacion, servicio=Depends(obtener_servicio)):
        entrada = ImportacionEntrada(anio=datos.anio, filas=datos.filas)
        return servicio.confirmar(entrada, datos.huella)

    return router
