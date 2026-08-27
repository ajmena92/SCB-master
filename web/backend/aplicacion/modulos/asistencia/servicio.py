"""Casos de uso de asistencia, independientes de HTTP y SQL."""

from datetime import date

from .esquemas import CorreccionEntrada, MarcaEntrada, MarcaSalida
from .repositorio import RepositorioAsistencia


def _salida(datos: dict) -> MarcaSalida:
    return MarcaSalida(**datos)


class ServicioAsistencia:
    def __init__(self, repositorio: RepositorioAsistencia) -> None:
        self._repositorio = repositorio

    def listar(self, fecha: date) -> list[MarcaSalida]:
        return [_salida(marca) for marca in self._repositorio.listar(fecha)]

    def registrar(self, datos: MarcaEntrada, id_usuario: int, ip: str) -> MarcaSalida:
        valores = datos.model_dump()
        if valores.get("observacion"):
            valores["observacion"] = " ".join(valores["observacion"].split())
        return _salida(self._repositorio.registrar(valores, id_usuario, ip))

    def corregir(
        self, id_marca: int, datos: CorreccionEntrada, id_usuario: int, ip: str
    ) -> MarcaSalida:
        motivo = " ".join(datos.motivo.split())
        if len(motivo) < 3:
            raise ValueError("El motivo de la corrección es obligatorio")
        return _salida(self._repositorio.corregir(id_marca, datos.estado, motivo, id_usuario, ip))
