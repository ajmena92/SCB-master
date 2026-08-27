from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends, Query

from .esquemas import EventoSalida
from .repositorio import RepositorioAuditoria
from .servicio import ServicioAuditoria


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioAuditoria]],
    exigir_permiso: Callable[..., Callable],
) -> APIRouter:
    r = APIRouter(prefix="/auditoria", tags=["auditoria"])

    def servicio(repo: RepositorioAuditoria = Depends(obtener_repositorio)) -> ServicioAuditoria:
        return ServicioAuditoria(repo)

    @r.get("/eventos", response_model=list[EventoSalida], response_model_by_alias=True)
    def eventos(
        limite: int = Query(100, ge=1, le=500),
        _u: dict = Depends(exigir_permiso("auditoria.leer")),
        caso: ServicioAuditoria = Depends(servicio),
    ) -> list[EventoSalida]:
        return caso.consultar(limite)

    return r
