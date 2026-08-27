from .esquemas import PlantillaMenuEntrada, PlantillaMenuSalida
from .repositorio import RepositorioMenu


class ServicioMenu:
    def __init__(self, repositorio: RepositorioMenu) -> None:
        self._repositorio = repositorio

    def listar(self) -> list[PlantillaMenuSalida]:
        return [PlantillaMenuSalida(**x) for x in self._repositorio.listar()]

    def guardar(self, datos: PlantillaMenuEntrada, usuario: int) -> PlantillaMenuSalida:
        return PlantillaMenuSalida(**self._repositorio.guardar(datos.model_dump(), usuario))
