from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends, Query

from .esquemas import DiaCalendario, ParametrosEntrada, ParametrosSalida
from .repositorio import RepositorioParametros
from .servicio import ServicioParametros


def crear_enrutador(obtener_repositorio: Callable[[], Iterator[RepositorioParametros]],
                    exigir_permiso: Callable[..., Callable], exigir_csrf: Callable) -> APIRouter:
    enrutador = APIRouter(tags=["parametros"])

    def servicio(repositorio=Depends(obtener_repositorio)):
        return ServicioParametros(repositorio)

    @enrutador.get("/parametros", response_model=ParametrosSalida, response_model_by_alias=True)
    def obtener(_=Depends(exigir_permiso("parametros.leer")), s=Depends(servicio)):
        return s.obtener()

    @enrutador.put("/parametros", response_model=ParametrosSalida, response_model_by_alias=True)
    def guardar(datos: ParametrosEntrada, _=Depends(exigir_permiso("parametros.editar")),
                __=Depends(exigir_csrf), s=Depends(servicio)):
        return s.guardar(datos)

    @enrutador.get("/calendario", response_model=list[DiaCalendario])
    def calendario(anio: int = Query(ge=2000, le=2200), mes: int = Query(ge=1, le=12),
                   _=Depends(exigir_permiso("calendario.leer")), s=Depends(servicio)):
        return s.calendario(anio, mes)

    return enrutador
