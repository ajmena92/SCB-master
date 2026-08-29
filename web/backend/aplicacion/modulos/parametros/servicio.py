from .esquemas import DiaCalendario, ParametrosEntrada, ParametrosSalida
from .repositorio import RepositorioParametros


class ServicioParametros:
    def __init__(self, repositorio: RepositorioParametros) -> None:
        self._repositorio = repositorio

    def obtener(self) -> ParametrosSalida:
        return ParametrosSalida(**self._repositorio.obtener())

    def guardar(self, datos: ParametrosEntrada) -> ParametrosSalida:
        return ParametrosSalida(
            **self._repositorio.guardar(
                datos.minutos_aviso_previo,
                [horario.model_dump(by_alias=False) for horario in datos.horarios],
                datos.permitir_marca_tardia,
                datos.permitir_sin_marca_transporte,
            )
        )

    def calendario(self, anio: int, mes: int) -> list[DiaCalendario]:
        return [DiaCalendario(**fila) for fila in self._repositorio.calendario(anio, mes)]
