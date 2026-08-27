"""Contratos HTTP del dominio de beneficios."""

from pydantic import BaseModel, ConfigDict, Field


class BeneficioEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    nombre: str = Field(min_length=1, max_length=100)
    descripcion: str | None = Field(default=None, max_length=500)
    dias_permitidos: int = Field(default=5, alias="diasPermitidos", ge=0, le=7)
    activo: bool = True


class BeneficioSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_beneficio: int = Field(serialization_alias="idBeneficio")
    nombre: str
    descripcion: str | None
    dias_permitidos: int = Field(serialization_alias="diasPermitidos")
    activo: bool


class AsignacionEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_beneficio: int | None = Field(default=None, alias="idBeneficio", ge=1)


class AsignacionSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_estudiante: int = Field(serialization_alias="idEstudiante")
    id_beneficio: int | None = Field(serialization_alias="idBeneficio")
