from datetime import date

from pydantic import BaseModel, Field


class ParametrosSalida(BaseModel):
    minutos_aviso_previo: int = Field(serialization_alias="minutosAvisoPrevio")


class ParametrosEntrada(BaseModel):
    minutos_aviso_previo: int = Field(alias="minutosAvisoPrevio", ge=1, le=120)


class DiaCalendario(BaseModel):
    fecha: date
    habilitado: bool
