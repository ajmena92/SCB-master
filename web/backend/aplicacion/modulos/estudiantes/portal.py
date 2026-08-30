"""Rutas del portal estudiantil y su autenticación."""

from __future__ import annotations

import secrets
from collections.abc import Callable, Iterator
from datetime import date

from fastapi import APIRouter, Cookie, Depends, Header, HTTPException, Response, status

from aplicacion.modulos.identidad.seguridad import hash_contrasena, verificar_contrasena
from aplicacion.modulos.identidad.servicio import (
    AutenticacionBloqueada,
    AutenticacionFallida,
    ServicioIdentidad,
)
from aplicacion.nucleo.tiempo import fecha_local
from aplicacion.modulos.comedor.servicio import ServicioComedor

from .beneficios import normalizar_beneficio_transporte
from .esquemas import AccesoEstudiante, CambioPinEstudiante


def crear_enrutador_portal(
    obtener_repositorio: Callable[[], Iterator],
    obtener_identidad: Callable[[], ServicioIdentidad] | None = None,
    obtener_identidad_estudiante: Callable[[], ServicioIdentidad] | None = None,
    obtener_menu: Callable[[], Iterator] | None = None,
    obtener_asistencia: Callable[[], Iterator] | None = None,
    obtener_comedor: Callable[[], Iterator] | None = None,
    cookies_seguras: bool = True,
    duracion_sesion_estudiante: int = 31536000,
    exigir_csrf: Callable[..., object] | None = None,
    obtener_fecha_local: Callable[[], date] | None = None,
) -> APIRouter:
    """Construye las rutas autenticadas que consume el portal estudiantil."""
    enrutador = APIRouter()
    identidad_portal = obtener_identidad_estudiante or obtener_identidad
    fecha_hoy = obtener_fecha_local or (lambda: fecha_local("America/Costa_Rica"))

    def estudiante_actual(repo, identidad, id_sesion, secreto):
        if identidad is None or not id_sesion or not secreto:
            raise HTTPException(401, "La sesión no es válida")
        try:
            sesion = identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(401, str(exc)) from exc
        credencial = repo.buscar_credencial_por_id(sesion.id_usuario)
        if not credencial or not credencial.get("activo"):
            raise HTTPException(401, "El estudiante no está disponible")
        perfil = normalizar_beneficio_transporte(credencial)
        return {
            "idEstudiante": credencial["id_estudiante"],
            "carne": credencial["carne"],
            "nombre": credencial["nombre"],
            "primerApellido": credencial.get("primer_apellido"),
            "segundoApellido": credencial.get("segundo_apellido"),
            "cedula": credencial.get("cedula"),
            "seccion": credencial.get("seccion"),
            "turno": credencial.get("turno"),
            "idRuta": perfil.get("id_ruta"),
            "rutaCodigo": perfil.get("ruta_codigo"),
            "rutaDescripcion": perfil.get("ruta_descripcion"),
            "rutaColor": perfil.get("ruta_color"),
            "tieneBeneficioTransporte": perfil["tiene_beneficio_transporte"],
            "beneficioTransporte": perfil["beneficio_transporte"],
            "idEstadoComedor": int(credencial.get("id_estado_comedor", 2)),
            "beneficioComedor": credencial.get("beneficio_comedor", "No beneficiario"),
            "debeCambiarPin": bool(credencial.get("debe_cambiar_pin")),
            "tieneFoto": bool(credencial.get("tiene_foto")),
            "barcode": credencial["carne"],
        }

    @enrutador.get("/menu")
    def menu(
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(identidad_portal),
        id_sesion: str | None = Cookie(default=None),
        secreto: str | None = Cookie(default=None),
        menu_repo=Depends(obtener_menu) if obtener_menu else None,
    ):
        estudiante_actual(repo, identidad, id_sesion, secreto)
        if menu_repo is None:
            raise HTTPException(503, "El menú no está configurado")
        return {"menu": menu_repo.listar()}

    @enrutador.get("/carnet")
    def carnet(
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(identidad_portal),
        id_sesion: str | None = Cookie(default=None),
        secreto: str | None = Cookie(default=None),
    ):
        return estudiante_actual(repo, identidad, id_sesion, secreto)

    @enrutador.get("/carnet/foto")
    def foto_carnet(
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(identidad_portal),
        id_sesion: str | None = Cookie(default=None),
        secreto: str | None = Cookie(default=None),
    ):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        foto = repo.obtener_foto(int(estudiante["idEstudiante"]))
        if foto is None:
            return Response(status_code=404)
        return Response(content=foto[0], media_type=foto[1])

    @enrutador.get("/asistencia/hoy")
    def asistencia_hoy(
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(identidad_portal),
        id_sesion: str | None = Cookie(default=None),
        secreto: str | None = Cookie(default=None),
        asistencia_repo=Depends(obtener_asistencia) if obtener_asistencia else None,
    ):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        if asistencia_repo is None:
            raise HTTPException(503, "La asistencia no está configurada")
        marcas = [
            marca
            for marca in asistencia_repo.listar(fecha_hoy())
            if int(marca["id_estudiante"]) == int(estudiante["idEstudiante"])
        ]
        return {"marcas": marcas}

    @enrutador.post("/asistencia/{accion}")
    def registrar_asistencia(
        accion: str,
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(identidad_portal),
        id_sesion: str | None = Cookie(default=None),
        secreto: str | None = Cookie(default=None),
        asistencia_repo=Depends(obtener_asistencia) if obtener_asistencia else None,
        comedor_repo=Depends(obtener_comedor) if obtener_comedor else None,
        __=Depends(exigir_csrf) if exigir_csrf else None,
    ):
        estudiante = estudiante_actual(repo, identidad, id_sesion, secreto)
        if asistencia_repo is None:
            raise HTTPException(503, "La asistencia no está configurada")
        if comedor_repo is None:
            raise HTTPException(503, "El comedor no está configurado")
        estados = {"confirm": "presente", "decline": "ausente"}
        estado = estados.get(accion)
        if estado is None:
            raise HTTPException(400, "Acción de asistencia no válida")
        comedor = ServicioComedor(comedor_repo)
        fecha = fecha_hoy()
        id_estudiante = int(estudiante["idEstudiante"])
        if estado == "presente":
            try:
                reserva = comedor.reservar_estudiante(id_estudiante, fecha, None)
            except ValueError as exc:
                raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc
            if reserva.estado != "reservada":
                raise HTTPException(
                    status.HTTP_409_CONFLICT,
                    "Debés reservar tu ingreso al comedor antes de confirmar asistencia",
                )
        else:
            try:
                comedor.cancelar_estudiante(id_estudiante, fecha, None)
            except ValueError as exc:
                if str(exc) != "La reserva no existe":
                    raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc
        try:
            return asistencia_repo.registrar(
                {
                    "id_estudiante": id_estudiante,
                    "fecha": fecha,
                    "estado": estado,
                    "observacion": None,
                },
                id_estudiante,
                "WEB",
            )
        except Exception:
            if estado == "presente":
                try:
                    comedor.cancelar_estudiante(id_estudiante, fecha, None)
                except ValueError:
                    pass
            raise

    @enrutador.post("/autenticacion")
    def autenticar(
        datos: AccesoEstudiante,
        respuesta: Response,
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(obtener_identidad_estudiante)
        if obtener_identidad_estudiante
        else None,
    ):
        if identidad is None:
            raise HTTPException(503, "La identidad no está configurada")
        try:
            identidad.verificar_bloqueo(datos.carne)
        except AutenticacionBloqueada as exc:
            raise HTTPException(status.HTTP_429_TOO_MANY_REQUESTS, str(exc)) from exc
        credencial = repo.buscar_credencial(datos.carne)
        if (
            not credencial
            or not credencial.get("activo")
            or not credencial.get("hash_contrasena")
            or not verificar_contrasena(datos.pin, str(credencial["hash_contrasena"]))
        ):
            identidad.registrar_fallo_autenticacion(datos.carne)
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "Las credenciales no son válidas")
        identidad.registrar_exito_autenticacion(datos.carne)
        resultado = identidad.crear_sesion(
            int(credencial["id_estudiante"]), str(credencial["carne"])
        )
        token_csrf = secrets.token_urlsafe(32)
        identidad.establecer_csrf(resultado.id_sesion, token_csrf)
        respuesta.set_cookie(
            "id_sesion",
            resultado.id_sesion,
            max_age=duracion_sesion_estudiante,
            httponly=True,
            secure=cookies_seguras,
            samesite="strict",
        )
        respuesta.set_cookie(
            "secreto_sesion",
            resultado.secreto_sesion,
            max_age=duracion_sesion_estudiante,
            httponly=True,
            secure=cookies_seguras,
            samesite="strict",
        )
        respuesta.set_cookie(
            "csrf_token",
            token_csrf,
            max_age=duracion_sesion_estudiante,
            httponly=False,
            secure=cookies_seguras,
            samesite="strict",
        )
        return {
            "idEstudiante": resultado.id_usuario,
            "debeCambiarPin": bool(credencial.get("debe_cambiar_pin")),
        }

    @enrutador.post("/pin")
    def cambiar_pin(
        datos: CambioPinEstudiante,
        repo=Depends(obtener_repositorio),
        identidad: ServicioIdentidad | None = Depends(obtener_identidad_estudiante)
        if obtener_identidad_estudiante
        else None,
        id_sesion: str | None = Cookie(default=None),
        secreto: str | None = Cookie(default=None),
        token_csrf: str | None = Header(default=None, alias="X-CSRF-Token"),
        csrf_cookie: str | None = Cookie(default=None, alias="csrf_token"),
    ):
        if identidad is None or not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        try:
            sesion = identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        if (
            not token_csrf
            or token_csrf != csrf_cookie
            or not identidad.validar_csrf(sesion, token_csrf)
        ):
            raise HTTPException(status.HTTP_403_FORBIDDEN, "El token CSRF no es válido")
        credencial = repo.buscar_credencial_por_id(sesion.id_usuario)
        if not credencial or not verificar_contrasena(
            datos.pin_actual, str(credencial.get("hash_contrasena", ""))
        ):
            raise HTTPException(status.HTTP_400_BAD_REQUEST, "El PIN actual no es válido")
        repo.actualizar_pin(sesion.id_usuario, hash_contrasena(datos.pin_nuevo))
        return {"estado": "actualizado"}

    return enrutador
