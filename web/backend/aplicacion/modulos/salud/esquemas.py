"""Contratos publicos del modulo de salud."""

from datetime import datetime
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field


class EstadoSalud(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    estado: Literal["disponible"] = "disponible"
    fecha_hora_utc: datetime = Field(serialization_alias="fechaHoraUtc")
