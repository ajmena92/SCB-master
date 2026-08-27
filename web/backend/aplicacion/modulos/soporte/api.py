from collections.abc import Callable

from fastapi import APIRouter, Depends

from .esquemas import SolicitudEntrada, SolicitudSalida
from .servicio import ServicioSoporte


def crear_enrutador(
    obtener_repositorio: Callable, exigir_permiso: Callable[..., Callable], exigir_csrf: Callable
) -> APIRouter:
    r = APIRouter(prefix="/soporte", tags=["soporte"])

    def servicio(repo=Depends(obtener_repositorio)):
        return ServicioSoporte(repo)

    @r.post("/solicitudes", response_model=SolicitudSalida, response_model_by_alias=True)
    def crear(
        d: SolicitudEntrada,
        u: dict = Depends(exigir_permiso("soporte.crear")),
        _=Depends(exigir_csrf),
        s=Depends(servicio),
    ):
        return s.crear(d, int(u["idUsuario"]))

    return r
