"""Fábricas de dependencias de infraestructura para los módulos web."""

from __future__ import annotations

from collections.abc import Callable, Iterator
from dataclasses import dataclass
from datetime import timedelta
from typing import Any, TypedDict

from aplicacion.modulos.administracion.repositorio import RepositorioSqlAdministracion
from aplicacion.modulos.asistencia.repositorio import RepositorioSqlAsistencia
from aplicacion.modulos.auditoria.repositorio import RepositorioSqlAuditoria
from aplicacion.modulos.beneficios.repositorio import RepositorioSqlBeneficios
from aplicacion.modulos.comedor.repositorio import RepositorioSqlComedor
from aplicacion.modulos.cuentas.repositorio import RepositorioSqlCuentas
from aplicacion.modulos.estudiantes.repositorio_completo import RepositorioSqlEstudiantesCompleto
from aplicacion.modulos.estudiantes.servicio_perfil import crear_perfil_sesion
from aplicacion.modulos.identidad.repositorio import (
    RepositorioSqlSesiones,
    RepositorioSqlSesionesEstudiante,
    RepositorioSqlUsuarios,
)
from aplicacion.modulos.identidad.servicio import ServicioIdentidad, ServicioSesiones
from aplicacion.modulos.importaciones.repositorio import RepositorioSqlImportaciones
from aplicacion.modulos.menu.repositorio import RepositorioSqlMenu
from aplicacion.modulos.parametros.repositorio import RepositorioSqlParametros
from aplicacion.modulos.reportes.repositorio import RepositorioSqlReportes
from aplicacion.modulos.soporte.repositorio import RepositorioSqlSoporte
from aplicacion.modulos.transporte.repositorio import RepositorioSqlRutas
from aplicacion.nucleo.base_datos import FabricaConexionSql
from config import Settings


@dataclass(frozen=True)
class DependenciasAplicacion:
    """Puertos de composición; el valor predeterminado siempre es SQL real."""

    fabrica_sql: FabricaConexionSql
    cookies_seguras: bool = True


class ContratoDependenciasModulo(TypedDict, total=False):
    """Contrato tipado de los puertos que puede consumir un módulo HTTP."""

    obtener_repositorio: Callable[..., Any]
    exigir_permiso: Callable[..., Callable[..., Any]]
    exigir_csrf: Callable[..., Any]
    obtener_ip: Callable[..., str]
    obtener_identidad: Callable[[], ServicioIdentidad]
    obtener_identidad_estudiante: Callable[[], ServicioIdentidad]
    obtener_menu: Callable[..., Any]
    obtener_asistencia: Callable[..., Any]
    cookies_seguras: bool
    duracion_sesion_estudiante: int
    obtener_servicio: Callable[[], ServicioIdentidad]
    obtener_servicio_estudiante: Callable[[], ServicioIdentidad]
    obtener_repositorio_estudiante: Callable[[], Any]
    obtener_sesiones: Callable[[], ServicioSesiones]


class DependenciasModulos(TypedDict):
    """Mapa cerrado de contratos que se entrega a la composición."""

    dependencias_transporte: ContratoDependenciasModulo
    dependencias_identidad: ContratoDependenciasModulo
    dependencias_estudiantes: ContratoDependenciasModulo
    dependencias_asistencia: ContratoDependenciasModulo
    dependencias_beneficios: ContratoDependenciasModulo
    dependencias_cuentas: ContratoDependenciasModulo
    dependencias_reportes: ContratoDependenciasModulo
    dependencias_importaciones: ContratoDependenciasModulo
    dependencias_auditoria: ContratoDependenciasModulo
    dependencias_administracion: ContratoDependenciasModulo
    dependencias_parametros: ContratoDependenciasModulo
    dependencias_menu: ContratoDependenciasModulo
    dependencias_comedor: ContratoDependenciasModulo
    dependencias_soporte: ContratoDependenciasModulo


def crear_servicios_identidad(
    fabrica: FabricaConexionSql, configuracion: Settings | None
) -> dict[str, Callable[[], ServicioIdentidad]]:
    """Crea las fábricas de servicios de sesión de administración y estudiantes."""

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

    return {
        "obtener_identidad": obtener_identidad,
        "obtener_identidad_estudiante": obtener_identidad_estudiante,
    }


def crear_fabricas_repositorios(fabrica: FabricaConexionSql) -> dict[str, Any]:
    """Crea las fábricas de repositorios sin mezclar autorización ni HTTP."""

    def fabrica_de(tipo: type) -> Iterator[object]:
        yield tipo(fabrica)

    def obtener_estudiante() -> object:
        return next(fabrica_de(RepositorioSqlEstudiantesCompleto))

    return {
        "obtener_rutas": lambda: fabrica_de(RepositorioSqlRutas),
        "obtener_asistencia": lambda: fabrica_de(RepositorioSqlAsistencia),
        "obtener_estudiantes": lambda: fabrica_de(RepositorioSqlEstudiantesCompleto),
        "obtener_estudiante": obtener_estudiante,
        "obtener_beneficios": lambda: fabrica_de(RepositorioSqlBeneficios),
        "obtener_cuentas": lambda: fabrica_de(RepositorioSqlCuentas),
        "obtener_reportes": lambda: fabrica_de(RepositorioSqlReportes),
        "obtener_importaciones": lambda: fabrica_de(RepositorioSqlImportaciones),
        "obtener_auditoria": lambda: fabrica_de(RepositorioSqlAuditoria),
        "obtener_administracion": lambda: fabrica_de(RepositorioSqlAdministracion),
        "obtener_parametros": lambda: fabrica_de(RepositorioSqlParametros),
        "obtener_menu": lambda: fabrica_de(RepositorioSqlMenu),
        "obtener_comedor": lambda: fabrica_de(RepositorioSqlComedor),
        "obtener_soporte": lambda: fabrica_de(RepositorioSqlSoporte),
    }


def crear_servicio_sesiones(
    fabrica: FabricaConexionSql, configuracion: Settings | None
) -> Callable[[], ServicioSesiones]:
    """Crea el servicio neutral que resuelve el ciclo de vida de cualquier sesión."""

    def obtener_sesiones() -> ServicioSesiones:
        servicios = crear_servicios_identidad(fabrica, configuracion)
        repositorio = next(crear_fabricas_repositorios(fabrica)["obtener_estudiantes"]())
        return ServicioSesiones(
            servicios["obtener_identidad"](),
            servicios["obtener_identidad_estudiante"](),
            lambda id_estudiante: crear_perfil_sesion(id_estudiante, repositorio.buscar_por_id),
        )

    return obtener_sesiones


def crear_dependencias_modulos(
    dependencias: DependenciasAplicacion, configuracion: Settings | None
) -> DependenciasModulos:
    """Construye todos los contratos de módulos fuera del ensamblador HTTP."""

    from aplicacion.seguridad_dependencias import crear_dependencias_seguridad, ip_cliente

    servicios = crear_servicios_identidad(dependencias.fabrica_sql, configuracion)
    seguridad = crear_dependencias_seguridad(servicios["obtener_identidad"])
    repositorios = crear_fabricas_repositorios(dependencias.fabrica_sql)

    def contrato_repositorio(
        nombre: str, requiere_ip: bool = False, requiere_csrf: bool = True
    ) -> ContratoDependenciasModulo:
        contrato: ContratoDependenciasModulo = {
            "obtener_repositorio": repositorios[f"obtener_{nombre}"],
            "exigir_permiso": seguridad["exigir_permiso"],
            "exigir_csrf": seguridad["exigir_csrf"],
        }
        if requiere_ip:
            contrato["obtener_ip"] = ip_cliente
        if not requiere_csrf:
            contrato.pop("exigir_csrf", None)
        return contrato

    contrato_identidad = ContratoDependenciasModulo(
        obtener_servicio=servicios["obtener_identidad"],
        obtener_servicio_estudiante=servicios["obtener_identidad_estudiante"],
        obtener_repositorio_estudiante=repositorios["obtener_estudiante"],
        cookies_seguras=dependencias.cookies_seguras,
        obtener_sesiones=crear_servicio_sesiones(dependencias.fabrica_sql, configuracion),
    )
    return {
        "dependencias_transporte": contrato_repositorio("rutas", True),
        "dependencias_identidad": contrato_identidad,
        "dependencias_estudiantes": {
            **contrato_repositorio("estudiantes", True),
            "obtener_identidad": servicios["obtener_identidad"],
            "obtener_identidad_estudiante": servicios["obtener_identidad_estudiante"],
            "obtener_menu": repositorios["obtener_menu"],
            "obtener_asistencia": repositorios["obtener_asistencia"],
            "cookies_seguras": dependencias.cookies_seguras,
            "duracion_sesion_estudiante": configuracion.dias_sesion_estudiante * 24 * 60 * 60
            if configuracion
            else 31536000,
        },
        "dependencias_asistencia": contrato_repositorio("asistencia", True),
        "dependencias_beneficios": contrato_repositorio("beneficios", True),
        "dependencias_cuentas": contrato_repositorio("cuentas", True),
        "dependencias_reportes": contrato_repositorio("reportes", requiere_csrf=False),
        "dependencias_importaciones": contrato_repositorio("importaciones"),
        "dependencias_auditoria": contrato_repositorio("auditoria", requiere_csrf=False),
        "dependencias_administracion": contrato_repositorio("administracion"),
        "dependencias_parametros": contrato_repositorio("parametros"),
        "dependencias_menu": contrato_repositorio("menu"),
        "dependencias_comedor": contrato_repositorio("comedor"),
        "dependencias_soporte": contrato_repositorio("soporte"),
    }
