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


def crear_router(obtener_servicio, administrador) -> APIRouter:
    router = APIRouter(dependencies=[Depends(administrador)])

    @router.get("/personas")
    async def personas(servicio=Depends(obtener_servicio)):
        return servicio.listar_personas()

    @router.post(
        "/personas", response_model=PersonaSalida, response_model_by_alias=True, status_code=201
    )
    async def crear_persona(datos: PersonaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_persona(datos)

    @router.get("/anios-lectivos")
    async def anios(servicio=Depends(obtener_servicio)):
        return servicio.listar_anios()

    @router.post("/anios-lectivos", status_code=201)
    async def crear_anio(datos: AnioEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_anio(datos)

    @router.post("/anios-lectivos/{anio_id}/activar")
    async def activar(anio_id: int, servicio=Depends(obtener_servicio)):
        return servicio.activar_anio(anio_id)

    @router.get("/matriculas")
    async def matriculas(anio_id: int | None = None, servicio=Depends(obtener_servicio)):
        return servicio.listar_matriculas(anio_id)

    @router.post("/matriculas", status_code=201)
    async def crear_matricula(datos: MatriculaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_matricula(datos)

    @router.get("/rutas")
    async def rutas(servicio=Depends(obtener_servicio)):
        return servicio.listar_rutas()

    @router.post("/rutas", status_code=201)
    async def crear_ruta(datos: RutaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_ruta(datos)

    @router.post("/rutas/{ruta_id}/asignaciones", status_code=201)
    async def asignar(
        ruta_id: int, datos: AsignacionRutaEntrada, servicio=Depends(obtener_servicio)
    ):
        return servicio.asignar_ruta(ruta_id, datos)

    return router
