"""Modelos ORM del dominio de soporte."""

from sqlalchemy import Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class SolicitudSoporte(BaseDeclarativa):
    __tablename__ = "solicitud"
    __table_args__ = {"schema": "soporte"}
    id_solicitud: Mapped[int] = mapped_column(Integer, primary_key=True)
    asunto: Mapped[str] = mapped_column(String(200), nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)
