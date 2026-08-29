"""Modelos ORM del catálogo y operación canónica del comedor."""

from __future__ import annotations

from datetime import date, datetime

from sqlalchemy import BigInteger, Boolean, Date, DateTime, ForeignKey, Integer, String, Time
from sqlalchemy.orm import Mapped, mapped_column, relationship

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class EstadoComedor(BaseDeclarativa):
    __tablename__ = "estado_comedor"
    __table_args__ = {"schema": "comedor"}
    id_estado_comedor: Mapped[int] = mapped_column(Integer, primary_key=True)
    codigo: Mapped[str] = mapped_column(String(30), unique=True, nullable=False)
    descripcion: Mapped[str] = mapped_column(String(80), nullable=False)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class PersonaComedor(BaseDeclarativa):
    __tablename__ = "persona"
    __table_args__ = {"schema": "comedor"}
    id_persona: Mapped[int] = mapped_column(Integer, primary_key=True)
    tipo_persona: Mapped[str] = mapped_column(String(20), nullable=False)
    id_estudiante: Mapped[int | None] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante"), nullable=True
    )
    id_usuario: Mapped[int | None] = mapped_column(
        ForeignKey("identidad.usuario.id_usuario"), nullable=True
    )
    codigo_barras: Mapped[str] = mapped_column(String(80), nullable=False, unique=True)
    nombre_completo: Mapped[str] = mapped_column(String(220), nullable=False)
    colegio: Mapped[str | None] = mapped_column(String(200), nullable=True)
    id_estado_comedor: Mapped[int] = mapped_column(
        ForeignKey("comedor.estado_comedor.id_estado_comedor"), nullable=False
    )
    estado = relationship("EstadoComedor")
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    creado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    actualizado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class CuentaTiquetes(BaseDeclarativa):
    __tablename__ = "cuenta_tiquetes"
    __table_args__ = {"schema": "comedor"}
    id_cuenta: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_persona: Mapped[int] = mapped_column(
        ForeignKey("comedor.persona.id_persona"), nullable=False, unique=True
    )
    saldo: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    reservados: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    actualizado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    persona = relationship("PersonaComedor")


class MovimientoTiquetes(BaseDeclarativa):
    __tablename__ = "movimiento_tiquetes"
    __table_args__ = {"schema": "comedor"}
    id_movimiento: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    id_cuenta: Mapped[int] = mapped_column(ForeignKey("comedor.cuenta_tiquetes.id_cuenta"))
    tipo: Mapped[str] = mapped_column(String(12), nullable=False)
    cantidad: Mapped[int] = mapped_column(Integer, nullable=False)
    saldo_anterior: Mapped[int] = mapped_column(Integer, nullable=False)
    saldo_nuevo: Mapped[int] = mapped_column(Integer, nullable=False)
    reservados_anterior: Mapped[int] = mapped_column(Integer, nullable=False)
    reservados_nuevo: Mapped[int] = mapped_column(Integer, nullable=False)
    clave_idempotencia: Mapped[str] = mapped_column(String(100), nullable=False, unique=True)
    concepto: Mapped[str | None] = mapped_column(String(250), nullable=True)
    creado_por: Mapped[int | None] = mapped_column(Integer, nullable=True)
    creado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)

class ReservaComedor(BaseDeclarativa):
    __tablename__ = "reserva"
    __table_args__ = {"schema": "comedor"}
    id_reserva: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    id_persona: Mapped[int] = mapped_column(ForeignKey("comedor.persona.id_persona"))
    fecha: Mapped[date] = mapped_column(Date, nullable=False)
    estado: Mapped[str] = mapped_column(String(12), nullable=False)
    requiere_tiquete: Mapped[bool] = mapped_column(Boolean, nullable=False)
    modalidad: Mapped[str] = mapped_column(String(12), nullable=False)
    registrada_por: Mapped[int | None] = mapped_column(Integer, nullable=True)
    creado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    actualizada_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class IngresoComedor(BaseDeclarativa):
    __tablename__ = "ingreso"
    __table_args__ = {"schema": "comedor"}
    id_ingreso: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    id_persona: Mapped[int] = mapped_column(ForeignKey("comedor.persona.id_persona"))
    fecha: Mapped[date] = mapped_column(Date, nullable=False)
    modalidad: Mapped[str] = mapped_column(String(12), nullable=False)
    codigo_horario: Mapped[str | None] = mapped_column(String(20), nullable=True)
    hora_marca: Mapped[datetime | None] = mapped_column(DateTime, nullable=True)
    marca_transporte_existente: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    registrado_por: Mapped[int | None] = mapped_column(Integer, nullable=True)
    creado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    hora_limite_aplicada: Mapped[object | None] = mapped_column(Time, nullable=True)
    resultado: Mapped[str] = mapped_column(String(20), nullable=False, default="registrado")
    advertencias: Mapped[str | None] = mapped_column(String(500), nullable=True)
    permitir_marca_tardia: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    permitir_sin_marca_transporte: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class AuditoriaIngreso(BaseDeclarativa):
    __tablename__ = "auditoria_ingreso"
    __table_args__ = {"schema": "comedor"}
    id_auditoria: Mapped[int] = mapped_column(BigInteger, primary_key=True)
    id_ingreso: Mapped[int | None] = mapped_column(
        ForeignKey("comedor.ingreso.id_ingreso"), nullable=True
    )
    id_persona: Mapped[int | None] = mapped_column(Integer, nullable=True)
    fecha: Mapped[date] = mapped_column(Date, nullable=False)
    codigo_resultado: Mapped[str] = mapped_column(String(40), nullable=False)
    detalle: Mapped[str | None] = mapped_column(String(500), nullable=True)
    advertencias: Mapped[str | None] = mapped_column(String(500), nullable=True)
    hora_servidor: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    registrado_por: Mapped[int | None] = mapped_column(Integer, nullable=True)
    terminal_id: Mapped[str | None] = mapped_column(String(100), nullable=True)
