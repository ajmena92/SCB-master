"""Dependencias HTTP compartidas para sesión, autorización y CSRF."""

from __future__ import annotations

import ipaddress
from collections.abc import Callable

from fastapi import Cookie, Depends, Header, HTTPException, Request

from aplicacion.modulos.identidad.esquemas import SesionPersistida
from aplicacion.modulos.identidad.servicio import (
    AutenticacionFallida,
    ServicioIdentidad,
    ServicioPermisos,
)
from config import Settings


def ip_cliente(request: Request) -> str:
    """Obtiene la IP del cliente respetando únicamente proxies confiables."""

    settings = Settings.from_environment()
    cliente = request.client.host if request.client else ""
    try:
        confiable = any(
            ipaddress.ip_address(cliente) in ipaddress.ip_network(red)
            for red in settings.trusted_proxy_cidrs
        )
    except ValueError:
        confiable = False
    reenviado = request.headers.get("x-forwarded-for", "").split(",", 1)[0].strip()
    return (reenviado if confiable and reenviado else cliente or "WEB")[:64]


def crear_dependencias_seguridad(
    obtener_identidad: Callable[[], ServicioIdentidad],
) -> dict[str, Callable]:
    """Crea dependencias FastAPI cerradas sobre el servicio de identidad."""

    def sesion_actual(
        identidad: ServicioIdentidad = Depends(obtener_identidad),
        id_sesion: str | None = Cookie(default=None, alias="id_sesion"),
        secreto: str | None = Cookie(default=None, alias="secreto_sesion"),
    ) -> tuple[ServicioIdentidad, SesionPersistida]:
        if not id_sesion or not secreto:
            raise HTTPException(401, "La sesión no es válida")
        try:
            return identidad, identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(401, str(exc)) from exc

    def exigir_permiso(permiso: str) -> Callable:
        def dependencia(
            datos: tuple[ServicioIdentidad, SesionPersistida] = Depends(sesion_actual),
        ) -> dict[str, object]:
            identidad, sesion = datos
            try:
                permisos = identidad.permisos_de_sesion(sesion)
            except AutenticacionFallida as exc:
                raise HTTPException(401, str(exc)) from exc
            if not ServicioPermisos.tiene(permisos, permiso):
                raise HTTPException(403, "No tiene permiso para esta operación")
            return {"idUsuario": sesion.id_usuario, "permisos": permisos}

        return dependencia

    def exigir_csrf(
        datos: tuple[ServicioIdentidad, SesionPersistida] = Depends(sesion_actual),
        token: str | None = Header(default=None, alias="X-CSRF-Token"),
        cookie: str | None = Cookie(default=None, alias="csrf_token"),
    ) -> dict[str, object]:
        identidad, sesion = datos
        if not token or token != cookie or not identidad.validar_csrf(sesion, token):
            raise HTTPException(403, "El token CSRF no es válido")
        return {"idUsuario": sesion.id_usuario}

    return {
        "sesion_actual": sesion_actual,
        "exigir_permiso": exigir_permiso,
        "exigir_csrf": exigir_csrf,
    }
