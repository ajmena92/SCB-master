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
    Index,
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
        CheckConstraint("tipo_persona IN ('estudiante','profesor')", name="tipo_tarifa"),
        Index("ix_tarifa_vigencia", "tipo_persona", "fecha_inicio", "fecha_fin"),
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
    __table_args__ = (
        CheckConstraint(
            "tipo IN ('venta','reserva','liberacion','consumo','ajuste')",
            name="tipo_movimiento_tiquete",
        ),
        CheckConstraint("saldo_resultante >= 0", name="saldo_movimiento_tiquete"),
        Index("ix_movimiento_tiquete_creado_en", "creado_en"),
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
    __table_args__ = (
        CheckConstraint("cantidad > 0", name="cantidad_venta_tiquete"),
        CheckConstraint("tarifa_aplicada >= 0 AND total >= 0", name="montos_venta_tiquete"),
        Index("ix_venta_tiquete_creado_en", "creado_en"),
    )


class ReservaComedor(BaseDeclarativa):
    __tablename__ = "reserva_comedor"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"))
    fecha: Mapped[date] = mapped_column(Date)
    estado: Mapped[str] = mapped_column(String(16), default="reservada")
    tiquete_inmovilizado: Mapped[bool] = mapped_column(Boolean, default=False)
    sin_tiquete: Mapped[bool] = mapped_column(Boolean, default=False)
    __table_args__ = (
        UniqueConstraint("persona_id", "fecha"),
        CheckConstraint(
            "estado IN ('reservada','cancelada','consumida')", name="estado_reserva_comedor"
        ),
        Index("ix_reserva_comedor_fecha", "fecha"),
    )


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
    __table_args__ = (
        UniqueConstraint("persona_id", "fecha"),
        CheckConstraint("decision IN ('aprobada','rechazada')", name="decision_autorizacion"),
        Index("ix_autorizacion_comedor_fecha", "fecha"),
    )


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
    marca_transporte_existente: Mapped[bool] = mapped_column(Boolean, default=False)
    advertencia: Mapped[str | None] = mapped_column(String(120), nullable=True)
    operador_id: Mapped[int] = mapped_column(ForeignKey("cuenta_administrativa.id"))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (
        UniqueConstraint("persona_id", "fecha"),
        CheckConstraint(
            "modalidad IN ('reserva','autorizacion','directo_profesor')",
            name="modalidad_ingreso_comedor",
        ),
        Index("ix_ingreso_comedor_fecha", "fecha"),
    )


class EventoOperacionComedor(BaseDeclarativa):
    __tablename__ = "evento_operacion_comedor"
    id: Mapped[int] = mapped_column(primary_key=True)
    fecha_evento: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    fecha_operativa: Mapped[date] = mapped_column(Date, index=True)
    persona_id: Mapped[int | None] = mapped_column(
        ForeignKey("persona.id", ondelete="SET NULL"), nullable=True
    )
    codigo_capturado: Mapped[str] = mapped_column(String(40))
    resultado: Mapped[str] = mapped_column(String(24))
    motivo: Mapped[str | None] = mapped_column(String(240), nullable=True)
    operador_id: Mapped[int] = mapped_column(ForeignKey("cuenta_administrativa.id"))
    advertencia: Mapped[bool] = mapped_column(Boolean, default=False)
    duracion_ms: Mapped[int | None] = mapped_column(Integer, nullable=True)
    __table_args__ = (
        CheckConstraint(
            "resultado IN ('aceptado','duplicado','no_encontrado','sin_reserva',"
            "'sin_tiquete','rechazado','error')",
            name="resultado_evento_operacion_comedor",
        ),
        CheckConstraint("duracion_ms IS NULL OR duracion_ms >= 0", name="duracion_evento_comedor"),
        Index("ix_evento_operacion_comedor_fecha_evento", "fecha_evento"),
    )


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
    __table_args__ = (
        UniqueConstraint("matricula_id", "fecha"),
        Index("ix_marca_transporte_fecha", "fecha"),
    )


class LoteImportacion(BaseDeclarativa):
    __tablename__ = "lote_importacion"
    id: Mapped[int] = mapped_column(primary_key=True)
    huella: Mapped[str] = mapped_column(String(64), unique=True)
    estado: Mapped[str] = mapped_column(String(16))
    resumen: Mapped[str] = mapped_column(String(1000))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (
        CheckConstraint(
            "estado IN ('pendiente','validado','confirmado','fallido')",
            name="estado_lote_importacion",
        ),
        Index("ix_lote_importacion_creado_en", "creado_en"),
    )


class IndicadorAnaliticoComedor(BaseDeclarativa):
    """Resultado diario de pandas; es una señal para revisión, no una decisión."""

    __tablename__ = "indicador_analitico_comedor"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"), index=True)
    fecha_corte: Mapped[date] = mapped_column(Date, index=True)
    dias_observados: Mapped[int] = mapped_column(Integer)
    dias_presentes: Mapped[int] = mapped_column(Integer)
    porcentaje_asistencia: Mapped[float] = mapped_column(Numeric(5, 2))
    consumos_comedor: Mapped[int] = mapped_column(Integer, default=0)
    consumos_tiquete: Mapped[int] = mapped_column(Integer, default=0)
    senal: Mapped[str] = mapped_column(String(64))
    generado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (
        UniqueConstraint("persona_id", "fecha_corte"),
        CheckConstraint("dias_observados >= 0 AND dias_presentes >= 0", name="conteos_indicador_no_negativos"),
        CheckConstraint("porcentaje_asistencia >= 0 AND porcentaje_asistencia <= 100", name="porcentaje_indicador_valido"),
        Index("ix_indicador_analitico_corte_senal", "fecha_corte", "senal"),
    )
