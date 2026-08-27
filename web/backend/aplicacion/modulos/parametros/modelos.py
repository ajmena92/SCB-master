"""Modelos ORM del dominio de parámetros operativos."""

from datetime import datetime

from sqlalchemy import DateTime, Integer
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class ParametroOperativo(BaseDeclarativa):
    """Parámetro administrado por este dominio y almacenado en esquema comedor."""

    __tablename__ = "parametro"
    __table_args__ = {"schema": "comedor"}
    id_parametro: Mapped[int] = mapped_column(Integer, primary_key=True)
    minutos_aviso_previo: Mapped[int] = mapped_column(Integer, nullable=False)
    actualizado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
