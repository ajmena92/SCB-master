"""Adaptadores HTTP para comedor, transporte y tiquetes."""

from fastapi import APIRouter, Depends

from aplicacion.esquemas import (
    AutorizacionEntrada,
    CancelacionReservaEntrada,
    IngresoEntrada,
    MarcaTransporteEntrada,
    ReservaEntrada,
    TarifaEntrada,
    VentaEntrada,
)


def crear_router(obtener_servicio, portal_operativo, administrativo, administrador) -> APIRouter:
    router = APIRouter()

    @router.get("/tiquetes/tarifas", dependencies=[Depends(administrativo)])
    async def tarifas(servicio=Depends(obtener_servicio)):
        return servicio.listar_tarifas()

    @router.post("/tiquetes/tarifas", status_code=201, dependencies=[Depends(administrador)])
    async def tarifa(datos: TarifaEntrada, servicio=Depends(obtener_servicio)):
        return servicio.crear_tarifa(datos)

    @router.post("/tiquetes/ventas", status_code=201)
    async def venta(
        datos: VentaEntrada, identidad=Depends(administrativo), servicio=Depends(obtener_servicio)
    ):
        return servicio.vender(datos, identidad["cuenta"].id)

    @router.post("/comedor/reservas", status_code=201)
    async def reserva(
        datos: ReservaEntrada,
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.reservar(datos, identidad)

    @router.delete("/comedor/reservas", status_code=204)
    async def cancelar(
        datos: CancelacionReservaEntrada,
        identidad=Depends(portal_operativo),
        servicio=Depends(obtener_servicio),
    ):
        servicio.cancelar(datos, identidad)

    @router.post("/comedor/autorizaciones", status_code=201)
    async def autorizar(
        datos: AutorizacionEntrada,
        identidad=Depends(administrativo),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.autorizar(datos, identidad["cuenta"].id)

    @router.post("/comedor/operacion", status_code=201)
    async def ingresar(
        datos: IngresoEntrada, identidad=Depends(administrativo), servicio=Depends(obtener_servicio)
    ):
        return servicio.ingresar(datos, identidad["cuenta"].id)

    @router.post("/transporte/marcas", status_code=201)
    async def marcar(
        datos: MarcaTransporteEntrada,
        identidad=Depends(administrativo),
        servicio=Depends(obtener_servicio),
    ):
        return servicio.marcar_transporte(datos, identidad["cuenta"].id)

    return router
