"""Modelos ORM del dominio de menú."""

from datetime import datetime

from sqlalchemy import Boolean, DateTime, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class Menu(BaseDeclarativa):
    __tablename__ = "menu"
    __table_args__ = {"schema": "menu"}
    id_menu: Mapped[int] = mapped_column(Integer, primary_key=True)
    fecha: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    descripcion: Mapped[str] = mapped_column(String(500), nullable=False)


class CalendarioMenu(BaseDeclarativa):
    __tablename__ = "calendario"
    __table_args__ = {"schema": "menu"}
    id_calendario: Mapped[int] = mapped_column(Integer, primary_key=True)
    fecha: Mapped[datetime] = mapped_column(DateTime, nullable=False, unique=True)
    habilitado: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
