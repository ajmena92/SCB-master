"""Rutas administrativas del dominio de estudiantes."""

from __future__ import annotations

from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends, HTTPException

from aplicacion.modulos.identidad.seguridad import hash_contrasena

from .esquemas import CambioAsignacion, GeneracionPinesSeccion, PinGenerado
from .pines import construir_filas, generar_pin, seleccionar_estudiantes


def crear_enrutador_administracion(
    obtener_repositorio: Callable[[], Iterator],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
) -> APIRouter:
    """Construye las rutas administrativas de estudiantes."""
    enrutador = APIRouter()

    @enrutador.get("/secciones")
    def secciones(
        turno: str | None = None,
        _=Depends(exigir_permiso("estudiantes.leer")),
        repo=Depends(obtener_repositorio),
    ):
        return repo.secciones(turno)

    @enrutador.get("/{id_estudiante}/perfil")
    def perfil(
        id_estudiante: int,
        _=Depends(exigir_permiso("estudiantes.leer")),
        repo=Depends(obtener_repositorio),
    ):
        return repo.perfil_detallado(id_estudiante)

    @enrutador.put("/{id_estudiante}/beneficio", status_code=204)
    def beneficio(
        id_estudiante: int,
        datos: CambioAsignacion,
        _=Depends(exigir_permiso("beneficios.editar")),
        __=Depends(exigir_csrf),
        repo=Depends(obtener_repositorio),
    ):
        repo.asignar_beneficio(id_estudiante, datos.id_beneficio)

    @enrutador.put("/{id_estudiante}/ruta", status_code=204)
    def ruta(
        id_estudiante: int,
        datos: CambioAsignacion,
        _=Depends(exigir_permiso("rutas.administrar")),
        __=Depends(exigir_csrf),
        repo=Depends(obtener_repositorio),
    ):
        repo.asignar_ruta(id_estudiante, datos.id_ruta)

    @enrutador.post(
        "/{id_estudiante}/reset-pin",
        response_model=PinGenerado,
        response_model_by_alias=True,
    )
    def reset_pin(
        id_estudiante: int,
        _=Depends(exigir_permiso("estudiantes.editar")),
        __=Depends(exigir_csrf),
        repo=Depends(obtener_repositorio),
    ):
        pin = generar_pin()
        repo.reiniciar_pin(id_estudiante, hash_contrasena(pin))
        return PinGenerado(idEstudiante=id_estudiante, pin=pin)

    @enrutador.post("/pines/seccion")
    def generar_pines_seccion(
        datos: GeneracionPinesSeccion,
        _=Depends(exigir_permiso("estudiantes.editar")),
        __=Depends(exigir_csrf),
        repo=Depends(obtener_repositorio),
    ):
        datos.seccion = datos.seccion.strip() if datos.seccion and datos.seccion.strip() else None
        estudiantes = seleccionar_estudiantes(
            repo.listar_para_generacion_pines(), datos.seccion, datos.turno
        )
        generados = [
            PinGenerado(idEstudiante=int(estudiante["id_estudiante"]), pin=generar_pin())
            for estudiante in estudiantes
        ]
        repo.actualizar_pines_seccion(
            datos.seccion, {pin.id_estudiante: hash_contrasena(pin.pin) for pin in generados}
        )
        filas = construir_filas(estudiantes, generados, datos.seccion)
        if not filas:
            raise HTTPException(404, "No hay estudiantes para la sección indicada")
        return {
            "total": len(filas),
            "seccion": datos.seccion or "Sin sección",
            "turno": datos.turno,
            "estudiantes": filas,
        }

    return enrutador
