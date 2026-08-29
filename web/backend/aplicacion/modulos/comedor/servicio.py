"""Casos de uso del comedor."""

from datetime import date

from .esquemas import (
    CuentaTiquetesSalida,
    IngresoSalida,
    MovimientoTiquetesSalida,
    PersonaComedorSalida,
    ProfesorComedorEntrada,
    ReservaSalida,
    TiquetesEntrada,
)
from .repositorio import RepositorioComedor


class ServicioComedor:
    def __init__(self, repositorio: RepositorioComedor) -> None:
        self._repositorio = repositorio

    def personas(
        self, tipo_persona: str | None = None, incluir_inactivas: bool = False
    ) -> list[PersonaComedorSalida]:
        if tipo_persona not in (None, "estudiante", "profesor"):
            raise ValueError("El tipo de persona no es válido")
        return [
            PersonaComedorSalida(**fila)
            for fila in self._repositorio.personas(tipo_persona, incluir_inactivas)
        ]

    def profesor_portal(self, id_usuario: int) -> PersonaComedorSalida:
        persona = self._repositorio.persona_por_usuario(id_usuario)
        return PersonaComedorSalida(**persona)

    def estado_reserva(self, id_persona: int, fecha: date) -> ReservaSalida | None:
        reserva = self._repositorio.reserva_por_persona_fecha(id_persona, fecha)
        return ReservaSalida(**reserva) if reserva is not None else None

    def crear_profesor(self, datos: ProfesorComedorEntrada) -> PersonaComedorSalida:
        nombre = " ".join(datos.nombre_completo.split())
        if not nombre:
            raise ValueError("El nombre del profesor es obligatorio")
        return PersonaComedorSalida(
            **self._repositorio.crear_profesor(datos.id_usuario, nombre, datos.colegio)
        )

    def cuenta(self, id_persona: int) -> CuentaTiquetesSalida:
        return CuentaTiquetesSalida(**self._repositorio.cuenta(id_persona))

    def movimientos(self, id_persona: int, limite: int) -> list[MovimientoTiquetesSalida]:
        return [
            MovimientoTiquetesSalida(**fila)
            for fila in self._repositorio.movimientos(id_persona, limite)
        ]

    def recargar(
        self, id_persona: int, datos: TiquetesEntrada, usuario: int
    ) -> MovimientoTiquetesSalida:
        if id_persona < 1:
            raise ValueError("La persona no es válida")
        persona = self._repositorio.persona(id_persona)
        if persona["id_estado_comedor"] == 1:
            raise ValueError("Las personas becadas no compran ni reciben tiquetes")
        return MovimientoTiquetesSalida(
            **self._repositorio.recargar(
                id_persona,
                datos.cantidad,
                datos.concepto,
                datos.clave_idempotencia,
                usuario,
            )
        )

    def reservar(self, id_persona: int, fecha: date, usuario: int | None) -> ReservaSalida:
        return ReservaSalida(**self._repositorio.reservar(id_persona, fecha, usuario))

    def reservar_estudiante(
        self, id_estudiante: int, fecha: date, usuario: int | None
    ) -> ReservaSalida:
        id_persona = self._repositorio.persona_por_estudiante(id_estudiante)
        return self.reservar(id_persona, fecha, usuario)

    def cancelar(self, id_persona: int, fecha: date, usuario: int | None) -> ReservaSalida:
        return ReservaSalida(**self._repositorio.cancelar(id_persona, fecha, usuario))

    def cancelar_estudiante(
        self, id_estudiante: int, fecha: date, usuario: int | None
    ) -> ReservaSalida:
        id_persona = self._repositorio.persona_por_estudiante(id_estudiante)
        return self.cancelar(id_persona, fecha, usuario)

    def ingresar(
        self,
        codigo_barras: str,
        fecha: date,
        usuario: int | None,
        terminal_id: str | None = None,
    ) -> IngresoSalida:
        return IngresoSalida(
            **self._repositorio.ingresar(codigo_barras, fecha, usuario, terminal_id)
        )
