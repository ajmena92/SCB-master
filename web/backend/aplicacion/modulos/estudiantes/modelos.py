"""Modelos ORM del dominio de estudiantes."""

from __future__ import annotations

from datetime import datetime
from typing import TYPE_CHECKING

from sqlalchemy import Boolean, DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa

if TYPE_CHECKING:
    from aplicacion.modulos.transporte.modelos import Ruta


class Estudiante(BaseDeclarativa):
    __tablename__ = "estudiante"
    __table_args__ = {"schema": "estudiantes"}
    id_estudiante: Mapped[int] = mapped_column(Integer, primary_key=True)
    carne: Mapped[str] = mapped_column(String(30), unique=True, nullable=False)
    nombre: Mapped[str] = mapped_column(String(100), nullable=False)
    primer_apellido: Mapped[str] = mapped_column(String(100), nullable=False)
    segundo_apellido: Mapped[str | None] = mapped_column(String(100), nullable=True)
    cedula: Mapped[str | None] = mapped_column(String(30), nullable=True)
    seccion: Mapped[str | None] = mapped_column(String(30), nullable=True)
    turno: Mapped[str | None] = mapped_column(String(30), nullable=True)
    hash_contrasena: Mapped[str | None] = mapped_column(String(255), nullable=True)
    debe_cambiar_pin: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    fecha_expiracion_pin: Mapped[datetime | None] = mapped_column(DateTime, nullable=True)
    id_ruta: Mapped[int | None] = mapped_column(
        ForeignKey("transporte.ruta.id_ruta", name="fk_estudiante_ruta"), nullable=True
    )
    id_beneficio: Mapped[int | None] = mapped_column(Integer, nullable=True)
    ruta: Mapped["Ruta | None"] = relationship("Ruta", back_populates="estudiantes")


class FotografiaEstudiante(BaseDeclarativa):
    __tablename__ = "fotografia"
    __table_args__ = {"schema": "estudiantes"}
    id_fotografia: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_fotografia_estudiante"),
        nullable=False,
        unique=True,
    )
    contenido: Mapped[bytes] = mapped_column(nullable=False)
    tipo_contenido: Mapped[str] = mapped_column(String(80), nullable=False)
