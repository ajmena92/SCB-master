"""Contratos HTTP del dominio de asistencia."""

from datetime import date
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field

EstadoAsistencia = Literal["presente", "ausente", "tardanza", "justificada"]


class MarcaEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_estudiante: int = Field(alias="idEstudiante", gt=0)
    fecha: date
    estado: EstadoAsistencia
    observacion: str | None = Field(default=None, max_length=500)


class MarcaSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_marca: int = Field(serialization_alias="idMarca")
    id_estudiante: int = Field(serialization_alias="idEstudiante")
    fecha: date
    estado: EstadoAsistencia
    observacion: str | None = None
    corregida: bool = False


class CorreccionEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    estado: EstadoAsistencia
    motivo: str = Field(min_length=3, max_length=500)
