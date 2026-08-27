"""Modelos ORM del dominio de identidad."""

from __future__ import annotations

from datetime import datetime

from sqlalchemy import Boolean, DateTime, ForeignKey, Integer, String
from sqlalchemy.orm import Mapped, mapped_column

from aplicacion.nucleo.modelos_base import BaseDeclarativa


class Usuario(BaseDeclarativa):
    __tablename__ = "usuario"
    __table_args__ = {"schema": "identidad"}
    id_usuario: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre_usuario: Mapped[str] = mapped_column(String(100), unique=True, nullable=False)
    hash_contrasena: Mapped[str] = mapped_column(String(255), nullable=False)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    fecha_creacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    fecha_actualizacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class Permiso(BaseDeclarativa):
    __tablename__ = "permiso"
    __table_args__ = {"schema": "identidad"}
    id_permiso: Mapped[int] = mapped_column(Integer, primary_key=True)
    clave: Mapped[str] = mapped_column(String(150), unique=True, nullable=False)
    descripcion: Mapped[str | None] = mapped_column(String(300), nullable=True)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class Rol(BaseDeclarativa):
    __tablename__ = "rol"
    __table_args__ = {"schema": "identidad"}
    id_rol: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120), unique=True, nullable=False)
    descripcion: Mapped[str | None] = mapped_column(String(300), nullable=True)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class UsuarioPermiso(BaseDeclarativa):
    __tablename__ = "usuario_permiso"
    __table_args__ = {"schema": "identidad"}
    id_usuario: Mapped[int] = mapped_column(
        ForeignKey("identidad.usuario.id_usuario"), primary_key=True
    )
    id_permiso: Mapped[int] = mapped_column(
        ForeignKey("identidad.permiso.id_permiso"), primary_key=True
    )


class RolPermiso(BaseDeclarativa):
    __tablename__ = "rol_permiso"
    __table_args__ = {"schema": "identidad"}
    id_rol: Mapped[int] = mapped_column(ForeignKey("identidad.rol.id_rol"), primary_key=True)
    id_permiso: Mapped[int] = mapped_column(
        ForeignKey("identidad.permiso.id_permiso"), primary_key=True
    )


class UsuarioRol(BaseDeclarativa):
    __tablename__ = "usuario_rol"
    __table_args__ = {"schema": "identidad"}
    id_usuario: Mapped[int] = mapped_column(
        ForeignKey("identidad.usuario.id_usuario"), primary_key=True
    )
    id_rol: Mapped[int] = mapped_column(ForeignKey("identidad.rol.id_rol"), primary_key=True)


class SesionEstudiante(BaseDeclarativa):
    __tablename__ = "sesion_estudiante"
    __table_args__ = {"schema": "identidad"}
    id_sesion: Mapped[str] = mapped_column(String(100), primary_key=True)
    id_usuario: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante"), nullable=False
    )
    secreto_hash: Mapped[str] = mapped_column(String(64), nullable=False)
    expira_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    csrf_hash: Mapped[str | None] = mapped_column(String(64), nullable=True)
    revocada: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    fecha_revocacion: Mapped[datetime | None] = mapped_column(DateTime, nullable=True)


class Sesion(BaseDeclarativa):
    __tablename__ = "sesion"
    __table_args__ = {"schema": "identidad"}
    id_sesion: Mapped[str] = mapped_column(String(64), primary_key=True)
    id_usuario: Mapped[int] = mapped_column(
        ForeignKey("identidad.usuario.id_usuario"), nullable=False
    )
    secreto_hash: Mapped[str] = mapped_column(String(64), nullable=False)
    expira_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    csrf_hash: Mapped[str | None] = mapped_column(String(64), nullable=True)
    revocada: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)


class IntentoAutenticacion(BaseDeclarativa):
    """Estado compartido de bloqueo, sin conservar el identificador en claro."""

    __tablename__ = "intento_autenticacion"
    __table_args__ = {"schema": "identidad"}
    identificador_hash: Mapped[str] = mapped_column(String(64), primary_key=True)
    intentos_fallidos: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    bloqueado_hasta: Mapped[datetime | None] = mapped_column(DateTime, nullable=True)
    fecha_actualizacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)
