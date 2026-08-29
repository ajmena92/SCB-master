"""Modelos ORM del dominio de transporte."""

from __future__ import annotations

from datetime import date, datetime
from typing import TYPE_CHECKING

from sqlalchemy import Boolean, Date, DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa

if TYPE_CHECKING:
    from aplicacion.modulos.estudiantes.modelos import Estudiante


class Ruta(BaseDeclarativa):
    __tablename__ = "ruta"
    __table_args__ = {"schema": "transporte"}
    id_ruta: Mapped[int] = mapped_column(Integer, primary_key=True)
    codigo: Mapped[str] = mapped_column(String(50), unique=True, nullable=False)
    descripcion: Mapped[str] = mapped_column(String(500), nullable=False)
    color_hex: Mapped[str] = mapped_column(String(7), nullable=False, default="#CBD5E1")
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    creado_por: Mapped[int] = mapped_column(Integer, nullable=False)
    actualizado_por: Mapped[int | None] = mapped_column(Integer, nullable=True)
    direccion_ip: Mapped[str] = mapped_column(String(64), nullable=False)
    estudiantes: Mapped[list["Estudiante"]] = relationship("Estudiante")


class AsignacionRuta(BaseDeclarativa):
    __tablename__ = "asignacion_ruta"
    __table_args__ = {"schema": "transporte"}
    id_asignacion: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_ruta: Mapped[int] = mapped_column(
        ForeignKey("transporte.ruta.id_ruta"), nullable=False
    )
    id_estudiante: Mapped[int] = mapped_column(Integer, nullable=False, unique=True)
    activa: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class UsoDiarioTransporte(BaseDeclarativa):
    """Marca diaria importada para consulta; transporte no la escribe desde comedor."""

    __tablename__ = "uso_diario"
    __table_args__ = {"schema": "transporte"}
    id_uso: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(Integer, nullable=False)
    fecha: Mapped[date] = mapped_column(Date, nullable=False)
    marcado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
