"""Contratos de identidad, cuentas administrativas e importación."""

from datetime import datetime
from typing import Literal

from pydantic import Field, field_validator, model_validator

from aplicacion.esquemas_base import Contrato
from aplicacion.secciones import normalizar_seccion

PIN_COMUNES = frozenset({"012345", "123456", "987654"})


def pin_debil(valor: str) -> bool:
    """Detecta PIN obvios que no deben usarse como credencial personal."""
    return valor in PIN_COMUNES or len(set(valor)) == 1


class CambioPinEntrada(Contrato):
    pin_actual: str = Field(pattern=r"^\d{6}$")
    pin_nuevo: str = Field(pattern=r"^\d{6}$")

    @field_validator("pin_nuevo")
    @classmethod
    def validar_pin_seguro(cls, valor: str) -> str:
        if pin_debil(valor):
            raise ValueError("El PIN nuevo no puede ser obvio o repetitivo")
        return valor


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
    contrasena_nueva: str = Field(min_length=10, max_length=128)


class SesionSalida(Contrato):
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

    @field_validator("seccion", mode="before")
    @classmethod
    def normalizar_seccion_importada(cls, valor: str | None) -> str | None:
        return normalizar_seccion(str(valor) if valor is not None else None)


class ImportacionEntrada(Contrato):
    anio: int = Field(ge=2000, le=2200)
    filas: list[FilaImportacion] = Field(min_length=1, max_length=5000)


class ConfirmacionImportacion(ImportacionEntrada):
    huella: str
