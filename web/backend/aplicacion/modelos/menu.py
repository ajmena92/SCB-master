"""Plantillas y publicaciones inmutables del menu."""

from datetime import date

from sqlalchemy import Boolean, CheckConstraint, Date, ForeignKey, Integer, String, Text, UniqueConstraint
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class PlantillaMenu(BaseDeclarativa):
    __tablename__ = "plantilla_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    semana: Mapped[int] = mapped_column(Integer)
    dia: Mapped[int] = mapped_column(Integer)
    titulo: Mapped[str] = mapped_column(String(180))
    observaciones: Mapped[str | None] = mapped_column(Text)
    activo: Mapped[bool] = mapped_column(Boolean, default=True)
    __table_args__ = (
        UniqueConstraint("semana", "dia", name="uq_plantilla_menu_semana_dia"),
        CheckConstraint("semana BETWEEN 1 AND 5", name="semana_plantilla_menu"),
        CheckConstraint("dia BETWEEN 1 AND 5", name="dia_plantilla_menu"),
    )


class ComponenteMenu(BaseDeclarativa):
    __tablename__ = "componente_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    plantilla_id: Mapped[int] = mapped_column(ForeignKey("plantilla_menu.id", ondelete="CASCADE"))
    nombre: Mapped[str] = mapped_column(String(180))
    tipo: Mapped[str] = mapped_column(String(40), default="Principal")
    orden: Mapped[int] = mapped_column(Integer)
    __table_args__ = (
        UniqueConstraint("plantilla_id", "orden"),
        CheckConstraint("orden > 0", name="orden_componente_menu"),
    )


class PublicacionMenu(BaseDeclarativa):
    __tablename__ = "publicacion_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    fecha: Mapped[date] = mapped_column(Date, unique=True)
    titulo: Mapped[str] = mapped_column(String(180))
    observaciones: Mapped[str | None] = mapped_column(Text)
    origen: Mapped[str] = mapped_column(String(20))


class ComponentePublicado(BaseDeclarativa):
    __tablename__ = "componente_publicado"
    id: Mapped[int] = mapped_column(primary_key=True)
    publicacion_id: Mapped[int] = mapped_column(
        ForeignKey("publicacion_menu.id", ondelete="CASCADE")
    )
    nombre: Mapped[str] = mapped_column(String(180))
    tipo: Mapped[str] = mapped_column(String(40), default="Principal")
    orden: Mapped[int] = mapped_column(Integer)
    __table_args__ = (
        UniqueConstraint("publicacion_id", "orden"),
        CheckConstraint("orden > 0", name="orden_componente_publicado"),
    )


class CalendarioMenu(BaseDeclarativa):
    """Excepciones institucionales; los días no registrados siguen la regla ordinaria."""

    __tablename__ = "calendario_menu"
    fecha: Mapped[date] = mapped_column(Date, primary_key=True)
    habilitado: Mapped[bool] = mapped_column(Boolean, nullable=False)
    motivo: Mapped[str | None] = mapped_column(String(300))


class ConfiguracionCicloMenu(BaseDeclarativa):
    """Configuración institucional única del ciclo PANEA, independiente del año."""

    __tablename__ = "configuracion_ciclo_menu"
    id: Mapped[int] = mapped_column(Integer, primary_key=True)
    inicio_ciclo_menu: Mapped[date] = mapped_column(Date, nullable=False)
    __table_args__ = (CheckConstraint("id = 1", name="configuracion_ciclo_menu_unica"),)


class SustitucionMenu(BaseDeclarativa):
    __tablename__ = "sustitucion_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    fecha: Mapped[date] = mapped_column(Date, unique=True)
    titulo: Mapped[str] = mapped_column(String(180))
    observaciones: Mapped[str | None] = mapped_column(Text)


class ComponenteSustitucionMenu(BaseDeclarativa):
    __tablename__ = "componente_sustitucion_menu"
    id: Mapped[int] = mapped_column(primary_key=True)
    sustitucion_id: Mapped[int] = mapped_column(
        ForeignKey("sustitucion_menu.id", ondelete="CASCADE")
    )
    nombre: Mapped[str] = mapped_column(String(180))
    tipo: Mapped[str] = mapped_column(String(40), default="Principal")
    orden: Mapped[int] = mapped_column(Integer)
    __table_args__ = (
        UniqueConstraint("sustitucion_id", "orden"),
        CheckConstraint("orden > 0", name="orden_componente_sustitucion_menu"),
    )
