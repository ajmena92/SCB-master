"""Modelos ORM del dominio de auditoría."""

from datetime import datetime

from sqlalchemy import DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class EventoAuditoria(BaseDeclarativa):
    __tablename__ = "evento"
    __table_args__ = {"schema": "auditoria"}
    id_evento: Mapped[int] = mapped_column(Integer, primary_key=True)
    accion: Mapped[str] = mapped_column(String(100), nullable=False)
    id_usuario: Mapped[int | None] = mapped_column(
        ForeignKey("identidad.usuario.id_usuario", name="fk_evento_usuario"), nullable=True
    )
    fecha_hora: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    usuario = relationship("Usuario")
