"""Tiquetes, comedor, transporte e importaciones."""

from __future__ import annotations

from datetime import date, datetime, timezone
from decimal import Decimal

from sqlalchemy import (
    Boolean,
    CheckConstraint,
    Date,
    DateTime,
    ForeignKey,
    Integer,
    Numeric,
    String,
    UniqueConstraint,
)
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class Tarifa(BaseDeclarativa):
    __tablename__ = "tarifa"
    id: Mapped[int] = mapped_column(primary_key=True)
    tipo_persona: Mapped[str] = mapped_column(String(12))
    monto: Mapped[Decimal] = mapped_column(Numeric(10, 2))
    fecha_inicio: Mapped[date] = mapped_column(Date)
    fecha_fin: Mapped[date | None] = mapped_column(Date, nullable=True)
    __table_args__ = (
        CheckConstraint("monto >= 0", name="monto_tarifa"),
        CheckConstraint("fecha_fin IS NULL OR fecha_fin >= fecha_inicio", name="vigencia_tarifa"),
    )


class CuentaTiquete(BaseDeclarativa):
    __tablename__ = "cuenta_tiquete"
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"), primary_key=True)
    saldo: Mapped[int] = mapped_column(Integer, default=0)
    reservados: Mapped[int] = mapped_column(Integer, default=0)
    __table_args__ = (
        CheckConstraint("saldo >= 0", name="saldo_no_negativo"),
        CheckConstraint("reservados >= 0", name="reservados_no_negativo"),
    )


class MovimientoTiquete(BaseDeclarativa):
    __tablename__ = "movimiento_tiquete"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"), index=True)
    tipo: Mapped[str] = mapped_column(String(20))
    cantidad: Mapped[int] = mapped_column(Integer)
    saldo_resultante: Mapped[int] = mapped_column(Integer)
    referencia: Mapped[str | None] = mapped_column(String(80), nullable=True)
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )


class VentaTiquete(BaseDeclarativa):
    __tablename__ = "venta_tiquete"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"))
    tarifa_id: Mapped[int] = mapped_column(ForeignKey("tarifa.id"))
    cantidad: Mapped[int] = mapped_column(Integer)
    tarifa_aplicada: Mapped[Decimal] = mapped_column(Numeric(10, 2))
    total: Mapped[Decimal] = mapped_column(Numeric(12, 2))
    medio_pago: Mapped[str] = mapped_column(String(30))
    operador_id: Mapped[int] = mapped_column(ForeignKey("cuenta_administrativa.id"))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )


class ReservaComedor(BaseDeclarativa):
    __tablename__ = "reserva_comedor"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"))
    fecha: Mapped[date] = mapped_column(Date)
    estado: Mapped[str] = mapped_column(String(16), default="reservada")
    tiquete_inmovilizado: Mapped[bool] = mapped_column(Boolean, default=False)
    __table_args__ = (UniqueConstraint("persona_id", "fecha"),)


class AutorizacionComedor(BaseDeclarativa):
    __tablename__ = "autorizacion_comedor"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"))
    fecha: Mapped[date] = mapped_column(Date)
    decision: Mapped[str] = mapped_column(String(12))
    motivo: Mapped[str | None] = mapped_column(String(240), nullable=True)
    operador_id: Mapped[int] = mapped_column(ForeignKey("cuenta_administrativa.id"))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (UniqueConstraint("persona_id", "fecha"),)


class IngresoComedor(BaseDeclarativa):
    __tablename__ = "ingreso_comedor"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"))
    fecha: Mapped[date] = mapped_column(Date)
    reserva_id: Mapped[int | None] = mapped_column(ForeignKey("reserva_comedor.id"), nullable=True)
    autorizacion_id: Mapped[int | None] = mapped_column(
        ForeignKey("autorizacion_comedor.id"), nullable=True
    )
    modalidad: Mapped[str] = mapped_column(String(24))
    consumio_tiquete: Mapped[bool] = mapped_column(Boolean)
    operador_id: Mapped[int] = mapped_column(ForeignKey("cuenta_administrativa.id"))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (UniqueConstraint("persona_id", "fecha"),)


class MarcaTransporte(BaseDeclarativa):
    __tablename__ = "marca_transporte"
    id: Mapped[int] = mapped_column(primary_key=True)
    matricula_id: Mapped[int] = mapped_column(ForeignKey("matricula.id"))
    ruta_id: Mapped[int] = mapped_column(ForeignKey("ruta.id"))
    fecha: Mapped[date] = mapped_column(Date)
    operador_id: Mapped[int] = mapped_column(ForeignKey("cuenta_administrativa.id"))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (UniqueConstraint("matricula_id", "fecha"),)


class LoteImportacion(BaseDeclarativa):
    __tablename__ = "lote_importacion"
    id: Mapped[int] = mapped_column(primary_key=True)
    huella: Mapped[str] = mapped_column(String(64), unique=True)
    estado: Mapped[str] = mapped_column(String(16))
    resumen: Mapped[str] = mapped_column(String(1000))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
