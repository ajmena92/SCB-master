"""Modelos ORM del dominio de asistencia."""

from __future__ import annotations

from datetime import datetime

from sqlalchemy import DateTime, ForeignKey, Integer, String
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
    fecha_hora: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)
    estudiante = relationship("Estudiante")
