from pydantic import BaseModel, ConfigDict, Field


class SolicitudEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    asunto: str = Field(min_length=3, max_length=160)
    detalle: str = Field(min_length=3, max_length=2000)


class SolicitudSalida(SolicitudEntrada):
    id_solicitud: int = Field(serialization_alias="idSolicitud")
    estado: str
    creado_por: int = Field(serialization_alias="creadoPor")
