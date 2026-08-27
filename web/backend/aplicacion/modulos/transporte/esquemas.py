"""Contratos HTTP del modulo de transporte."""

from pydantic import BaseModel, ConfigDict, Field


class RutaEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    codigo: str = Field(min_length=1, max_length=50)
    descripcion: str = Field(min_length=6, max_length=500)
    color_hex: str = Field(alias="colorHex", min_length=7, max_length=7)
    activo: bool = True


class RutaSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_ruta: int = Field(serialization_alias="idRuta")
    codigo: str
    descripcion: str
    activo: bool
    color_carnet_hex: str = Field(serialization_alias="colorCarnetHex")
    estudiantes_asignados: int = Field(default=0, serialization_alias="estudiantesAsignados")
