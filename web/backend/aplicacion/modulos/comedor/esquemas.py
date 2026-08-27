from pydantic import BaseModel, ConfigDict, Field


class RegistroComedorEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_estudiante: int = Field(alias="idEstudiante", ge=1)
    fecha: str


class RegistroComedorSalida(RegistroComedorEntrada):
    id_registro: int = Field(serialization_alias="idRegistro")
    registrado_por: int = Field(serialization_alias="registradoPor")
