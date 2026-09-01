"""API de mantenimiento administrativo con autoridad exclusiva del backend."""

from fastapi import APIRouter, Depends, HTTPException

from aplicacion.casos_administracion import ServicioAdministracion
from aplicacion.esquemas import (
    CuentaAdministrativaActualizacion,
    CuentaAdministrativaEntrada,
    VinculacionCuentaEntrada,
)


def crear_router(obtener_servicio, actual, administrador) -> APIRouter:
    router = APIRouter(prefix="/administracion")

    @router.get("/cuentas", dependencies=[Depends(administrador)])
    async def cuentas(servicio: ServicioAdministracion = Depends(obtener_servicio)):
        return servicio.listar()

    @router.post("/cuentas", status_code=201)
    async def crear_cuenta(
        datos: CuentaAdministrativaEntrada,
        identidad=Depends(administrador),
        servicio: ServicioAdministracion = Depends(obtener_servicio),
    ):
        return servicio.crear(datos)

    @router.put("/cuentas/{cuenta_id}")
    async def actualizar_cuenta(
        cuenta_id: int,
        datos: CuentaAdministrativaActualizacion,
        identidad=Depends(administrador),
        servicio: ServicioAdministracion = Depends(obtener_servicio),
    ):
        return servicio.actualizar(cuenta_id, datos, identidad["cuenta"].id)

    @router.post("/cuentas/{cuenta_id}/restablecer-contrasena")
    async def restablecer_contrasena(
        cuenta_id: int,
        identidad=Depends(administrador),
        servicio: ServicioAdministracion = Depends(obtener_servicio),
    ):
        return servicio.restablecer(cuenta_id, identidad["cuenta"].id)

    @router.get("/permisos", dependencies=[Depends(administrador)])
    async def permisos(servicio: ServicioAdministracion = Depends(obtener_servicio)):
        return servicio.listar_permisos()

    @router.get("/profesores-disponibles")
    async def profesores(
        identidad=Depends(actual),
        servicio: ServicioAdministracion = Depends(obtener_servicio),
    ):
        if identidad.get("tipo") != "administracion" or identidad.get("rol") != "administrador":
            raise HTTPException(403, "Solo un administrador puede consultar profesores")
        return servicio.profesores_disponibles()

    @router.post("/vinculacion-inicial")
    async def vinculacion(
        datos: VinculacionCuentaEntrada,
        identidad=Depends(actual),
        servicio: ServicioAdministracion = Depends(obtener_servicio),
    ):
        return servicio.vincular_inicial(identidad, datos)

    return router
