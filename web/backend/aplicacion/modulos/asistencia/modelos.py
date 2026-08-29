"""Modelos ORM del dominio de asistencia."""

from __future__ import annotations

from datetime import date, datetime

from sqlalchemy import Boolean, Date, DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class MarcaAsistencia(BaseDeclarativa):
    __tablename__ = "marca"
    __table_args__ = {"schema": "asistencia"}
    id_marca: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_marca_estudiante"),
        nullable=False,
    )
    fecha: Mapped[date] = mapped_column(Date, nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)
    observacion: Mapped[str | None] = mapped_column(String(500), nullable=True)
    corregida: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    creado_por: Mapped[int] = mapped_column(Integer, nullable=False)
    actualizado_por: Mapped[int | None] = mapped_column(Integer, nullable=True)
    direccion_ip: Mapped[str] = mapped_column(String(64), nullable=False)
    fecha_creacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    fecha_actualizacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    estudiante = relationship("Estudiante")
