from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class EventoEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    modulo: str = Field(min_length=1, max_length=80)
    accion: str = Field(min_length=1, max_length=80)
    entidad: str = Field(min_length=1, max_length=100)
    id_entidad: str | None = Field(default=None, serialization_alias="idEntidad")
    detalle: dict[str, object] = {}


class EventoSalida(EventoEntrada):
    id_evento: int = Field(serialization_alias="idEvento")
    id_usuario: int | None = Field(serialization_alias="idUsuario")
    creado_en: datetime = Field(serialization_alias="creadoEn")
    direccion_ip: str | None = Field(default=None, serialization_alias="direccionIp")
