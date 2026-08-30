from .esquemas import (
    PlantillaMenuEntrada,
    PlantillaMenuSalida,
    SustitucionMenuEntrada,
    SustitucionMenuSalida,
)
from .repositorio import RepositorioMenu


class ServicioMenu:
    def __init__(self, repositorio: RepositorioMenu) -> None:
        self._repositorio = repositorio

    def listar(self) -> list[PlantillaMenuSalida]:
        return [PlantillaMenuSalida(**x) for x in self._repositorio.listar()]

    def guardar(self, datos: PlantillaMenuEntrada, usuario: int) -> PlantillaMenuSalida:
        return PlantillaMenuSalida(**self._repositorio.guardar(datos.model_dump(), usuario))

    def listar_sustituciones(self) -> list[SustitucionMenuSalida]:
        return [SustitucionMenuSalida(**fila) for fila in self._repositorio.listar_sustituciones()]

    def guardar_sustitucion(
        self, datos: SustitucionMenuEntrada, usuario: int
    ) -> SustitucionMenuSalida:
        return SustitucionMenuSalida(
            **self._repositorio.guardar_sustitucion(datos.model_dump(), usuario)
        )
