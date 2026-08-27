"""Modelos ORM del dominio de reportes."""

from datetime import datetime

from sqlalchemy import DateTime, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class Reporte(BaseDeclarativa):
    __tablename__ = "reporte"
    __table_args__ = {"schema": "reportes"}
    id_reporte: Mapped[int] = mapped_column(Integer, primary_key=True)
    tipo: Mapped[str] = mapped_column(String(80), nullable=False)
    fecha_generacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class ResumenOperativo(BaseDeclarativa):
    __tablename__ = "resumen"
    __table_args__ = {"schema": "reportes"}
    id_resumen: Mapped[int] = mapped_column(Integer, primary_key=True)
    fecha: Mapped[datetime] = mapped_column(DateTime, nullable=False, unique=True)
    estudiantes: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    confirmaciones: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    cancelaciones: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
