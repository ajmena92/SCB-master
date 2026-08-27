"""Modelos ORM del dominio de cuentas."""

from sqlalchemy import ForeignKey, Integer
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class CuentaEstudiante(BaseDeclarativa):
    __tablename__ = "cuenta_estudiante"
    __table_args__ = {"schema": "cuentas"}
    id_cuenta: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_cuenta_estudiante"),
        nullable=False,
        unique=True,
    )
    saldo: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    estudiante = relationship("Estudiante")
