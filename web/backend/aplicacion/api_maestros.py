"""Adaptador HTTP de datos maestros."""

from fastapi import APIRouter, Depends

from aplicacion.esquemas import (
    AnioEntrada,
    AsignacionRutaEntrada,
    MatriculaEntrada,
    PersonaEntrada,
    PersonaSalida,
    RutaEntrada,
)
from aplicacion.paleta_rutas import opciones


def crear_router(obtener_servicio, exigir_permiso, exigir_alguno) -> APIRouter:
    router = APIRouter()

    @router.get("/personas", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def personas(servicio=Depends(obtener_servicio)):
        return servicio.listar_personas()

    @router.post(
        "/personas",
        response_model=PersonaSalida,
        response_model_by_alias=True,
        status_code=201,
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def crear_persona(datos: PersonaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_persona(datos)

    @router.get(
        "/anios-lectivos",
        dependencies=[Depends(exigir_alguno("personas.administrar", "importaciones.administrar"))],
    )
    async def anios(servicio=Depends(obtener_servicio)):
        return servicio.listar_anios()

    @router.post(
        "/anios-lectivos",
        status_code=201,
        dependencies=[Depends(exigir_permiso("importaciones.administrar"))],
    )
    async def crear_anio(datos: AnioEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_anio(datos)

    @router.post(
        "/anios-lectivos/{anio_id}/activar",
        dependencies=[Depends(exigir_permiso("importaciones.administrar"))],
    )
    async def activar(anio_id: int, servicio=Depends(obtener_servicio)):
        return servicio.activar_anio(anio_id)

    @router.get(
        "/matriculas",
        dependencies=[Depends(exigir_alguno("personas.administrar", "rutas.administrar"))],
    )
    async def matriculas(anio_id: int | None = None, servicio=Depends(obtener_servicio)):
        return servicio.listar_matriculas(anio_id)

    @router.post(
        "/matriculas",
        status_code=201,
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def crear_matricula(datos: MatriculaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_matricula(datos)

    @router.get(
        "/rutas",
        dependencies=[Depends(exigir_alguno("transporte.operar", "rutas.administrar"))],
    )
    async def rutas(servicio=Depends(obtener_servicio)):
        return servicio.listar_rutas()

    @router.get("/rutas/paleta", dependencies=[Depends(exigir_permiso("rutas.administrar"))])
    async def paleta_rutas():
        return opciones()

    @router.post(
        "/rutas", status_code=201, dependencies=[Depends(exigir_permiso("rutas.administrar"))]
    )
    async def crear_ruta(datos: RutaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_ruta(datos)

    @router.put("/rutas/{ruta_id}", dependencies=[Depends(exigir_permiso("rutas.administrar"))])
    async def actualizar_ruta(ruta_id: int, datos: RutaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.actualizar_ruta(ruta_id, datos)

    @router.post(
        "/rutas/{ruta_id}/asignaciones",
        status_code=201,
        dependencies=[Depends(exigir_permiso("rutas.administrar"))],
    )
    async def asignar(
        ruta_id: int, datos: AsignacionRutaEntrada, servicio=Depends(obtener_servicio)
    ):
        return servicio.asignar_ruta(ruta_id, datos)

    return router
