"""Adaptador HTTP de sesiones."""

from fastapi import APIRouter, Depends

from aplicacion.casos_identidad import ServicioIdentidad
from aplicacion.esquemas import (
    AdministracionEntrada,
    CambioContrasenaAdministrativaEntrada,
    CambioPinEntrada,
    PortalEntrada,
    SesionSalida,
)


def crear_router(obtener_servicio, actual) -> APIRouter:
    router = APIRouter()

    @router.post("/autenticacion/portal", response_model=SesionSalida, response_model_by_alias=True)
    async def portal(datos: PortalEntrada, servicio: ServicioIdentidad = Depends(obtener_servicio)):
        return servicio.autenticar_portal(datos)

    @router.post(
        "/autenticacion/administracion", response_model=SesionSalida, response_model_by_alias=True
    )
    async def administracion(
        datos: AdministracionEntrada, servicio: ServicioIdentidad = Depends(obtener_servicio)
    ):
        return servicio.autenticar_administracion(datos)

    @router.get("/sesion")
    async def consultar(identidad: dict = Depends(actual)):
        if identidad["tipo"] == "portal":
            persona = identidad["persona"]
            return {
                "tipo": "portal",
                "personaId": persona.id,
                "codigo": persona.codigo,
                "nombres": persona.nombres,
                "rol": persona.tipo,
                "cambioObligatorio": identidad.get("cambioObligatorio", False),
            }
        cuenta = identidad["cuenta"]
        persona = identidad.get("persona")
        return {
            "tipo": "administracion",
            "cuentaId": cuenta.id,
            "personaId": cuenta.persona_id,
            "usuario": cuenta.usuario,
            "nombres": persona.nombres if persona else None,
            "rol": cuenta.rol,
            "permisos": identidad["permisos"],
            "cambioContrasenaObligatorio": cuenta.cambio_contrasena_obligatorio,
            "vinculacionPendiente": cuenta.vinculacion_pendiente,
        }

    @router.post("/autenticacion/portal/pin")
    async def cambiar_pin(
        datos: CambioPinEntrada,
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        return servicio.cambiar_pin(identidad, datos)

    @router.post("/autenticacion/logout", status_code=204)
    async def logout(
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        servicio.cerrar_sesion(identidad["_token"])

    @router.post("/autenticacion/administracion/contrasena")
    async def cambiar_contrasena_administrativa(
        datos: CambioContrasenaAdministrativaEntrada,
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        return servicio.cambiar_contrasena_administrativa(identidad, datos)

    return router
