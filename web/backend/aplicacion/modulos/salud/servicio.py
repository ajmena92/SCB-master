"""Caso de uso del modulo de salud, independiente de FastAPI."""

from aplicacion.modulos.salud.esquemas import EstadoSalud
from aplicacion.nucleo.reloj import Reloj


class ServicioSalud:
    def __init__(self, reloj: Reloj) -> None:
        self._reloj = reloj

    def consultar(self) -> EstadoSalud:
        return EstadoSalud(fecha_hora_utc=self._reloj.ahora_utc())
