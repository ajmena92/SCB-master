"""Casos de uso de cuentas y saldos."""

from .esquemas import MovimientoEntrada, MovimientoSalida, SaldoSalida
from .repositorio import RepositorioCuentas


class ServicioCuentas:
    def __init__(self, repositorio: RepositorioCuentas) -> None:
        self._repositorio = repositorio

    def saldo(self, id_estudiante: int) -> SaldoSalida:
        if id_estudiante < 1:
            raise ValueError("El estudiante no es válido")
        return SaldoSalida(**self._repositorio.saldo(id_estudiante))

    def registrar_movimiento(
        self, id_estudiante: int, datos: MovimientoEntrada, id_usuario: int, ip: str
    ) -> MovimientoSalida:
        if id_estudiante < 1:
            raise ValueError("El estudiante no es válido")
        valores = datos.model_dump()
        return MovimientoSalida(
            **self._repositorio.movimiento(id_estudiante, valores, id_usuario, ip)
        )
