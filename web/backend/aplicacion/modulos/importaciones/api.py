from collections.abc import Callable, Iterator
from typing import Any

from fastapi import APIRouter, Depends, File, HTTPException, Request, UploadFile

from aplicacion.nucleo.archivos import ArchivoExcedeLimite, leer_archivo_limitado

from .esquemas import LoteSalida, Previsualizacion
from .repositorio import RepositorioImportaciones
from .servicio import ServicioImportaciones

MAXIMO_IMPORTACION_BYTES = 10_000_000


async def _leer_importacion(archivo: UploadFile) -> bytes:
    try:
        contenido = await leer_archivo_limitado(archivo, MAXIMO_IMPORTACION_BYTES)
    except ArchivoExcedeLimite as exc:
        raise HTTPException(413, "El archivo supera el tamaño máximo permitido") from exc
    if not contenido:
        raise HTTPException(400, "El archivo está vacío")
    return contenido


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioImportaciones]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
) -> APIRouter:
    r = APIRouter(prefix="/importaciones", tags=["importaciones"])

    def servicio(
        repo: RepositorioImportaciones = Depends(obtener_repositorio),
    ) -> ServicioImportaciones:
        return ServicioImportaciones(repo)

    @r.post("/previsualizaciones", response_model=Previsualizacion, response_model_by_alias=True)
    async def previsualizar(
        archivo: UploadFile = File(...),
        _u: dict = Depends(exigir_permiso("importaciones.leer")),
        caso: ServicioImportaciones = Depends(servicio),
    ) -> Previsualizacion:
        return caso.previsualizar(await _leer_importacion(archivo))

    @r.post("/lotes", response_model=LoteSalida, response_model_by_alias=True)
    async def ejecutar(
        request: Request,
        archivo: UploadFile = File(...),
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("importaciones.ejecutar")),
        caso: ServicioImportaciones = Depends(servicio),
    ) -> LoteSalida:
        try:
            return caso.ejecutar(
                archivo.filename or "archivo.csv",
                await _leer_importacion(archivo),
                int(usuario["idUsuario"]),
            )
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @r.get("/lotes/{id_lote}", response_model=LoteSalida, response_model_by_alias=True)
    def lote(
        id_lote: int,
        _u: dict = Depends(exigir_permiso("importaciones.leer")),
        caso: ServicioImportaciones = Depends(servicio),
    ) -> LoteSalida:
        try:
            return caso.obtener(id_lote)
        except ValueError as exc:
            raise HTTPException(404, str(exc)) from exc

    @r.post("/lotes/{id_lote}/reversion", response_model=LoteSalida, response_model_by_alias=True)
    def revertir(
        id_lote: int,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("importaciones.ejecutar")),
        caso: ServicioImportaciones = Depends(servicio),
    ) -> LoteSalida:
        try:
            return caso.revertir(id_lote, int(usuario["idUsuario"]))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    return r
