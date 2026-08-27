"""Modelos ORM del dominio de beneficios."""

from sqlalchemy import Boolean, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class Beneficio(BaseDeclarativa):
    __tablename__ = "beneficio"
    __table_args__ = {"schema": "beneficios"}
    id_beneficio: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120), nullable=False)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class BeneficioEstudiante(BaseDeclarativa):
    __tablename__ = "beneficio_estudiante"
    __table_args__ = {"schema": "beneficios"}
    id_beneficio: Mapped[int] = mapped_column(
        ForeignKey("beneficios.beneficio.id_beneficio", name="fk_beneficio_estudiante_beneficio"),
        primary_key=True,
    )
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey(
            "estudiantes.estudiante.id_estudiante", name="fk_beneficio_estudiante_estudiante"
        ),
        primary_key=True,
    )
    beneficio = relationship("Beneficio")
