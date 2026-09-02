"""Personas, matriculas, identidad y transporte."""

from __future__ import annotations

from datetime import date, datetime, timezone

from sqlalchemy import (
    Boolean,
    CheckConstraint,
    Date,
    DateTime,
    ForeignKey,
    Index,
    Integer,
    LargeBinary,
    String,
    UniqueConstraint,
    text,
)
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class Persona(BaseDeclarativa):
    __tablename__ = "persona"
    id: Mapped[int] = mapped_column(primary_key=True)
    codigo: Mapped[str] = mapped_column(String(32), unique=True, index=True)
    cedula: Mapped[str | None] = mapped_column(String(32), unique=True, nullable=True)
    nombres: Mapped[str] = mapped_column(String(180))
    tipo: Mapped[str] = mapped_column(String(12))
    activo: Mapped[bool] = mapped_column(Boolean, default=True)
    __table_args__ = (CheckConstraint("tipo IN ('estudiante','profesor')", name="tipo_persona"),)


class FotografiaPersona(BaseDeclarativa):
    """Fotografía privada asociada a una persona del padrón."""

    __tablename__ = "fotografia_persona"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(
        ForeignKey("persona.id", ondelete="CASCADE"), unique=True
    )
    contenido: Mapped[bytes] = mapped_column(LargeBinary)
    tipo_contenido: Mapped[str] = mapped_column(String(80))


class CredencialPortal(BaseDeclarativa):
    __tablename__ = "credencial_portal"
    persona_id: Mapped[int] = mapped_column(
        ForeignKey("persona.id", ondelete="CASCADE"), primary_key=True
    )
    pin_hash: Mapped[str] = mapped_column(String(255))
    cambio_obligatorio: Mapped[bool] = mapped_column(Boolean, default=True)


class EventoCredencialPortal(BaseDeclarativa):
    """Trazabilidad de entregas y reinicios, sin persistir nunca el PIN."""

    __tablename__ = "evento_credencial_portal"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"), index=True)
    cuenta_administrativa_id: Mapped[int | None] = mapped_column(
        ForeignKey("cuenta_administrativa.id"), nullable=True
    )
    tipo: Mapped[str] = mapped_column(String(24))
    creado_en: Mapped[datetime] = mapped_column(
        DateTime(timezone=True), default=lambda: datetime.now(timezone.utc)
    )
    __table_args__ = (
        CheckConstraint(
            "tipo IN ('creacion','reinicio_individual','reinicio_masivo')",
            name="tipo_evento_credencial_portal",
        ),
        Index("ix_evento_credencial_portal_persona_fecha", "persona_id", "creado_en"),
    )


class CuentaAdministrativa(BaseDeclarativa):
    __tablename__ = "cuenta_administrativa"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int | None] = mapped_column(
        ForeignKey("persona.id", ondelete="RESTRICT"), nullable=True
    )
    usuario: Mapped[str] = mapped_column(String(80), unique=True)
    contrasena_hash: Mapped[str] = mapped_column(String(255))
    rol: Mapped[str] = mapped_column(String(16))
    activo: Mapped[bool] = mapped_column(Boolean, default=True)
    cambio_contrasena_obligatorio: Mapped[bool] = mapped_column(Boolean, default=False)
    vinculacion_pendiente: Mapped[bool] = mapped_column(Boolean, default=True)
    __table_args__ = (
        CheckConstraint("rol IN ('administrador','operador')", name="rol_administrativo"),
        CheckConstraint(
            "(persona_id IS NULL AND vinculacion_pendiente) OR "
            "(persona_id IS NOT NULL AND NOT vinculacion_pendiente)",
            name="estado_vinculacion_cuenta",
        ),
        UniqueConstraint("persona_id", name="uq_cuenta_administrativa_persona_id"),
        Index("ix_cuenta_administrativa_persona_id", "persona_id"),
        Index("uq_cuenta_administrativa_usuario_lower", text("lower(usuario)"), unique=True),
    )


class PermisoAdministrativo(BaseDeclarativa):
    __tablename__ = "permiso_administrativo"
    clave: Mapped[str] = mapped_column(String(80), primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120))
    descripcion: Mapped[str] = mapped_column(String(300))
    modulo: Mapped[str] = mapped_column(String(80), index=True)


class CuentaPermiso(BaseDeclarativa):
    __tablename__ = "cuenta_permiso"
    cuenta_id: Mapped[int] = mapped_column(
        ForeignKey("cuenta_administrativa.id", ondelete="CASCADE"), primary_key=True
    )
    permiso_clave: Mapped[str] = mapped_column(
        ForeignKey("permiso_administrativo.clave", ondelete="CASCADE"), primary_key=True
    )


class SesionAcceso(BaseDeclarativa):
    __tablename__ = "sesion_acceso"
    token_hash: Mapped[str] = mapped_column(String(64), primary_key=True)
    tipo: Mapped[str] = mapped_column(String(16))
    persona_id: Mapped[int | None] = mapped_column(
        ForeignKey("persona.id", ondelete="CASCADE"), nullable=True, index=True
    )
    cuenta_id: Mapped[int | None] = mapped_column(
        ForeignKey("cuenta_administrativa.id", ondelete="CASCADE"), nullable=True, index=True
    )
    cambio_obligatorio: Mapped[bool] = mapped_column(Boolean, default=False)
    expira_en: Mapped[datetime] = mapped_column(DateTime(timezone=True))
    __table_args__ = (
        CheckConstraint(
            "(tipo = 'portal' AND persona_id IS NOT NULL AND cuenta_id IS NULL) OR "
            "(tipo = 'administracion' AND cuenta_id IS NOT NULL AND persona_id IS NULL)",
            name="propietario_sesion",
        ),
        Index("ix_sesion_acceso_expira_en", "expira_en"),
    )


class AnioLectivo(BaseDeclarativa):
    __tablename__ = "anio_lectivo"
    id: Mapped[int] = mapped_column(primary_key=True)
    anio: Mapped[int] = mapped_column(Integer, unique=True)
    vigente: Mapped[bool] = mapped_column(Boolean, default=False)
    __table_args__ = (
        CheckConstraint("anio >= 2000 AND anio <= 2200", name="rango_anio"),
        Index(
            "uq_anio_lectivo_vigente",
            "vigente",
            unique=True,
            postgresql_where=text("vigente"),
            sqlite_where=text("vigente = 1"),
        ),
    )


class Matricula(BaseDeclarativa):
    __tablename__ = "matricula"
    id: Mapped[int] = mapped_column(primary_key=True)
    persona_id: Mapped[int] = mapped_column(ForeignKey("persona.id"), index=True)
    anio_lectivo_id: Mapped[int] = mapped_column(ForeignKey("anio_lectivo.id"), index=True)
    seccion: Mapped[str] = mapped_column(String(40))
    turno: Mapped[str] = mapped_column(String(24))
    becado: Mapped[bool] = mapped_column(Boolean, default=False)
    estado: Mapped[str] = mapped_column(String(16), default="activo")
    __table_args__ = (
        UniqueConstraint("persona_id", "anio_lectivo_id"),
        CheckConstraint("turno = 'diurno'", name="turno_matricula_diurno"),
        CheckConstraint(
            "estado IN ('activo','retirado','graduado','trasladado')",
            name="estado_matricula",
        ),
    )


class Ruta(BaseDeclarativa):
    __tablename__ = "ruta"
    id: Mapped[int] = mapped_column(primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120), unique=True)
    codigo: Mapped[str] = mapped_column(String(50), unique=True)
    descripcion: Mapped[str] = mapped_column(String(500))
    color_hex: Mapped[str] = mapped_column(String(7), default="#CBD5E1")
    activo: Mapped[bool] = mapped_column(Boolean, default=True)


class AsignacionRuta(BaseDeclarativa):
    __tablename__ = "asignacion_ruta"
    id: Mapped[int] = mapped_column(primary_key=True)
    matricula_id: Mapped[int] = mapped_column(ForeignKey("matricula.id"), index=True)
    ruta_id: Mapped[int] = mapped_column(ForeignKey("ruta.id"), index=True)
    fecha_inicio: Mapped[date] = mapped_column(Date)
    fecha_fin: Mapped[date | None] = mapped_column(Date, nullable=True)
    __table_args__ = (
        CheckConstraint("fecha_fin IS NULL OR fecha_fin >= fecha_inicio", name="vigencia_ruta"),
        Index(
            "uq_asignacion_ruta_matricula_activa",
            "matricula_id",
            unique=True,
            postgresql_where=text("fecha_fin IS NULL"),
            sqlite_where=text("fecha_fin IS NULL"),
        ),
    )


class HorarioReserva(BaseDeclarativa):
    __tablename__ = "horario_reserva"
    turno: Mapped[str] = mapped_column(String(24), primary_key=True)
    hora_limite: Mapped[str] = mapped_column(String(5))
