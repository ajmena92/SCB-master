from collections.abc import Callable, Iterator
from typing import Any

from fastapi import APIRouter, Depends, File, HTTPException, Request, UploadFile

from .esquemas import LoteSalida, Previsualizacion
from .repositorio import RepositorioImportaciones
from .servicio import ServicioImportaciones


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
        return caso.previsualizar(await archivo.read())

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
                archivo.filename or "archivo.csv", await archivo.read(), int(usuario["idUsuario"])
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
