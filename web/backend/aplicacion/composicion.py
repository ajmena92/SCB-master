"""Punto único de composición de los módulos canónicos de la aplicación."""

from fastapi import APIRouter, FastAPI

from aplicacion.modulos.administracion.api import crear_enrutador as crear_enrutador_administracion
from aplicacion.modulos.asistencia.api import crear_enrutador as crear_enrutador_asistencia
from aplicacion.modulos.auditoria.api import crear_enrutador as crear_enrutador_auditoria
from aplicacion.modulos.beneficios.api import crear_enrutador as crear_enrutador_beneficios
from aplicacion.modulos.comedor.api import crear_enrutador as crear_enrutador_comedor
from aplicacion.modulos.cuentas.api import crear_enrutador as crear_enrutador_cuentas
from aplicacion.modulos.estudiantes.api import crear_enrutador as crear_enrutador_estudiantes
from aplicacion.modulos.estudiantes.fotos import crear_enrutador_fotos
from aplicacion.modulos.estudiantes.operaciones import crear_enrutador_operaciones
from aplicacion.modulos.importaciones.api import crear_enrutador as crear_enrutador_importaciones
from aplicacion.modulos.menu.api import crear_enrutador as crear_enrutador_menu
from aplicacion.modulos.parametros.api import crear_enrutador as crear_enrutador_parametros
from aplicacion.modulos.reportes.api import crear_enrutador as crear_enrutador_reportes
from aplicacion.modulos.salud.api import enrutador as enrutador_salud
from aplicacion.modulos.soporte.api import crear_enrutador as crear_enrutador_soporte
from aplicacion.modulos.transporte.api import crear_enrutador as crear_enrutador_transporte
from aplicacion.nucleo.identidad.api import crear_enrutador as crear_enrutador_identidad

_MARCA_MODULOS_INCLUIDOS = "modulos_aplicacion_incluidos"


def crear_enrutador_aplicacion(
    dependencias_transporte: dict | None = None,
    dependencias_identidad: dict | None = None,
    dependencias_estudiantes: dict | None = None,
    dependencias_asistencia: dict | None = None,
    dependencias_beneficios: dict | None = None,
    dependencias_cuentas: dict | None = None,
    dependencias_reportes: dict | None = None,
    dependencias_importaciones: dict | None = None,
    dependencias_auditoria: dict | None = None,
    dependencias_administracion: dict | None = None,
    dependencias_parametros: dict | None = None,
    dependencias_menu: dict | None = None,
    dependencias_comedor: dict | None = None,
    dependencias_soporte: dict | None = None,
) -> APIRouter:
    """Construye el arbol versionado sin conocer detalles de cada dominio."""
    enrutador = APIRouter(prefix="/api/v1")
    enrutador.include_router(enrutador_salud)
    if dependencias_estudiantes:
        dependencias_estudiantes_base = {
            k: dependencias_estudiantes[k]
            for k in ("obtener_repositorio", "exigir_permiso", "exigir_csrf", "obtener_ip")
        }
        # Las rutas literales del portal deben registrarse antes de
        # /estudiantes/{id_estudiante}, que de lo contrario intentaría
        # convertir "menu", "carnet" o "asistencia" a entero.
        enrutador.include_router(crear_enrutador_operaciones(**dependencias_estudiantes))
        enrutador.include_router(crear_enrutador_estudiantes(**dependencias_estudiantes_base))
        enrutador.include_router(crear_enrutador_fotos(**dependencias_estudiantes_base))
    if dependencias_identidad:
        enrutador.include_router(crear_enrutador_identidad(**dependencias_identidad))
    if dependencias_transporte:
        # La composicion conoce unicamente las dependencias del contrato
        # autonomo de Transporte. Las claves sobrantes de integraciones
        # antiguas no forman parte de este contrato y se ignoran aqui.
        enrutador.include_router(
            crear_enrutador_transporte(
                obtener_repositorio=dependencias_transporte["obtener_repositorio"],
                exigir_permiso=dependencias_transporte["exigir_permiso"],
                exigir_csrf=dependencias_transporte["exigir_csrf"],
                obtener_ip=dependencias_transporte["obtener_ip"],
            )
        )
    if dependencias_asistencia:
        enrutador.include_router(
            crear_enrutador_asistencia(
                obtener_repositorio=dependencias_asistencia["obtener_repositorio"],
                exigir_permiso=dependencias_asistencia["exigir_permiso"],
                exigir_csrf=dependencias_asistencia["exigir_csrf"],
                obtener_ip=dependencias_asistencia["obtener_ip"],
            )
        )
    if dependencias_beneficios:
        enrutador.include_router(
            crear_enrutador_beneficios(
                obtener_repositorio=dependencias_beneficios["obtener_repositorio"],
                exigir_permiso=dependencias_beneficios["exigir_permiso"],
                exigir_csrf=dependencias_beneficios["exigir_csrf"],
                obtener_ip=dependencias_beneficios["obtener_ip"],
            )
        )
    if dependencias_cuentas:
        enrutador.include_router(
            crear_enrutador_cuentas(
                obtener_repositorio=dependencias_cuentas["obtener_repositorio"],
                exigir_permiso=dependencias_cuentas["exigir_permiso"],
                exigir_csrf=dependencias_cuentas["exigir_csrf"],
                obtener_ip=dependencias_cuentas["obtener_ip"],
            )
        )
    if dependencias_reportes:
        enrutador.include_router(
            crear_enrutador_reportes(
                obtener_repositorio=dependencias_reportes["obtener_repositorio"],
                exigir_permiso=dependencias_reportes["exigir_permiso"],
            )
        )
    if dependencias_importaciones:
        enrutador.include_router(crear_enrutador_importaciones(**dependencias_importaciones))
    if dependencias_auditoria:
        enrutador.include_router(crear_enrutador_auditoria(**dependencias_auditoria))
    if dependencias_administracion:
        enrutador.include_router(crear_enrutador_administracion(**dependencias_administracion))
    if dependencias_parametros:
        enrutador.include_router(crear_enrutador_parametros(**dependencias_parametros))
    if dependencias_menu:
        enrutador.include_router(crear_enrutador_menu(**dependencias_menu))
    if dependencias_comedor:
        enrutador.include_router(crear_enrutador_comedor(**dependencias_comedor))
    if dependencias_soporte:
        enrutador.include_router(crear_enrutador_soporte(**dependencias_soporte))
    return enrutador


def incluir_modulos(
    aplicacion: FastAPI,
    dependencias_transporte: dict | None = None,
    dependencias_identidad: dict | None = None,
    dependencias_estudiantes: dict | None = None,
    dependencias_asistencia: dict | None = None,
    dependencias_beneficios: dict | None = None,
    dependencias_cuentas: dict | None = None,
    dependencias_reportes: dict | None = None,
    dependencias_importaciones: dict | None = None,
    dependencias_auditoria: dict | None = None,
    dependencias_administracion: dict | None = None,
    dependencias_parametros: dict | None = None,
    dependencias_menu: dict | None = None,
    dependencias_comedor: dict | None = None,
    dependencias_soporte: dict | None = None,
) -> None:
    """Incluye los modulos una sola vez en una instancia FastAPI."""
    if getattr(aplicacion.state, _MARCA_MODULOS_INCLUIDOS, False):
        return
    aplicacion.include_router(
        crear_enrutador_aplicacion(
            dependencias_transporte,
            dependencias_identidad,
            dependencias_estudiantes,
            dependencias_asistencia,
            dependencias_beneficios,
            dependencias_cuentas,
            dependencias_reportes,
            dependencias_importaciones,
            dependencias_auditoria,
            dependencias_administracion,
            dependencias_parametros,
            dependencias_menu,
            dependencias_comedor,
            dependencias_soporte,
        )
    )
    setattr(aplicacion.state, _MARCA_MODULOS_INCLUIDOS, True)
