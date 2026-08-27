"""Adaptador HTTP canónico para identidad.

Este enrutador no conoce el servidor histórico ni sus dependencias. La sesión se
transporta exclusivamente mediante cookies opacas y las operaciones mutantes
exigen un token CSRF independiente.
"""

from __future__ import annotations

import secrets
from collections.abc import Callable
from datetime import datetime
from typing import Annotated

from fastapi import APIRouter, Cookie, Depends, Header, HTTPException, Response, status
from pydantic import BaseModel, ConfigDict, Field

from .esquemas import SesionPersistida
from .servicio import AutenticacionFallida, ServicioIdentidad

NOMBRE_COOKIE_ID_SESION = "id_sesion"
NOMBRE_COOKIE_SECRETO = "secreto_sesion"
NOMBRE_COOKIE_CSRF = "csrf_token"


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


class SesionActualSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_usuario: int = Field(alias="idUsuario")
    expira_en: datetime = Field(alias="expiraEn")
    tipo: str = "admin"
    usuario: dict[str, object] = Field(default_factory=dict)


def crear_enrutador(
    obtener_servicio: Callable[[], ServicioIdentidad],
    *,
    obtener_servicio_estudiante: Callable[[], ServicioIdentidad] | None = None,
    obtener_repositorio_estudiante: Callable[[], object] | None = None,
    cookies_seguras: bool = True,
) -> APIRouter:
    """Construye las rutas versionables de autenticación y sesión."""
    enrutador = APIRouter(prefix="", tags=["identidad"])

    def servicio() -> ServicioIdentidad:
        return obtener_servicio()

    def servicio_estudiante() -> ServicioIdentidad | None:
        return obtener_servicio_estudiante() if obtener_servicio_estudiante else None

    def sesion_actual(
        id_sesion: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_ID_SESION)] = None,
        secreto: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_SECRETO)] = None,
        caso_uso: ServicioIdentidad = Depends(servicio),
    ) -> SesionPersistida:
        if not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        try:
            return caso_uso.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc

    @enrutador.post(
        "/autenticacion", response_model=AutenticacionSalida, response_model_by_alias=True
    )
    def autenticar(
        datos: CredencialesEntrada,
        respuesta: Response,
        caso_uso: ServicioIdentidad = Depends(servicio),
    ) -> AutenticacionSalida:
        try:
            resultado = caso_uso.autenticar(datos.nombre_usuario, datos.contrasena)
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        token_csrf = secrets.token_urlsafe(32)
        caso_uso.establecer_csrf(resultado.id_sesion, token_csrf)
        id_sesion = resultado.id_sesion
        respuesta.set_cookie(
            NOMBRE_COOKIE_ID_SESION,
            id_sesion,
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

    @enrutador.get("/sesion", response_model=SesionActualSalida, response_model_by_alias=True)
    def consultar_sesion(
        id_sesion: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_ID_SESION)] = None,
        secreto: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_SECRETO)] = None,
        admin: ServicioIdentidad = Depends(servicio),
        estudiante: ServicioIdentidad | None = Depends(servicio_estudiante),
    ) -> SesionActualSalida:
        if not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        try:
            sesion = admin.validar_sesion(id_sesion, secreto)
            permisos = admin.permisos_de_sesion(sesion)
            es_administrador = "administracion.usuarios.editar" in permisos
            return SesionActualSalida(
                idUsuario=sesion.id_usuario,
                expiraEn=sesion.expira_en,
                tipo="admin",
                usuario={
                    "idUsuario": sesion.id_usuario,
                    "permisos": list(permisos),
                    "roles": ["Administrador"] if es_administrador else [],
                },
            )
        except AutenticacionFallida:
            if estudiante is None:
                raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
            try:
                sesion = estudiante.validar_sesion(id_sesion, secreto)
            except AutenticacionFallida as exc:
                raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
            perfil: dict[str, object] = {"idEstudiante": sesion.id_usuario}
            if obtener_repositorio_estudiante is not None:
                repositorio = obtener_repositorio_estudiante()
                buscar = getattr(repositorio, "buscar_por_id", None)
                if callable(buscar):
                    datos = buscar(sesion.id_usuario) or {}
                    perfil.update(
                        {
                            "carne": datos.get("carne"),
                            "nombre": datos.get("nombre"),
                            "nombreCompleto": " ".join(
                                str(datos.get(c) or "")
                                for c in ("nombre", "primer_apellido", "segundo_apellido")
                            ).strip(),
                            "tieneFoto": bool(datos.get("tiene_foto", False)),
                            "debeCambiarPin": bool(datos.get("debe_cambiar_pin", False)),
                        }
                    )
            return SesionActualSalida(
                idUsuario=sesion.id_usuario,
                expiraEn=sesion.expira_en,
                tipo="estudiante",
                usuario=perfil,
            )

    @enrutador.post("/sesion/cerrar", status_code=status.HTTP_204_NO_CONTENT)
    def cerrar_sesion(
        respuesta: Response,
        id_sesion: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_ID_SESION)] = None,
        secreto: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_SECRETO)] = None,
        token_csrf: Annotated[str | None, Header(alias="X-CSRF-Token")] = None,
        csrf_cookie: Annotated[str | None, Cookie(alias=NOMBRE_COOKIE_CSRF)] = None,
        caso_uso: ServicioIdentidad = Depends(servicio),
        caso_estudiante: ServicioIdentidad | None = Depends(servicio_estudiante),
    ) -> None:
        if not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        servicio_sesion = caso_uso
        try:
            sesion = caso_uso.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida:
            if caso_estudiante is None:
                raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
            try:
                sesion = caso_estudiante.validar_sesion(id_sesion, secreto)
                servicio_sesion = caso_estudiante
            except AutenticacionFallida as exc:
                raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        if (
            not token_csrf
            or token_csrf != csrf_cookie
            or not servicio_sesion.validar_csrf(sesion, token_csrf)
        ):
            raise HTTPException(status.HTTP_403_FORBIDDEN, "El token CSRF no es válido")
        servicio_sesion.cerrar_sesion(sesion.id_sesion)
        for nombre in (NOMBRE_COOKIE_ID_SESION, NOMBRE_COOKIE_SECRETO, NOMBRE_COOKIE_CSRF):
            respuesta.delete_cookie(nombre)

    return enrutador
