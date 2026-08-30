from datetime import date

from pydantic import BaseModel, ConfigDict, Field


class ParametrosSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    minutos_aviso_previo: int = Field(serialization_alias="minutosAvisoPrevio")
    permitir_marca_tardia: bool = Field(default=False, serialization_alias="permitirMarcaTardia")
    permitir_sin_marca_transporte: bool = Field(
        default=True, serialization_alias="permitirSinMarcaTransporte"
    )
    horarios: list["HorarioSalida"] = Field(default_factory=list)


class ParametrosEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    minutos_aviso_previo: int = Field(alias="minutosAvisoPrevio", ge=1, le=120)
    permitir_marca_tardia: bool = Field(default=False, alias="permitirMarcaTardia")
    permitir_sin_marca_transporte: bool = Field(default=True, alias="permitirSinMarcaTransporte")
    horarios: list["HorarioEntrada"] = Field(default_factory=list, max_length=10)


class HorarioSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_horario: int = Field(serialization_alias="idHorario")
    codigo: str
    descripcion: str
    hora_limite: str = Field(serialization_alias="horaLimite")
    activo: bool


class HorarioEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_horario: int = Field(alias="idHorario", ge=1)
    hora_limite: str = Field(alias="horaLimite", pattern=r"^([01]\d|2[0-3]):[0-5]\d$")


class DiaCalendario(BaseModel):
    fecha: date
    habilitado: bool
