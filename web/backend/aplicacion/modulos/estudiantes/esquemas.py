"""Contratos del dominio de estudiantes."""

from typing import Literal

from pydantic import BaseModel, ConfigDict, Field


class EstudianteEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    carne: str = Field(min_length=1, max_length=30)
    nombre: str = Field(min_length=1, max_length=100)
    primer_apellido: str = Field(alias="primerApellido", min_length=1, max_length=100)
    segundo_apellido: str | None = Field(default=None, alias="segundoApellido", max_length=100)
    cedula: str | None = Field(default=None, max_length=30)
    seccion: str | None = Field(default=None, max_length=30)
    activo: bool = True


class EstudianteSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_estudiante: int = Field(serialization_alias="idEstudiante")
    carne: str
    nombre: str
    primer_apellido: str = Field(serialization_alias="primerApellido")
    segundo_apellido: str | None = Field(serialization_alias="segundoApellido")
    cedula: str | None
    seccion: str | None
    turno: str | None = None
    id_ruta: int | None = Field(default=None, alias="idRuta")
    ruta_codigo: str | None = Field(default=None, alias="rutaCodigo")
    ruta_descripcion: str | None = Field(default=None, alias="rutaDescripcion")
    id_estado_comedor: Literal[1, 2] = Field(default=2, alias="idEstadoComedor")
    beneficio_comedor: str = Field(default="No beneficiario", alias="beneficioComedor")
    id_beneficio: int | None = Field(default=None, alias="idBeneficio")
    bloqueado: bool = False
    debe_cambiar_pin: bool = Field(default=False, alias="debeCambiarPin")
    tiene_foto: bool = Field(default=False, alias="tieneFoto")
    activo: bool


class PaginaEstudiantes(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    elementos: list[EstudianteSalida]
    pagina: int
    tamano: int
    total: int


class CambioAsignacion(BaseModel):
    id_beneficio: int | None = Field(default=None, alias="idBeneficio", ge=1)
    id_ruta: int | None = Field(default=None, alias="idRuta", ge=1)


class CambioEstadoComedor(BaseModel):
    id_estado_comedor: Literal[1, 2] = Field(alias="idEstadoComedor")


class GeneracionPinesSeccion(BaseModel):
    seccion: str | None = Field(default=None, max_length=30)
    turno: str | None = Field(default=None, max_length=30)


class PinGenerado(BaseModel):
    id_estudiante: int = Field(alias="idEstudiante")
    pin: str


class AccesoEstudiante(BaseModel):
    carne: str = Field(min_length=1, max_length=30)
    pin: str = Field(pattern=r"^\d{6}$")


class CambioPinEstudiante(BaseModel):
    pin_actual: str = Field(alias="pinActual", pattern=r"^\d{6}$")
    pin_nuevo: str = Field(alias="pinNuevo", pattern=r"^\d{6}$")
