"""Adaptador HTTP de sesiones."""

from datetime import datetime, timedelta, timezone

from fastapi import APIRouter, Depends, Request, Response

from aplicacion.casos_identidad import ServicioIdentidad
from aplicacion.esquemas import (
    AdministracionEntrada,
    CambioContrasenaAdministrativaEntrada,
    CambioPinEntrada,
    PortalEntrada,
    SesionSalida,
)

NOMBRE_COOKIE_SESION = "scb_sesion"
NOMBRE_COOKIE_CSRF = "csrf_token"
RUTA_COOKIE_API = "/api/v1"


def _segundos_restantes(expira_en: datetime) -> int:
    vencimiento = expira_en.replace(tzinfo=timezone.utc) if expira_en.tzinfo is None else expira_en
    return max(1, int((vencimiento - datetime.now(timezone.utc)).total_seconds()))


def _emitir_cookies(
    response: Response,
    *,
    token: str,
    csrf: str,
    expira_en: datetime,
    seguro: bool,
) -> None:
    vencimiento = (
        expira_en.replace(tzinfo=timezone.utc)
        if expira_en.tzinfo is None
        else expira_en.astimezone(timezone.utc)
    )
    max_age = _segundos_restantes(vencimiento)
    response.set_cookie(
        NOMBRE_COOKIE_SESION,
        token,
        httponly=True,
        secure=seguro,
        samesite="lax",
        path=RUTA_COOKIE_API,
        max_age=max_age,
        expires=vencimiento,
    )
    response.set_cookie(
        NOMBRE_COOKIE_CSRF,
        csrf,
        httponly=False,
        secure=seguro,
        samesite="lax",
        # La SPA debe poder leer este valor para enviarlo como cabecera. La
        # cookie de sesión permanece restringida a /api/v1 y es HttpOnly.
        path="/",
        max_age=max_age,
        expires=vencimiento,
    )


def _limpiar_cookies(response: Response, *, seguro: bool) -> None:
    for nombre, httponly, ruta in (
        (NOMBRE_COOKIE_SESION, True, RUTA_COOKIE_API),
        (NOMBRE_COOKIE_CSRF, False, "/"),
    ):
        response.delete_cookie(
            nombre,
            httponly=httponly,
            secure=seguro,
            samesite="lax",
            path=ruta,
        )


def crear_router(
    obtener_servicio,
    actual,
    *,
    csrf_secret: str,
    cookie_secure: bool,
    csrf_anonymous_ttl_seconds: int = 600,
) -> APIRouter:
    router = APIRouter()

    @router.get("/autenticacion/csrf", status_code=204)
    async def csrf(request: Request, response: Response):
        from aplicacion.seguridad import csrf_anonimo, csrf_sesion

        token = request.cookies.get(NOMBRE_COOKIE_SESION)
        ahora = datetime.now(timezone.utc)
        valor = csrf_sesion(token, csrf_secret) if token else csrf_anonimo(
            csrf_secret, ttl_seconds=csrf_anonymous_ttl_seconds, ahora=ahora
        )
        opciones_expiracion = (
            {}
            if token
            else {
                "max_age": csrf_anonymous_ttl_seconds,
                "expires": ahora + timedelta(seconds=csrf_anonymous_ttl_seconds),
            }
        )
        response.set_cookie(
            NOMBRE_COOKIE_CSRF,
            valor,
            httponly=False,
            secure=cookie_secure,
            samesite="lax",
            path="/",
            **opciones_expiracion,
        )

    @router.post("/autenticacion/portal", response_model=SesionSalida, response_model_by_alias=True)
    async def portal(
        datos: PortalEntrada,
        response: Response,
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        token, salida = servicio.autenticar_portal(datos)
        from aplicacion.seguridad import csrf_sesion
        _emitir_cookies(
            response,
            token=token,
            csrf=csrf_sesion(token, csrf_secret),
            expira_en=salida.expira_en,
            seguro=cookie_secure,
        )
        return salida

    @router.post(
        "/autenticacion/administracion", response_model=SesionSalida, response_model_by_alias=True
    )
    async def administracion(
        datos: AdministracionEntrada, response: Response, servicio: ServicioIdentidad = Depends(obtener_servicio)
    ):
        token, salida = servicio.autenticar_administracion(datos)
        from aplicacion.seguridad import csrf_sesion
        _emitir_cookies(
            response,
            token=token,
            csrf=csrf_sesion(token, csrf_secret),
            expira_en=salida.expira_en,
            seguro=cookie_secure,
        )
        return salida

    @router.get("/sesion")
    async def consultar(identidad: dict = Depends(actual)):
        if identidad["tipo"] == "portal":
            persona = identidad["persona"]
            return {
                "tipo": "portal",
                "personaId": persona.id,
                "cedula": persona.cedula,
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
        response: Response,
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        salida = servicio.cambiar_pin(identidad, datos)
        _limpiar_cookies(response, seguro=cookie_secure)
        return salida

    @router.post("/autenticacion/logout", status_code=204)
    async def logout(
        response: Response,
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        servicio.cerrar_sesion(identidad["_token"])
        _limpiar_cookies(response, seguro=cookie_secure)

    @router.post("/autenticacion/renovar", status_code=204)
    async def renovar(
        response: Response,
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        from aplicacion.seguridad import csrf_sesion
        token, expira_en = servicio.renovar_sesion(identidad["_token"])
        _emitir_cookies(
            response,
            token=token,
            csrf=csrf_sesion(token, csrf_secret),
            expira_en=expira_en,
            seguro=cookie_secure,
        )

    @router.post("/autenticacion/administracion/contrasena")
    async def cambiar_contrasena_administrativa(
        datos: CambioContrasenaAdministrativaEntrada,
        response: Response,
        identidad=Depends(actual),
        servicio: ServicioIdentidad = Depends(obtener_servicio),
    ):
        salida = servicio.cambiar_contrasena_administrativa(identidad, datos)
        _limpiar_cookies(response, seguro=cookie_secure)
        return salida

    return router
