from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends

from .esquemas import RegistroComedorEntrada, RegistroComedorSalida
from .repositorio import RepositorioComedor
from .servicio import ServicioComedor


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioComedor]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
) -> APIRouter:
    r = APIRouter(prefix="/comedor", tags=["comedor"])

    def servicio(repo=Depends(obtener_repositorio)):
        return ServicioComedor(repo)

    @r.post("/registros", response_model=RegistroComedorSalida, response_model_by_alias=True)
    def registrar(
        d: RegistroComedorEntrada,
        u: dict = Depends(exigir_permiso("comedor.registrar")),
        _=Depends(exigir_csrf),
        s=Depends(servicio),
    ):
        return s.registrar(d, int(u["idUsuario"]))

    return r
