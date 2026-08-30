"""Adaptador HTTP neutral para el ciclo de vida de una sesión."""

from __future__ import annotations

from collections.abc import Callable
from datetime import datetime
from typing import Annotated

from fastapi import APIRouter, Cookie, Depends, Header, HTTPException, Response, status
from pydantic import BaseModel, ConfigDict, Field

from .constantes import NOMBRE_COOKIE_CSRF, NOMBRE_COOKIE_ID_SESION, NOMBRE_COOKIE_SECRETO
from .servicio import AutenticacionFallida, ServicioSesiones


class SesionActualSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_usuario: int = Field(alias="idUsuario")
    expira_en: datetime = Field(alias="expiraEn")
    tipo: str = "admin"
    usuario: dict[str, object] = Field(default_factory=dict)


def crear_enrutador_sesion(
    obtener_servicio: Callable[[], ServicioSesiones],
) -> APIRouter:
    """Construye solo el adaptador HTTP del servicio de sesiones."""

    enrutador = APIRouter(prefix="", tags=["sesion"])

    @enrutador.get(
        "/sesion",
        response_model=SesionActualSalida,
        response_model_by_alias=True,
        responses={status.HTTP_204_NO_CONTENT: {"description": "No existe una sesión activa"}},
    )
    def consultar_sesion(
        id_sesion: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_ID_SESION)] = None,
        secreto: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_SECRETO)] = None,
        servicio: ServicioSesiones = Depends(obtener_servicio),
    ) -> SesionActualSalida | Response:
        if not id_sesion or not secreto:
            return Response(status_code=status.HTTP_204_NO_CONTENT)
        try:
            resultado = servicio.validar(id_sesion, secreto)
        except AutenticacionFallida:
            respuesta = Response(status_code=status.HTTP_204_NO_CONTENT)
            for nombre in (NOMBRE_COOKIE_ID_SESION, NOMBRE_COOKIE_SECRETO, NOMBRE_COOKIE_CSRF):
                respuesta.delete_cookie(nombre)
            return respuesta
        return SesionActualSalida(
            idUsuario=resultado.sesion.id_usuario,
            expiraEn=resultado.sesion.expira_en,
            tipo=resultado.tipo,
            usuario=resultado.usuario,
        )

    @enrutador.post("/sesion/cerrar", status_code=status.HTTP_204_NO_CONTENT)
    def cerrar_sesion(
        respuesta: Response,
        id_sesion: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_ID_SESION)] = None,
        secreto: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_SECRETO)] = None,
        token_csrf: Annotated[str | None, Header(alias="X-CSRF-Token")] = None,
        csrf_cookie: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_CSRF)] = None,
        servicio: ServicioSesiones = Depends(obtener_servicio),
    ) -> None:
        if not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        try:
            servicio.cerrar(id_sesion, secreto, token_csrf or "", csrf_cookie or "")
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        except ValueError as exc:
            raise HTTPException(status.HTTP_403_FORBIDDEN, str(exc)) from exc
        for nombre in (NOMBRE_COOKIE_ID_SESION, NOMBRE_COOKIE_SECRETO, NOMBRE_COOKIE_CSRF):
            respuesta.delete_cookie(nombre)

    return enrutador
