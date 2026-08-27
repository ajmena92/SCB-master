from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends
from pydantic import BaseModel


class ResumenDashboard(BaseModel):
    estudiantes: int
    confirmaciones: int
    cancelaciones: int


def crear_enrutador_dashboard(obtener_repositorio: Callable[[], Iterator], exigir_permiso: Callable[..., Callable]) -> APIRouter:
    r = APIRouter(prefix="/dashboard", tags=["dashboard"])

    @r.get("", response_model=ResumenDashboard)
    def consultar(_=Depends(exigir_permiso("reportes.dashboard.leer")), repo=Depends(obtener_repositorio)):
        return ResumenDashboard(**repo.resumen())

    return r
