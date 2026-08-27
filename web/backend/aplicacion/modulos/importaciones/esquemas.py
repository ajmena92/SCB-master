from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class ErrorFila(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    fila: int
    mensaje: str


class Previsualizacion(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    cabeceras: list[str]
    filas: list[dict[str, str]]
    errores: list[ErrorFila]
    total_filas: int = Field(serialization_alias="totalFilas")
    valida: bool


class LoteSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_lote: int = Field(serialization_alias="idLote")
    nombre_archivo: str = Field(serialization_alias="nombreArchivo")
    estado: str
    total_filas: int = Field(serialization_alias="totalFilas")
    errores: list[ErrorFila]
    creado_en: datetime = Field(serialization_alias="creadoEn")
    revertido_en: datetime | None = Field(default=None, serialization_alias="revertidoEn")
