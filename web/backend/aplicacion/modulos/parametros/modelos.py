"""Modelos ORM del dominio de parámetros operativos."""

from datetime import datetime

from sqlalchemy import Boolean, DateTime, Integer, String, Time
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class ParametroOperativo(BaseDeclarativa):
    """Parámetro administrado por este dominio y almacenado en esquema comedor."""

    __tablename__ = "parametro"
    __table_args__ = {"schema": "comedor"}
    id_parametro: Mapped[int] = mapped_column(Integer, primary_key=True)
    minutos_aviso_previo: Mapped[int] = mapped_column(Integer, nullable=False)
    permitir_marca_tardia: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    permitir_sin_marca_transporte: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    actualizado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class HorarioOperacion(BaseDeclarativa):
    """Configuración de horario y hora límite aplicada por el comedor."""

    __tablename__ = "horario_operacion"
    __table_args__ = {"schema": "comedor"}
    id_horario: Mapped[int] = mapped_column(Integer, primary_key=True)
    codigo: Mapped[str] = mapped_column(String(20), nullable=False, unique=True)
    descripcion: Mapped[str] = mapped_column(String(100), nullable=False)
    hora_limite: Mapped[object] = mapped_column(Time, nullable=False)
    origen: Mapped[str] = mapped_column(String(30), nullable=False, default="configuracion_web")
    hora_limite_origen: Mapped[object | None] = mapped_column(Time, nullable=True)
    id_horario_origen: Mapped[int | None] = mapped_column(Integer, nullable=True)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    actualizado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
