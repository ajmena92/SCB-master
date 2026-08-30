"""Punto único de composición de los módulos canónicos de la aplicación."""

from typing import Any, cast

from fastapi import APIRouter, FastAPI

from aplicacion.dependencias import ContratoDependenciasModulo
from aplicacion.modulos.administracion.api import crear_enrutador as crear_enrutador_administracion
from aplicacion.modulos.asistencia.api import crear_enrutador as crear_enrutador_asistencia
from aplicacion.modulos.auditoria.api import crear_enrutador as crear_enrutador_auditoria
from aplicacion.modulos.comedor.api import crear_enrutador as crear_enrutador_comedor
from aplicacion.modulos.comedor.profesor_portal import crear_enrutador_profesores
from aplicacion.modulos.cuentas.api import crear_enrutador as crear_enrutador_cuentas
from aplicacion.modulos.estudiantes.api import crear_enrutador as crear_enrutador_estudiantes
from aplicacion.modulos.estudiantes.fotos import crear_enrutador_fotos
from aplicacion.modulos.estudiantes.operaciones import crear_enrutador_operaciones
from aplicacion.modulos.identidad.api_administracion import (
    crear_enrutador_administracion_identidad,
)
from aplicacion.modulos.identidad.api_sesion import crear_enrutador_sesion
from aplicacion.modulos.importaciones.api import crear_enrutador as crear_enrutador_importaciones
from aplicacion.modulos.menu.api import crear_enrutador as crear_enrutador_menu
from aplicacion.modulos.parametros.api import crear_enrutador as crear_enrutador_parametros
from aplicacion.modulos.reportes.api import crear_enrutador as crear_enrutador_reportes
from aplicacion.modulos.salud.api import enrutador as enrutador_salud
from aplicacion.modulos.soporte.api import crear_enrutador as crear_enrutador_soporte
from aplicacion.modulos.transporte.api import crear_enrutador as crear_enrutador_transporte

_MARCA_MODULOS_INCLUIDOS = "modulos_aplicacion_incluidos"


def _argumentos_router(contrato: ContratoDependenciasModulo) -> dict[str, Any]:
    """Adapta el contrato tipado al variádico de los adaptadores de cada módulo."""

    return cast(dict[str, Any], contrato)


def crear_enrutador_aplicacion(
    dependencias_transporte: ContratoDependenciasModulo | None = None,
    dependencias_identidad: ContratoDependenciasModulo | None = None,
    dependencias_estudiantes: ContratoDependenciasModulo | None = None,
    dependencias_asistencia: ContratoDependenciasModulo | None = None,
    dependencias_cuentas: ContratoDependenciasModulo | None = None,
    dependencias_reportes: ContratoDependenciasModulo | None = None,
    dependencias_importaciones: ContratoDependenciasModulo | None = None,
    dependencias_auditoria: ContratoDependenciasModulo | None = None,
    dependencias_administracion: ContratoDependenciasModulo | None = None,
    dependencias_parametros: ContratoDependenciasModulo | None = None,
    dependencias_menu: ContratoDependenciasModulo | None = None,
    dependencias_comedor: ContratoDependenciasModulo | None = None,
    dependencias_soporte: ContratoDependenciasModulo | None = None,
) -> APIRouter:
    """Construye el arbol versionado sin conocer detalles de cada dominio."""
    enrutador = APIRouter(prefix="/api/v1")
    enrutador.include_router(enrutador_salud)
    if dependencias_estudiantes:
        dependencias_estudiantes_base = cast(
            ContratoDependenciasModulo,
            {
                k: dependencias_estudiantes[k]
                for k in ("obtener_repositorio", "exigir_permiso", "exigir_csrf", "obtener_ip")
            },
        )
        # Las rutas literales del portal deben registrarse antes de
        # /estudiantes/{id_estudiante}, que de lo contrario intentaría
        # convertir "menu", "carnet" o "asistencia" a entero.
        enrutador.include_router(
            crear_enrutador_operaciones(
                obtener_repositorio=dependencias_estudiantes["obtener_repositorio"],
                exigir_permiso=dependencias_estudiantes["exigir_permiso"],
                exigir_csrf=dependencias_estudiantes["exigir_csrf"],
                obtener_identidad=dependencias_estudiantes["obtener_identidad"],
                obtener_identidad_estudiante=dependencias_estudiantes[
                    "obtener_identidad_estudiante"
                ],
                obtener_menu=dependencias_estudiantes["obtener_menu"],
                obtener_asistencia=dependencias_estudiantes["obtener_asistencia"],
                obtener_comedor=dependencias_estudiantes["obtener_comedor"],
                cookies_seguras=dependencias_estudiantes["cookies_seguras"],
                duracion_sesion_estudiante=dependencias_estudiantes["duracion_sesion_estudiante"],
                obtener_fecha_local=dependencias_estudiantes.get("obtener_fecha_local"),
            )
        )
        enrutador.include_router(
            crear_enrutador_estudiantes(**_argumentos_router(dependencias_estudiantes_base))
        )
        enrutador.include_router(
            crear_enrutador_fotos(
                obtener_repositorio=dependencias_estudiantes["obtener_repositorio"],
                exigir_permiso=dependencias_estudiantes["exigir_permiso"],
                exigir_csrf=dependencias_estudiantes["exigir_csrf"],
            )
        )
    if dependencias_identidad:
        enrutador.include_router(
            crear_enrutador_administracion_identidad(
                obtener_servicio=dependencias_identidad["obtener_servicio"],
                cookies_seguras=dependencias_identidad["cookies_seguras"],
            )
        )
        enrutador.include_router(
            crear_enrutador_sesion(
                obtener_servicio=dependencias_identidad["obtener_sesiones"],
            )
        )
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
        enrutador.include_router(
            crear_enrutador_importaciones(**_argumentos_router(dependencias_importaciones))
        )
    if dependencias_auditoria:
        enrutador.include_router(
            crear_enrutador_auditoria(**_argumentos_router(dependencias_auditoria))
        )
    if dependencias_administracion:
        enrutador.include_router(
            crear_enrutador_administracion(**_argumentos_router(dependencias_administracion))
        )
    if dependencias_parametros:
        enrutador.include_router(
            crear_enrutador_parametros(**_argumentos_router(dependencias_parametros))
        )
    if dependencias_menu:
        enrutador.include_router(crear_enrutador_menu(**_argumentos_router(dependencias_menu)))
    if dependencias_comedor:
        enrutador.include_router(
            crear_enrutador_comedor(
                obtener_repositorio=dependencias_comedor["obtener_repositorio"],
                exigir_permiso=dependencias_comedor["exigir_permiso"],
                exigir_csrf=dependencias_comedor["exigir_csrf"],
                obtener_identidad_estudiante=dependencias_comedor["obtener_identidad_estudiante"],
            )
        )
        enrutador.include_router(
            crear_enrutador_profesores(
                obtener_repositorio=dependencias_comedor["obtener_repositorio"],
                obtener_identidad=dependencias_comedor["obtener_identidad"],
                obtener_menu=dependencias_comedor["obtener_menu"],
                exigir_csrf=dependencias_comedor["exigir_csrf"],
                obtener_fecha_local=dependencias_comedor["obtener_fecha_local"],
            )
        )
    if dependencias_soporte:
        enrutador.include_router(
            crear_enrutador_soporte(**_argumentos_router(dependencias_soporte))
        )
    return enrutador


def incluir_modulos(
    aplicacion: FastAPI,
    dependencias_transporte: ContratoDependenciasModulo | None = None,
    dependencias_identidad: ContratoDependenciasModulo | None = None,
    dependencias_estudiantes: ContratoDependenciasModulo | None = None,
    dependencias_asistencia: ContratoDependenciasModulo | None = None,
    dependencias_cuentas: ContratoDependenciasModulo | None = None,
    dependencias_reportes: ContratoDependenciasModulo | None = None,
    dependencias_importaciones: ContratoDependenciasModulo | None = None,
    dependencias_auditoria: ContratoDependenciasModulo | None = None,
    dependencias_administracion: ContratoDependenciasModulo | None = None,
    dependencias_parametros: ContratoDependenciasModulo | None = None,
    dependencias_menu: ContratoDependenciasModulo | None = None,
    dependencias_comedor: ContratoDependenciasModulo | None = None,
    dependencias_soporte: ContratoDependenciasModulo | None = None,
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
