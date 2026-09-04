"""Contratos de personas, matrículas, años y rutas."""

from datetime import date
from typing import Literal

from pydantic import Field

from aplicacion.esquemas_base import Contrato


class PersonaEntrada(Contrato):
    cedula: str | None = None
    nombres: str = Field(min_length=2, max_length=180)
    tipo: Literal["estudiante", "profesor"]
    activo: bool = True


class PersonaSalida(Contrato):
    id: int
    referencia_publica: str
    cedula: str | None
    nombres: str
    tipo: str
    activo: bool
    pin_temporal: str | None = None


class ResumenPersonasSalida(Contrato):
    estudiantes_activos: int
    estudiantes_inactivos: int


class PersonaActualizacionEntrada(Contrato):
    cedula: str | None = Field(default=None, max_length=32)
    nombres: str = Field(min_length=2, max_length=180)


class MatriculaBeneficioEntrada(Contrato):
    becado: bool = False


class MatriculaBeneficiosEntrada(Contrato):
    """Beneficios administrados localmente para una matrícula del padrón."""

    becado: bool = False
    ruta_id: int | None = Field(default=None, ge=1)


class CambioRutaMatriculaEntrada(Contrato):
    ruta_id: int | None = Field(default=None, ge=1)


class PinTemporalSalida(Contrato):
    persona_id: int
    cedula: str
    nombre: str
    pin_temporal: str


class GeneracionPinesSeccionEntrada(Contrato):
    anio_lectivo_id: int
    seccion: str = Field(min_length=1, max_length=40)


class AnioEntrada(Contrato):
    anio: int = Field(ge=2000, le=2200)
    vigente: bool = False


class MatriculaEntrada(Contrato):
    persona_id: int
    anio_lectivo_id: int
    seccion: str
    becado: bool = False
    estado: str = "activo"


class RutaEntrada(Contrato):
    codigo: str = Field(min_length=1, max_length=50)
    descripcion: str = Field(min_length=6, max_length=500)
    color_hex: str = Field(default="#CBD5E1", pattern=r"^#[0-9A-Fa-f]{6}$")
    activa: bool = True


class AsignacionRutaEntrada(Contrato):
    matricula_id: int
    fecha_inicio: date
    fecha_fin: date | None = None
