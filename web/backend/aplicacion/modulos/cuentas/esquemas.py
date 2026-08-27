"""Contratos HTTP del dominio de cuentas."""

from datetime import datetime
from decimal import Decimal
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field

TipoMovimiento = Literal["recarga", "consumo", "ajuste"]


class MovimientoEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    tipo: TipoMovimiento
    monto: Decimal = Field(gt=0, max_digits=12, decimal_places=2)
    clave_idempotencia: str = Field(alias="claveIdempotencia", min_length=8, max_length=100)
    concepto: str | None = Field(default=None, max_length=250)


class SaldoSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_cuenta: int = Field(serialization_alias="idCuenta")
    id_estudiante: int = Field(serialization_alias="idEstudiante")
    saldo: Decimal
    actualizado_en: datetime = Field(serialization_alias="actualizadoEn")


class MovimientoSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_movimiento: int = Field(serialization_alias="idMovimiento")
    id_cuenta: int = Field(serialization_alias="idCuenta")
    tipo: TipoMovimiento
    monto: Decimal
    saldo_anterior: Decimal = Field(serialization_alias="saldoAnterior")
    saldo_nuevo: Decimal = Field(serialization_alias="saldoNuevo")
    clave_idempotencia: str = Field(serialization_alias="claveIdempotencia")
    concepto: str | None
    creado_en: datetime = Field(serialization_alias="creadoEn")
