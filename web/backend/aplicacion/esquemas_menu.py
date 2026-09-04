"""Contratos de plantillas, calendario y publicaciones de menú."""

from datetime import date

from pydantic import Field, field_validator

from aplicacion.esquemas_base import Contrato


class CicloMenuEntrada(Contrato):
    inicio_ciclo_menu: date

    @field_validator("inicio_ciclo_menu")
    @classmethod
    def validar_lunes(cls, valor: date) -> date:
        if valor.isoweekday() != 1:
            raise ValueError("El inicio del ciclo PANEA debe ser lunes")
        return valor


class ComponenteMenuEntrada(Contrato):
    nombre: str = Field(min_length=1, max_length=180)
    tipo: str = Field(default="Principal", min_length=1, max_length=40)
    orden: int = Field(ge=1, le=20)


class PlantillaEntrada(Contrato):
    semana: int = Field(ge=1, le=5)
    dia: int = Field(ge=1, le=5)
    titulo: str = Field(min_length=1, max_length=180)
    observaciones: str | None = Field(default=None, max_length=2000)
    activo: bool = True
    componentes: list[ComponenteMenuEntrada] = Field(min_length=1, max_length=20)


class PublicacionEntrada(Contrato):
    fecha: date


class CalendarioMenuEntrada(Contrato):
    fecha: date
    habilitado: bool
    motivo: str | None = Field(default=None, max_length=300)


class SustitucionMenuEntrada(Contrato):
    fecha: date
    titulo: str = Field(min_length=1, max_length=180)
    observaciones: str | None = Field(default=None, max_length=2000)
    componentes: list[ComponenteMenuEntrada] = Field(min_length=1, max_length=20)
