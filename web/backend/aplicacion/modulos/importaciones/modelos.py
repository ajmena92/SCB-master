"""Modelos ORM del dominio de importaciones."""

from datetime import datetime

from sqlalchemy import DateTime, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class LoteImportacion(BaseDeclarativa):
    __tablename__ = "lote"
    __table_args__ = {"schema": "importaciones"}
    id_lote: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre_archivo: Mapped[str] = mapped_column(String(255), nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)
    fecha_creacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)
