"""Contratos HTTP v1 con nombres JSON en camelCase."""

from __future__ import annotations

from datetime import date, datetime
from decimal import Decimal
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field, field_validator, model_validator


def _camel(nombre: str) -> str:
    partes = nombre.split("_")
    return partes[0] + "".join(parte.title() for parte in partes[1:])


class Contrato(BaseModel):
    model_config = ConfigDict(
        alias_generator=_camel,
        populate_by_name=True,
        from_attributes=True,
        extra="forbid",
    )


class PersonaEntrada(Contrato):
    cedula: str | None = None
    nombres: str = Field(min_length=2, max_length=180)
    tipo: Literal["estudiante", "profesor"]
    activo: bool = True


class PersonaSalida(Contrato):
    id: int
    codigo: str
    cedula: str | None
    nombres: str
    tipo: str
    activo: bool
    pin_temporal: str | None = None


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


class PlantillaEntrada(Contrato):
    nombre: str = Field(min_length=1, max_length=180)
    componentes: list[str] = Field(min_length=1)


class PublicacionEntrada(Contrato):
    plantilla_id: int
    fecha: date


class TarifaEntrada(Contrato):
    tipo_persona: Literal["estudiante", "profesor"]
    monto: Decimal = Field(ge=0)
    fecha_inicio: date
    fecha_fin: date | None = None


class VentaEntrada(Contrato):
    codigo: str
    cantidad: int = Field(gt=0)
    medio_pago: str = "efectivo"


class ReservaEntrada(Contrato):
    codigo: str | None = None
    fecha: date


class CancelacionReservaEntrada(Contrato):
    codigo: str | None = None
    fecha: date


class AutorizacionEntrada(Contrato):
    codigo: str
    fecha: date
    decision: Literal["aprobada", "rechazada"]
    motivo: str | None = None


class IngresoEntrada(Contrato):
    codigo: str
    fecha: date


class MarcaTransporteEntrada(Contrato):
    codigo: str
    fecha: date


class CambioPinEntrada(Contrato):
    pin_actual: str = Field(pattern=r"^\d{6}$")
    pin_nuevo: str = Field(pattern=r"^\d{6}$")


class PortalEntrada(Contrato):
    cedula: str
    pin: str


class AdministracionEntrada(Contrato):
    usuario: str
    contrasena: str


class ProfesorNuevoAdministrativo(Contrato):
    cedula: str = Field(min_length=1, max_length=32)
    nombres: str = Field(min_length=2, max_length=180)


class CuentaAdministrativaEntrada(Contrato):
    persona_id: int | None = None
    profesor_nuevo: ProfesorNuevoAdministrativo | None = None
    usuario: str = Field(min_length=3, max_length=80, pattern=r"^[a-zA-Z0-9._-]+$")
    rol: Literal["administrador", "operador"]
    permisos: list[str] = Field(default_factory=list)

    @field_validator("usuario", mode="before")
    @classmethod
    def normalizar_usuario(cls, valor):
        return valor.strip().lower() if isinstance(valor, str) else valor

    @model_validator(mode="after")
    def validar_origen_profesor(self):
        if (self.persona_id is None) != (self.profesor_nuevo is None):
            return self
        raise ValueError("Debe indicar exactamente un profesor existente o uno nuevo")


class CuentaAdministrativaActualizacion(Contrato):
    usuario: str | None = Field(
        default=None, min_length=3, max_length=80, pattern=r"^[a-zA-Z0-9._-]+$"
    )
    rol: Literal["administrador", "operador"] | None = None
    activo: bool | None = None
    permisos: list[str] | None = None
    persona_id: int | None = None

    @field_validator("usuario", mode="before")
    @classmethod
    def normalizar_usuario(cls, valor):
        return valor.strip().lower() if isinstance(valor, str) else valor


class VinculacionCuentaEntrada(Contrato):
    persona_id: int | None = None
    profesor_nuevo: ProfesorNuevoAdministrativo | None = None

    @model_validator(mode="after")
    def validar_origen_profesor(self):
        if (self.persona_id is None) != (self.profesor_nuevo is None):
            return self
        raise ValueError("Debe indicar exactamente un profesor existente o uno nuevo")


class CambioContrasenaAdministrativaEntrada(Contrato):
    contrasena_actual: str
    contrasena_nueva: str = Field(min_length=12, max_length=128)


class SesionSalida(Contrato):
    token: str
    tipo: str
    rol: str | None = None
    persona_id: int | None = None
    cuenta_id: int | None = None
    nombres: str | None = None
    usuario: str | None = None
    permisos: list[str] = Field(default_factory=list)
    vinculacion_pendiente: bool = False
    cambio_obligatorio: bool = False
    cambio_contrasena_obligatorio: bool = False
    expira_en: datetime


class FilaImportacion(Contrato):
    cedula: str | None = None
    nombres: str
    tipo: Literal["estudiante", "profesor"]
    seccion: str | None = None
    becado: bool = False
    ruta: str | None = None


class ImportacionEntrada(Contrato):
    anio: int
    filas: list[FilaImportacion]


class ConfirmacionImportacion(ImportacionEntrada):
    huella: str
