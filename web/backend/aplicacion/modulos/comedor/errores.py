"""Errores públicos del dominio de comedor."""


class ErrorOperacionComedor(ValueError):
    """Error seguro y estructurado para la operación de comedor."""

    codigo = "operacion_rechazada"

    def __init__(self, mensaje: str) -> None:
        super().__init__(mensaje)
        self.mensaje = mensaje


class CarnetNoReconocido(ErrorOperacionComedor):
    codigo = "carnet_no_reconocido"


class PersonaInactiva(ErrorOperacionComedor):
    codigo = "persona_inactiva"


class HorarioNoConfigurado(ErrorOperacionComedor):
    codigo = "horario_no_configurado"


class HoraLimiteExcedida(ErrorOperacionComedor):
    codigo = "hora_limite_excedida"


class SinMarcaTransporte(ErrorOperacionComedor):
    codigo = "sin_marca_transporte"


class SinReserva(ErrorOperacionComedor):
    codigo = "sin_reserva"


class TiqueteAgotado(ErrorOperacionComedor):
    codigo = "tiquete_agotado"


class IdempotenciaIncompatible(ValueError):
    """La misma clave fue reutilizada para una operación distinta."""


class IngresoDuplicado(ErrorOperacionComedor):
    """La persona ya tiene un ingreso registrado para la fecha indicada."""

    codigo = "ingreso_duplicado"
