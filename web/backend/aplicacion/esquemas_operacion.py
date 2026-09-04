"""Contratos de comedor, transporte y configuración institucional."""

from datetime import date
from decimal import Decimal
from typing import Literal

from pydantic import Field

from aplicacion.esquemas_base import Contrato


class TarifaEntrada(Contrato):
    tipo_persona: Literal["estudiante", "profesor"]
    monto: Decimal = Field(ge=0)
    fecha_inicio: date
    fecha_fin: date | None = None


class VentaEntrada(Contrato):
    cedula: str
    cantidad: int = Field(gt=0)
    medio_pago: str = "efectivo"


class HorarioReservaEntrada(Contrato):
    turno: str = Field(min_length=1, max_length=24)
    hora_limite: str = Field(pattern=r"^([01]\d|2[0-3]):[0-5]\d$")


class ConfiguracionInstitucionalEntrada(Contrato):
    nombre_colegio: str = Field(min_length=2, max_length=180)
    subtitulo_reportes: str = Field(min_length=2, max_length=220)


class ConfiguracionInstitucionalSalida(ConfiguracionInstitucionalEntrada):
    pass


class ReservaEntrada(Contrato):
    cedula: str | None = None
    fecha: date


class CancelacionReservaEntrada(Contrato):
    cedula: str | None = None
    fecha: date


class AutorizacionEntrada(Contrato):
    cedula: str
    fecha: date
    decision: Literal["aprobada", "rechazada"]
    motivo: str | None = None


class IngresoEntrada(Contrato):
    cedula: str
    fecha: date


class MarcaTransporteEntrada(Contrato):
    cedula: str
    fecha: date
