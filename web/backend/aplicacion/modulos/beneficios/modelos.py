"""Modelos ORM del dominio de beneficios."""

from sqlalchemy import Boolean, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class TipoBeneficio(BaseDeclarativa):
    __tablename__ = "tipo_beneficio"
    __table_args__ = {"schema": "beneficios"}
    id_beneficio: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(100), nullable=False)
    descripcion: Mapped[str | None] = mapped_column(String(500), nullable=True)
    dias_permitidos: Mapped[int] = mapped_column(Integer, nullable=False, default=5)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class AsignacionBeneficio(BaseDeclarativa):
    __tablename__ = "asignacion"
    __table_args__ = {"schema": "beneficios"}
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante"),
        primary_key=True,
    )
    id_beneficio: Mapped[int | None] = mapped_column(
        ForeignKey("beneficios.tipo_beneficio.id_beneficio"), nullable=True
    )
