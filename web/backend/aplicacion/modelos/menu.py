"""Plantillas y publicaciones inmutables del menu."""

from datetime import date

from sqlalchemy import Boolean, Date, ForeignKey, Integer, String, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class PlantillaMenu(BaseDeclarativa):
    __tablename__ = "plantilla_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120), unique=True)
    activo: Mapped[bool] = mapped_column(Boolean, default=True)


class ComponenteMenu(BaseDeclarativa):
    __tablename__ = "componente_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    plantilla_id: Mapped[int] = mapped_column(ForeignKey("plantilla_menu.id", ondelete="CASCADE"))
    nombre: Mapped[str] = mapped_column(String(180))
    orden: Mapped[int] = mapped_column(Integer)
    __table_args__ = (UniqueConstraint("plantilla_id", "orden"),)


class PublicacionMenu(BaseDeclarativa):
    __tablename__ = "publicacion_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    fecha: Mapped[date] = mapped_column(Date, unique=True)
    nombre: Mapped[str] = mapped_column(String(120))


class ComponentePublicado(BaseDeclarativa):
    __tablename__ = "componente_publicado"
    id: Mapped[int] = mapped_column(primary_key=True)
    publicacion_id: Mapped[int] = mapped_column(
        ForeignKey("publicacion_menu.id", ondelete="CASCADE")
    )
    nombre: Mapped[str] = mapped_column(String(180))
    orden: Mapped[int] = mapped_column(Integer)
    __table_args__ = (UniqueConstraint("publicacion_id", "orden"),)
