"""Adaptador HTTP de datos maestros."""

from fastapi import APIRouter, Depends, Query
from typing import Literal

from aplicacion.esquemas import (
    AnioEntrada,
    CicloMenuEntrada,
    AsignacionRutaEntrada,
    MatriculaEntrada,
    MatriculaBeneficioEntrada,
    MatriculaBeneficiosEntrada,
    PersonaEntrada,
    PersonaActualizacionEntrada,
    PersonaSalida,
    ResumenPersonasSalida,
    CambioRutaMatriculaEntrada,
    GeneracionPinesSeccionEntrada,
    RutaEntrada,
)
from aplicacion.paleta_rutas import opciones


def crear_router(obtener_servicio, exigir_permiso, exigir_alguno) -> APIRouter:
    router = APIRouter()

    @router.get("/personas", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def personas(
        buscar: str = "", estado: Literal["activos", "inactivos", "todos"] = "activos",
        tipo: Literal["estudiante", "profesor"] | None = None,
        pagina: int = Query(1, ge=1), tamano: int = Query(50, ge=1, le=100),
        ordenar_por: Literal["nombres", "cedula", "tipo", "estado"] = "nombres",
        direccion: Literal["asc", "desc"] = "asc",
        servicio=Depends(obtener_servicio),
    ):
        return servicio.listar_personas(buscar, estado, tipo, pagina, tamano, ordenar_por, direccion)

    @router.get(
        "/personas/resumen",
        response_model=ResumenPersonasSalida,
        response_model_by_alias=True,
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def resumen_personas(servicio=Depends(obtener_servicio)):
        return servicio.resumen_personas()

    @router.get(
        "/personas/{persona_id}",
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def obtener_persona(persona_id: int, servicio=Depends(obtener_servicio)):
        return servicio.obtener_persona(persona_id)

    @router.post(
        "/personas",
        response_model=PersonaSalida,
        response_model_by_alias=True,
        status_code=201,
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def crear_persona(datos: PersonaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_persona(datos)

    @router.post("/personas/pines/seccion", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def reiniciar_pines_seccion(datos: GeneracionPinesSeccionEntrada, usuario=Depends(exigir_permiso("personas.administrar")), servicio=Depends(obtener_servicio)):
        return servicio.reiniciar_pines_seccion(datos, usuario["cuenta"].id)

    @router.put("/personas/{persona_id}", response_model=PersonaSalida, response_model_by_alias=True,
                dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def actualizar_persona(persona_id: int, datos: PersonaActualizacionEntrada, servicio=Depends(obtener_servicio)):
        return servicio.actualizar_persona(persona_id, datos)

    @router.post("/personas/{persona_id}/desactivar", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def desactivar_persona(persona_id: int, usuario=Depends(exigir_permiso("personas.administrar")), servicio=Depends(obtener_servicio)):
        return servicio.desactivar_persona(persona_id, usuario["cuenta"].id)

    @router.post("/personas/{persona_id}/reiniciar-pin", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def reiniciar_pin(persona_id: int, usuario=Depends(exigir_permiso("personas.administrar")), servicio=Depends(obtener_servicio)):
        return servicio.reiniciar_pin(persona_id, usuario["cuenta"].id)

    @router.get(
        "/anios-lectivos",
        dependencies=[Depends(exigir_alguno("personas.administrar", "importaciones.administrar", "menu.administrar"))],
    )
    async def anios(servicio=Depends(obtener_servicio)):
        return servicio.listar_anios()

    @router.get(
        "/anios-lectivos/{anio_id}/secciones",
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def secciones_anio(anio_id: int, servicio=Depends(obtener_servicio)):
        return servicio.listar_secciones_anio(anio_id)

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
        "/menu/ciclo",
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def ciclo_menu(servicio=Depends(obtener_servicio)):
        configuracion = servicio.configuracion_ciclo_menu()
        return (
            {"inicioCicloMenu": configuracion.inicio_ciclo_menu}
            if configuracion is not None
            else None
        )

    @router.put(
        "/menu/ciclo",
        dependencies=[Depends(exigir_permiso("menu.administrar"))],
    )
    async def configurar_ciclo_menu(datos: CicloMenuEntrada, servicio=Depends(obtener_servicio)):
        configuracion = servicio.configurar_ciclo_menu(datos)
        return {"inicioCicloMenu": configuracion.inicio_ciclo_menu}

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

    @router.put(
        "/matriculas/{matricula_id}/beneficios",
        dependencies=[Depends(exigir_permiso("personas.administrar"))],
    )
    async def actualizar_beneficios(
        matricula_id: int,
        datos: MatriculaBeneficiosEntrada,
        servicio=Depends(obtener_servicio),
    ):
        return servicio.actualizar_beneficios_matricula(matricula_id, datos)

    @router.put("/matriculas/{matricula_id}/beneficio-comedor", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def actualizar_beneficio_comedor(matricula_id: int, datos: MatriculaBeneficioEntrada, servicio=Depends(obtener_servicio)):
        return servicio.actualizar_beneficios_matricula(
            matricula_id, MatriculaBeneficiosEntrada(becado=datos.becado)
        )

    @router.put("/matriculas/{matricula_id}/ruta", dependencies=[Depends(exigir_permiso("personas.administrar"))])
    async def cambiar_ruta_matricula(matricula_id: int, datos: CambioRutaMatriculaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.cambiar_ruta_matricula(matricula_id, datos.ruta_id)

    @router.get(
        "/rutas",
        dependencies=[Depends(exigir_alguno("personas.administrar", "transporte.operar", "rutas.administrar"))],
    )
    async def rutas(servicio=Depends(obtener_servicio)):
        return servicio.listar_rutas_activas()

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
