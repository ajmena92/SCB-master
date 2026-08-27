from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends

from .esquemas import PlantillaMenuEntrada, PlantillaMenuSalida
from .repositorio import RepositorioMenu
from .servicio import ServicioMenu


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioMenu]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
) -> APIRouter:
    r = APIRouter(prefix="/menu", tags=["menu"])

    def servicio(repo=Depends(obtener_repositorio)):
        return ServicioMenu(repo)

    @r.get("/plantillas", response_model=list[PlantillaMenuSalida], response_model_by_alias=True)
    def listar(_=Depends(exigir_permiso("menu.leer")), s=Depends(servicio)):
        return s.listar()

    @r.post("/plantillas", response_model=PlantillaMenuSalida, response_model_by_alias=True)
    def guardar(
        datos: PlantillaMenuEntrada,
        u: dict = Depends(exigir_permiso("menu.editar")),
        _=Depends(exigir_csrf),
        s=Depends(servicio),
    ):
        return s.guardar(datos, int(u["idUsuario"]))

    return r
