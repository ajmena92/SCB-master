"""Contratos públicos del dominio de reportes."""

from pydantic import BaseModel, ConfigDict, Field


class ReporteEstudiante(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_estudiante: int = Field(serialization_alias="idEstudiante")
    carne: str
    nombre_completo: str = Field(serialization_alias="nombreCompleto")
    seccion: str | None
    activo: bool


class ReporteRuta(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_ruta: int = Field(serialization_alias="idRuta")
    codigo: str
    descripcion: str
    estudiantes_asignados: int = Field(serialization_alias="estudiantesAsignados")
    activo: bool


class ReporteEstudiantes(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    total: int
    elementos: list[ReporteEstudiante]


class ReporteTransporte(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    total: int
    elementos: list[ReporteRuta]
