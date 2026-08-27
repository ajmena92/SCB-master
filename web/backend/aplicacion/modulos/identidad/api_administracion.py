"""API HTTP de autenticación administrativa."""

from __future__ import annotations

import secrets
from collections.abc import Callable
from datetime import datetime

from fastapi import APIRouter, Depends, HTTPException, Response, status
from pydantic import BaseModel, ConfigDict, Field

from .constantes import NOMBRE_COOKIE_CSRF, NOMBRE_COOKIE_ID_SESION, NOMBRE_COOKIE_SECRETO
from .servicio import AutenticacionFallida, ServicioIdentidad


class CredencialesEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    nombre_usuario: str = Field(alias="nombreUsuario", min_length=1)
    contrasena: str = Field(min_length=1)


class AutenticacionSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_usuario: int = Field(alias="idUsuario")
    nombre_usuario: str = Field(alias="nombreUsuario")
    expira_en: datetime = Field(alias="expiraEn")
    permisos: frozenset[str]
    csrf_token: str = Field(alias="csrfToken")


def crear_enrutador_administracion_identidad(
    obtener_servicio: Callable[[], ServicioIdentidad],
    *,
    cookies_seguras: bool = True,
) -> APIRouter:
    """Construye exclusivamente el inicio de sesión administrativo."""

    enrutador = APIRouter(prefix="", tags=["identidad-administrativa"])

    @enrutador.post(
        "/autenticacion", response_model=AutenticacionSalida, response_model_by_alias=True
    )
    def autenticar(
        datos: CredencialesEntrada,
        respuesta: Response,
        caso_uso: ServicioIdentidad = Depends(obtener_servicio),
    ) -> AutenticacionSalida:
        try:
            resultado = caso_uso.autenticar(datos.nombre_usuario, datos.contrasena)
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        token_csrf = secrets.token_urlsafe(32)
        caso_uso.establecer_csrf(resultado.id_sesion, token_csrf)
        respuesta.set_cookie(
            NOMBRE_COOKIE_ID_SESION,
            resultado.id_sesion,
            httponly=True,
            secure=cookies_seguras,
            samesite="strict",
        )
        respuesta.set_cookie(
            NOMBRE_COOKIE_SECRETO,
            resultado.secreto_sesion,
            httponly=True,
            secure=cookies_seguras,
            samesite="strict",
        )
        respuesta.set_cookie(
            NOMBRE_COOKIE_CSRF,
            token_csrf,
            httponly=False,
            secure=cookies_seguras,
            samesite="strict",
        )
        return AutenticacionSalida(
            idUsuario=resultado.id_usuario,
            nombreUsuario=resultado.nombre_usuario,
            expiraEn=resultado.expira_en,
            permisos=resultado.permisos,
            csrfToken=token_csrf,
        )

    return enrutador
