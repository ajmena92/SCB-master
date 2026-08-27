"""Modelo declarativo canónico, independiente de WinForms y del esquema dbo."""

from datetime import datetime

from sqlalchemy import Boolean, DateTime, ForeignKey, Integer, MetaData, String
from sqlalchemy.orm import DeclarativeBase, Mapped, mapped_column, relationship

CONVENCIONES = {
    "ix": "ix_%(column_0_label)s",
    "uq": "uq_%(table_name)s_%(column_0_name)s",
    "ck": "ck_%(table_name)s_%(constraint_name)s",
    "fk": "fk_%(table_name)s_%(column_0_name)s_%(referred_table_name)s",
    "pk": "pk_%(table_name)s",
}


class BaseDeclarativa(DeclarativeBase):
    metadata = MetaData(naming_convention=CONVENCIONES)


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
    id_usuario: Mapped[int] = mapped_column(ForeignKey("identidad.usuario.id_usuario"), primary_key=True)
    id_permiso: Mapped[int] = mapped_column(ForeignKey("identidad.permiso.id_permiso"), primary_key=True)


class RolPermiso(BaseDeclarativa):
    __tablename__ = "rol_permiso"
    __table_args__ = {"schema": "identidad"}
    id_rol: Mapped[int] = mapped_column(ForeignKey("identidad.rol.id_rol"), primary_key=True)
    id_permiso: Mapped[int] = mapped_column(ForeignKey("identidad.permiso.id_permiso"), primary_key=True)


class UsuarioRol(BaseDeclarativa):
    __tablename__ = "usuario_rol"
    __table_args__ = {"schema": "identidad"}
    id_usuario: Mapped[int] = mapped_column(ForeignKey("identidad.usuario.id_usuario"), primary_key=True)
    id_rol: Mapped[int] = mapped_column(ForeignKey("identidad.rol.id_rol"), primary_key=True)


class SesionEstudiante(BaseDeclarativa):
    __tablename__ = "sesion_estudiante"
    __table_args__ = {"schema": "identidad"}
    id_sesion: Mapped[str] = mapped_column(String(100), primary_key=True)
    id_usuario: Mapped[int] = mapped_column(ForeignKey("estudiantes.estudiante.id_estudiante"), nullable=False)
    secreto_hash: Mapped[str] = mapped_column(String(64), nullable=False)
    expira_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    csrf_hash: Mapped[str | None] = mapped_column(String(64), nullable=True)
    revocada: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    fecha_revocacion: Mapped[datetime | None] = mapped_column(DateTime, nullable=True)


class Sesion(BaseDeclarativa):
    __tablename__ = "sesion"
    __table_args__ = {"schema": "identidad"}
    id_sesion: Mapped[str] = mapped_column(String(64), primary_key=True)
    id_usuario: Mapped[int] = mapped_column(ForeignKey("identidad.usuario.id_usuario"), nullable=False)
    secreto_hash: Mapped[str] = mapped_column(String(64), nullable=False)
    expira_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    csrf_hash: Mapped[str | None] = mapped_column(String(64), nullable=True)
    revocada: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)


class Ruta(BaseDeclarativa):
    __tablename__ = "ruta"
    __table_args__ = {"schema": "transporte"}
    id_ruta: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(120), nullable=False)
    activa: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)
    estudiantes: Mapped[list["Estudiante"]] = relationship(back_populates="ruta")


class Estudiante(BaseDeclarativa):
    __tablename__ = "estudiante"
    __table_args__ = {"schema": "estudiantes"}
    id_estudiante: Mapped[int] = mapped_column(Integer, primary_key=True)
    carne: Mapped[str] = mapped_column(String(30), unique=True, nullable=False)
    nombre: Mapped[str] = mapped_column(String(100), nullable=False)
    primer_apellido: Mapped[str] = mapped_column(String(100), nullable=False)
    segundo_apellido: Mapped[str | None] = mapped_column(String(100), nullable=True)
    cedula: Mapped[str | None] = mapped_column(String(30), nullable=True)
    seccion: Mapped[str | None] = mapped_column(String(30), nullable=True)
    turno: Mapped[str | None] = mapped_column(String(30), nullable=True)
    hash_contrasena: Mapped[str | None] = mapped_column(String(255), nullable=True)
    debe_cambiar_pin: Mapped[bool] = mapped_column(Boolean, nullable=False, default=False)
    fecha_expiracion_pin: Mapped[datetime | None] = mapped_column(DateTime, nullable=True)
    id_ruta: Mapped[int | None] = mapped_column(
        ForeignKey("transporte.ruta.id_ruta", name="fk_estudiante_ruta"), nullable=True
    )
    id_beneficio: Mapped[int | None] = mapped_column(Integer, nullable=True)
    ruta: Mapped[Ruta | None] = relationship(back_populates="estudiantes")


class MarcaAsistencia(BaseDeclarativa):
    __tablename__ = "marca"
    __table_args__ = {"schema": "asistencia"}
    id_marca: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_marca_estudiante"), nullable=False
    )
    fecha_hora: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)
    estudiante: Mapped[Estudiante] = relationship()


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
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_beneficio_estudiante_estudiante"),
        primary_key=True,
    )
    beneficio: Mapped[Beneficio] = relationship()


class CuentaEstudiante(BaseDeclarativa):
    __tablename__ = "cuenta_estudiante"
    __table_args__ = {"schema": "cuentas"}
    id_cuenta: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_cuenta_estudiante"), nullable=False,
        unique=True,
    )
    saldo: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    estudiante: Mapped[Estudiante] = relationship()


class Reporte(BaseDeclarativa):
    __tablename__ = "reporte"
    __table_args__ = {"schema": "reportes"}
    id_reporte: Mapped[int] = mapped_column(Integer, primary_key=True)
    tipo: Mapped[str] = mapped_column(String(80), nullable=False)
    fecha_generacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class LoteImportacion(BaseDeclarativa):
    __tablename__ = "lote"
    __table_args__ = {"schema": "importaciones"}
    id_lote: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre_archivo: Mapped[str] = mapped_column(String(255), nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)
    fecha_creacion: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class EventoAuditoria(BaseDeclarativa):
    __tablename__ = "evento"
    __table_args__ = {"schema": "auditoria"}
    id_evento: Mapped[int] = mapped_column(Integer, primary_key=True)
    accion: Mapped[str] = mapped_column(String(100), nullable=False)
    id_usuario: Mapped[int | None] = mapped_column(
        ForeignKey("identidad.usuario.id_usuario", name="fk_evento_usuario"), nullable=True
    )
    fecha_hora: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    usuario: Mapped[Usuario | None] = relationship()


class Menu(BaseDeclarativa):
    __tablename__ = "menu"
    __table_args__ = {"schema": "menu"}
    id_menu: Mapped[int] = mapped_column(Integer, primary_key=True)
    fecha: Mapped[datetime] = mapped_column(DateTime, nullable=False)
    descripcion: Mapped[str] = mapped_column(String(500), nullable=False)


class ServicioComedor(BaseDeclarativa):
    __tablename__ = "servicio"
    __table_args__ = {"schema": "comedor"}
    id_servicio: Mapped[int] = mapped_column(Integer, primary_key=True)
    nombre: Mapped[str] = mapped_column(String(100), nullable=False)
    activo: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class ParametroOperativo(BaseDeclarativa):
    __tablename__ = "parametro"
    __table_args__ = {"schema": "comedor"}
    id_parametro: Mapped[int] = mapped_column(Integer, primary_key=True)
    minutos_aviso_previo: Mapped[int] = mapped_column(Integer, nullable=False)
    actualizado_en: Mapped[datetime] = mapped_column(DateTime, nullable=False)


class CalendarioMenu(BaseDeclarativa):
    __tablename__ = "calendario"
    __table_args__ = {"schema": "menu"}
    id_calendario: Mapped[int] = mapped_column(Integer, primary_key=True)
    fecha: Mapped[datetime] = mapped_column(DateTime, nullable=False, unique=True)
    habilitado: Mapped[bool] = mapped_column(Boolean, nullable=False, default=True)


class SolicitudSoporte(BaseDeclarativa):
    __tablename__ = "solicitud"
    __table_args__ = {"schema": "soporte"}
    id_solicitud: Mapped[int] = mapped_column(Integer, primary_key=True)
    asunto: Mapped[str] = mapped_column(String(200), nullable=False)
    estado: Mapped[str] = mapped_column(String(30), nullable=False)


class FotografiaEstudiante(BaseDeclarativa):
    __tablename__ = "fotografia"
    __table_args__ = {"schema": "estudiantes"}
    id_fotografia: Mapped[int] = mapped_column(Integer, primary_key=True)
    id_estudiante: Mapped[int] = mapped_column(
        ForeignKey("estudiantes.estudiante.id_estudiante", name="fk_fotografia_estudiante"), nullable=False,
        unique=True,
    )
    contenido: Mapped[bytes] = mapped_column(nullable=False)
    tipo_contenido: Mapped[str] = mapped_column(String(80), nullable=False)


class ResumenOperativo(BaseDeclarativa):
    __tablename__ = "resumen"
    __table_args__ = {"schema": "reportes"}
    id_resumen: Mapped[int] = mapped_column(Integer, primary_key=True)
    fecha: Mapped[datetime] = mapped_column(DateTime, nullable=False, unique=True)
    estudiantes: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    confirmaciones: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
    cancelaciones: Mapped[int] = mapped_column(Integer, nullable=False, default=0)
