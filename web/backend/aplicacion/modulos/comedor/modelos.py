"""Modelos ORM del dominio de comedor."""

from sqlalchemy import Boolean, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class ServicioComedor(BaseDeclarativa):
    __tablename__ = "servicio"
    __table_args__ = {"schema": "comedor"}
    id_servicio: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(100), nullable=False)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
