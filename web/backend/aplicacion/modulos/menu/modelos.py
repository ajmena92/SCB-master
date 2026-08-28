"""Modelos ORM del dominio de menú."""

from datetime import datetime

from sqlalchemy import Boolean, DateTime, ForeignKey, Integer, String
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


class Plantilla(BaseDeclarativa):
    __tablename__ = "plantilla"
    __table_args__ = {"schema": "menu"}
    id_plantilla: Mapped[int] = mapped_column(Integer, primary_key=True)
    semana: Mapped[int] = mapped_column(Integer, nullable=False)
    dia: Mapped[int] = mapped_column(Integer, nullable=False)
    titulo: Mapped[str] = mapped_column(String(160), nullable=False)
    observaciones: Mapped[str | None] = mapped_column(String(500), nullable=True)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    creado_por: Mapped[int] = mapped_column(Integer, nullable=False)
    actualizado_por: Mapped[int | None] = mapped_column(Integer, nullable=True)


class Componente(BaseDeclarativa):
    __tablename__ = "componente"
    __table_args__ = {"schema": "menu"}
    id_componente: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_plantilla: Mapped[int] = mapped_column(
        ForeignKey("menu.plantilla.id_plantilla"), nullable=False
    )
    nombre: Mapped[str] = mapped_column(String(500), nullable=False)
    tipo: Mapped[str] = mapped_column(String(40), nullable=False)
    orden: Mapped[int] = mapped_column(Integer, nullable=False)
