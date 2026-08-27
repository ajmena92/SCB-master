"""Modelos ORM del dominio de transporte."""

from __future__ import annotations

from typing import TYPE_CHECKING

from sqlalchemy import Boolean, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa

if TYPE_CHECKING:
    from aplicacion.modulos.estudiantes.modelos import Estudiante


class Ruta(BaseDeclarativa):
    __tablename__ = "ruta"
    __table_args__ = {"schema": "transporte"}
    id_ruta: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120), nullable=False)
    activa: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    estudiantes: Mapped[list["Estudiante"]] = relationship("Estudiante", back_populates="ruta")
