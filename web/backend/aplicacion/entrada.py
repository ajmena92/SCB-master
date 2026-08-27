"""Punto de entrada de la plataforma web canónica.

Este módulo es la composición ejecutable de la aplicación nueva. No importa el
servidor histórico ni sus adaptadores. Las fábricas recibidas permiten pruebas
aisladas y mantienen el acceso SQL explícito en producción.
"""

from __future__ import annotations

import ipaddress
from collections.abc import Callable, Iterator
from dataclasses import dataclass
from datetime import timedelta

from fastapi import Cookie, Depends, FastAPI, Header, HTTPException, Request
from starlette.middleware.cors import CORSMiddleware

from aplicacion.composicion import incluir_modulos
from aplicacion.modulos.administracion.repositorio import RepositorioSqlAdministracion
from aplicacion.modulos.asistencia.repositorio import RepositorioSqlAsistencia
from aplicacion.modulos.auditoria.repositorio import RepositorioSqlAuditoria
from aplicacion.modulos.beneficios.repositorio import RepositorioSqlBeneficios
from aplicacion.modulos.comedor.repositorio import RepositorioSqlComedor
from aplicacion.modulos.cuentas.repositorio import RepositorioSqlCuentas
from aplicacion.modulos.estudiantes.repositorio_completo import RepositorioSqlEstudiantesCompleto
from aplicacion.modulos.identidad.esquemas import SesionPersistida
from aplicacion.modulos.identidad.repositorio import (
    RepositorioSqlSesiones,
    RepositorioSqlSesionesEstudiante,
    RepositorioSqlUsuarios,
)
from aplicacion.modulos.identidad.servicio import (
    AutenticacionFallida,
    ServicioIdentidad,
    ServicioPermisos,
)
from aplicacion.modulos.importaciones.repositorio import RepositorioSqlImportaciones
from aplicacion.modulos.menu.repositorio import RepositorioSqlMenu
from aplicacion.modulos.parametros.repositorio import RepositorioSqlParametros
from aplicacion.modulos.reportes.repositorio import RepositorioSqlReportes
from aplicacion.modulos.salud.repositorio import RepositorioSalud
from aplicacion.modulos.soporte.repositorio import RepositorioSqlSoporte
from aplicacion.modulos.transporte.repositorio import RepositorioSqlRutas
from aplicacion.nucleo.base_datos import FabricaConexionSql
from config import Settings


@dataclass(frozen=True)
class DependenciasAplicacion:
    """Puertos de composición; el valor predeterminado siempre es SQL real."""

    fabrica_sql: FabricaConexionSql
    cookies_seguras: bool = True


def _ip_cliente(request: Request) -> str:
    settings = Settings.from_environment()
    cliente = request.client.host if request.client else ""
    try:
        confiable = any(
            ipaddress.ip_address(cliente) in ipaddress.ip_network(red)
            for red in settings.trusted_proxy_cidrs
        )
    except ValueError:
        confiable = False
    reenviado = request.headers.get("x-forwarded-for", "").split(",", 1)[0].strip()
    return (reenviado if confiable and reenviado else cliente or "WEB")[:64]


def crear_aplicacion(dependencias: DependenciasAplicacion | None = None) -> FastAPI:
    """Crea la aplicación canónica; sin dependencias inyectadas carga SQL del entorno."""
    configuracion = Settings.from_environment() if dependencias is None else None
    if dependencias is None:
        assert configuracion is not None
        dependencias = DependenciasAplicacion(
            FabricaConexionSql(configuracion.sql_connection_string), configuracion.cookie_secure
        )
    fabrica = dependencias.fabrica_sql
    repositorio_salud = RepositorioSalud(fabrica)

    def obtener_identidad() -> ServicioIdentidad:
        return ServicioIdentidad(
            RepositorioSqlUsuarios(fabrica),
            RepositorioSqlSesiones(fabrica),
            timedelta(minutes=(configuracion.admin_session_minutes if configuracion else 60)),
        )

    def obtener_identidad_estudiante() -> ServicioIdentidad:
        return ServicioIdentidad(
            RepositorioSqlUsuarios(fabrica),
            RepositorioSqlSesionesEstudiante(fabrica),
            timedelta(days=(configuracion.dias_sesion_estudiante if configuracion else 365)),
        )

    def obtener_rutas() -> Iterator[RepositorioSqlRutas]:
        yield RepositorioSqlRutas(fabrica)

    def obtener_asistencia() -> Iterator[RepositorioSqlAsistencia]:
        yield RepositorioSqlAsistencia(fabrica)

    def obtener_estudiantes() -> Iterator[RepositorioSqlEstudiantesCompleto]:
        yield RepositorioSqlEstudiantesCompleto(fabrica)

    def obtener_beneficios() -> Iterator[RepositorioSqlBeneficios]:
        yield RepositorioSqlBeneficios(fabrica)

    def obtener_cuentas() -> Iterator[RepositorioSqlCuentas]:
        yield RepositorioSqlCuentas(fabrica)

    def obtener_reportes() -> Iterator[RepositorioSqlReportes]:
        yield RepositorioSqlReportes(fabrica)

    def obtener_importaciones() -> Iterator[RepositorioSqlImportaciones]:
        yield RepositorioSqlImportaciones(fabrica)

    def obtener_auditoria() -> Iterator[RepositorioSqlAuditoria]:
        yield RepositorioSqlAuditoria(fabrica)

    def obtener_administracion() -> Iterator[RepositorioSqlAdministracion]:
        yield RepositorioSqlAdministracion(fabrica)

    def obtener_parametros() -> Iterator[RepositorioSqlParametros]:
        yield RepositorioSqlParametros(fabrica)

    def obtener_menu() -> Iterator[RepositorioSqlMenu]:
        yield RepositorioSqlMenu(fabrica)

    def obtener_comedor() -> Iterator[RepositorioSqlComedor]:
        yield RepositorioSqlComedor(fabrica)

    def obtener_soporte() -> Iterator[RepositorioSqlSoporte]:
        yield RepositorioSqlSoporte(fabrica)

    def sesion_actual(
        identidad: ServicioIdentidad = Depends(obtener_identidad),
        id_sesion: str | None = Cookie(default=None, alias="id_sesion"),
        secreto: str | None = Cookie(default=None, alias="secreto_sesion"),
    ) -> tuple[ServicioIdentidad, SesionPersistida]:
        if not id_sesion or not secreto:
            raise HTTPException(401, "La sesión no es válida")
        try:
            return identidad, identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(401, str(exc)) from exc

    # Las dependencias de cookies/CSRF se definen explícitamente en el router
    # de identidad; aquí solo se inyectan los puertos de Transporte.
    def exigir_permiso(permiso: str) -> Callable:
        def dependencia(
            datos: tuple[ServicioIdentidad, SesionPersistida] = Depends(sesion_actual),
        ) -> dict[str, object]:
            identidad, sesion = datos
            try:
                permisos = identidad.permisos_de_sesion(sesion)
            except AutenticacionFallida as exc:
                raise HTTPException(401, str(exc)) from exc
            if not ServicioPermisos.tiene(permisos, permiso):
                raise HTTPException(403, "No tiene permiso para esta operación")
            return {"idUsuario": sesion.id_usuario, "permisos": permisos}

        return dependencia

    def exigir_csrf(
        datos: tuple[ServicioIdentidad, SesionPersistida] = Depends(sesion_actual),
        token: str | None = Header(default=None, alias="X-CSRF-Token"),
        cookie: str | None = Cookie(default=None, alias="csrf_token"),
    ) -> dict[str, object]:
        identidad, sesion = datos
        if not token or token != cookie or not identidad.validar_csrf(sesion, token):
            raise HTTPException(403, "El token CSRF no es válido")
        return {"idUsuario": sesion.id_usuario}

    aplicacion = FastAPI(title="Plataforma web modular")

    @aplicacion.get("/api/health", tags=["operacion"])
    def consultar_salud() -> dict[str, str]:
        """Comprueba que el proceso puede abrir SQL Server."""
        try:
            repositorio_salud.comprobar_conexion()
        except Exception as exc:
            raise HTTPException(503, "SQL no disponible") from exc
        return {"status": "ok"}

    @aplicacion.get("/api/ready", tags=["operacion"])
    def consultar_disponibilidad() -> dict[str, str]:
        """Readiness para el orquestador; exige la misma comprobación que health."""
        consultar_salud()
        return {"status": "ready"}

    if configuracion is not None:
        aplicacion.add_middleware(
            CORSMiddleware,
            allow_origins=[configuracion.cors_origin],
            allow_credentials=True,
            allow_methods=["GET", "POST", "PUT"],
            allow_headers=["Content-Type", "X-CSRF-Token"],
        )
    incluir_modulos(
        aplicacion,
        dependencias_transporte={
            "obtener_repositorio": obtener_rutas,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
            "obtener_ip": _ip_cliente,
        },
        dependencias_estudiantes={
            "obtener_repositorio": obtener_estudiantes,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
            "obtener_ip": _ip_cliente,
            "obtener_identidad": obtener_identidad,
            "obtener_identidad_estudiante": obtener_identidad_estudiante,
            "obtener_menu": obtener_menu,
            "obtener_asistencia": obtener_asistencia,
            "cookies_seguras": dependencias.cookies_seguras,
            "duracion_sesion_estudiante": configuracion.dias_sesion_estudiante * 24 * 60 * 60
            if configuracion
            else 31536000,
        },
        dependencias_identidad={
            "obtener_servicio": obtener_identidad,
            "obtener_servicio_estudiante": obtener_identidad_estudiante,
            "obtener_repositorio_estudiante": lambda: next(obtener_estudiantes()),
            "cookies_seguras": dependencias.cookies_seguras,
        },
        dependencias_asistencia={
            "obtener_repositorio": obtener_asistencia,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
            "obtener_ip": _ip_cliente,
        },
        dependencias_beneficios={
            "obtener_repositorio": obtener_beneficios,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
            "obtener_ip": _ip_cliente,
        },
        dependencias_cuentas={
            "obtener_repositorio": obtener_cuentas,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
            "obtener_ip": _ip_cliente,
        },
        dependencias_reportes={
            "obtener_repositorio": obtener_reportes,
            "exigir_permiso": exigir_permiso,
        },
        dependencias_importaciones={
            "obtener_repositorio": obtener_importaciones,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
        },
        dependencias_auditoria={
            "obtener_repositorio": obtener_auditoria,
            "exigir_permiso": exigir_permiso,
        },
        dependencias_administracion={
            "obtener_repositorio": obtener_administracion,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
        },
        dependencias_parametros={
            "obtener_repositorio": obtener_parametros,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
        },
        dependencias_menu={
            "obtener_repositorio": obtener_menu,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
        },
        dependencias_comedor={
            "obtener_repositorio": obtener_comedor,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
        },
        dependencias_soporte={
            "obtener_repositorio": obtener_soporte,
            "exigir_permiso": exigir_permiso,
            "exigir_csrf": exigir_csrf,
        },
    )
    return aplicacion
