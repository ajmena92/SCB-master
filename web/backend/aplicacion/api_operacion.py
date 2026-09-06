"""Adaptadores HTTP para comedor y tiquetes.

La captura de transporte queda preservada para la etapa 2, sin endpoint público.
"""

from datetime import date

from fastapi import APIRouter, Depends, HTTPException, Response

from aplicacion.esquemas import (
    AutorizacionEntrada,
    CancelacionReservaEntrada,
    ConfiguracionInstitucionalEntrada,
    HorarioReservaEntrada,
    IngresoEntrada,
    ReservaEntrada,
    TarifaEntrada,
    VentaEntrada,
)


def crear_router(obtener_servicio, portal_operativo, exigir_permiso, exigir_alguno) -> APIRouter:
    router = APIRouter()

    @router.get(
        "/tiquetes/tarifas",
        dependencies=[Depends(exigir_alguno("tiquetes.operar", "tarifas.administrar"))],
    )
    def tarifas(servicio=Depends(obtener_servicio)):
        return servicio.listar_tarifas()

    @router.post(
        "/tiquetes/tarifas",
        status_code=201,
        dependencies=[Depends(exigir_permiso("tarifas.administrar"))],
    )
    def tarifa(datos: TarifaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_tarifa(datos)

    @router.put(
        "/tiquetes/tarifas/{tarifa_id}",
        dependencies=[Depends(exigir_permiso("tarifas.administrar"))],
    )
    def actualizar_tarifa(tarifa_id: int, datos: TarifaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.actualizar_tarifa(tarifa_id, datos)

    @router.get("/tiquetes/personas", dependencies=[Depends(exigir_permiso("tiquetes.operar"))])
    def buscar_personas_venta(buscar: str, servicio=Depends(obtener_servicio)):
        return servicio.buscar_personas_venta(buscar)

    @router.get(
        "/tiquetes/personas/{persona_id}/foto",
        dependencies=[Depends(exigir_permiso("tiquetes.operar"))],
    )
    def foto_persona_venta(persona_id: int, servicio=Depends(obtener_servicio)):
        foto = servicio.foto_persona_venta(persona_id)
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto.contenido, media_type=foto.tipo_contenido)

    @router.get(
        "/comedor/personas/{persona_id}/foto",
        dependencies=[Depends(exigir_permiso("comedor.operar"))],
    )
    def foto_persona_comedor(persona_id: int, servicio=Depends(obtener_servicio)):
        foto = servicio.foto_persona_comedor(persona_id)
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto.contenido, media_type=foto.tipo_contenido)

    @router.get(
        "/parametros-operativos/horarios-reserva",
        dependencies=[Depends(exigir_permiso("tarifas.administrar"))],
    )
    def horarios_reserva(servicio=Depends(obtener_servicio)):
        return servicio.listar_horarios_reserva()

    @router.put(
        "/parametros-operativos/horarios-reserva/{turno}",
        dependencies=[Depends(exigir_permiso("tarifas.administrar"))],
    )
    def horario_reserva(
        turno: str, datos: HorarioReservaEntrada, servicio=Depends(obtener_servicio)
    ):
        if turno != datos.turno:
            raise HTTPException(422, "El turno de la ruta no coincide con el cuerpo")
        return servicio.actualizar_horario_reserva(datos)

    @router.get(
        "/parametros-operativos/institucion",
        dependencies=[Depends(exigir_permiso("tarifas.administrar"))],
    )
    def institucion(servicio=Depends(obtener_servicio)):
        return servicio.configuracion_institucional()

    @router.put(
        "/parametros-operativos/institucion",
        dependencies=[Depends(exigir_permiso("tarifas.administrar"))],
    )
    def actualizar_institucion(
        datos: ConfiguracionInstitucionalEntrada, servicio=Depends(obtener_servicio)
    ):
        return servicio.actualizar_configuracion_institucional(datos)

    @router.post("/tiquetes/ventas", status_code=201)
    def venta(
        datos: VentaEntrada,
        identidad=Depends(exigir_permiso("tiquetes.operar")),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.vender(datos, identidad["cuenta"].id)

    @router.post("/comedor/reservas", status_code=201)
    def reserva(
        datos: ReservaEntrada,
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.reservar(datos, identidad)

    @router.delete("/comedor/reservas", status_code=204)
    def cancelar(
        datos: CancelacionReservaEntrada,
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        servicio.cancelar(datos, identidad)

    @router.post("/comedor/autorizaciones", status_code=201)
    def autorizar(
        datos: AutorizacionEntrada,
        identidad=Depends(exigir_permiso("comedor.operar")),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.autorizar(datos, identidad["cuenta"].id)

    @router.post("/comedor/operacion", status_code=201)
    def ingresar(
        datos: IngresoEntrada,
        response: Response,
        identidad=Depends(exigir_permiso("comedor.operar")),
        servicio=Depends(obtener_servicio),
    ):
        resultado, estado_http = servicio.capturar_ingreso(datos, identidad["cuenta"].id)
        response.status_code = estado_http
        return resultado

    @router.get("/comedor/operacion/estado")
    def estado_operacion(
        fecha: date,
        identidad=Depends(exigir_permiso("comedor.operar")),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.estado_captura(fecha)

    return router
