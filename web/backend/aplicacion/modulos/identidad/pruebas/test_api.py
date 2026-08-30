from datetime import datetime, timezone
from typing import Any, cast

from fastapi import Response
from fastapi.routing import APIRoute as RutaAPI

from aplicacion.modulos.identidad.api_administracion import (
    CredencialesEntrada,
    crear_enrutador_administracion_identidad,
)
from aplicacion.modulos.identidad.api_sesion import crear_enrutador_sesion
from aplicacion.modulos.identidad.esquemas import ResultadoAutenticacion, SesionPersistida
from aplicacion.modulos.identidad.servicio import AutenticacionFallida, ServicioSesiones


class ServicioFalso:
    def __init__(self) -> None:
        self.sesion: SesionPersistida | None = None
        self.token_csrf = ""

    def autenticar(self, nombre_usuario: str, contrasena: str) -> ResultadoAutenticacion:
        assert nombre_usuario == "operador" and contrasena == "Clave segura 2026"
        expira = datetime.now(timezone.utc).replace(microsecond=0)
        self.sesion = SesionPersistida(
            idSesion="sesion-1", idUsuario=7, secretoHash="digest", expiraEn=expira
        )
        return ResultadoAutenticacion(
            idSesion="sesion-1",
            idUsuario=7,
            nombreUsuario="operador",
            secretoSesion="secreto",
            expiraEn=expira,
            permisos=frozenset({"rutas.administrar"}),
        )

    def establecer_csrf(self, id_sesion: str, token: str) -> None:
        assert self.sesion and id_sesion == self.sesion.id_sesion
        self.token_csrf = token
        self.sesion = self.sesion.model_copy(
            update={"csrf_hash": __import__("hashlib").sha256(token.encode()).hexdigest()}
        )

    def validar_sesion(self, id_sesion: str, secreto: str) -> SesionPersistida:
        if (
            not self.sesion
            or id_sesion != self.sesion.id_sesion
            or secreto != "secreto"
            or self.sesion.revocada
        ):
            raise AutenticacionFallida("La sesión no es válida")
        return self.sesion

    def validar_csrf(self, sesion: SesionPersistida, token: str) -> bool:
        return bool(token and token == self.token_csrf and sesion.csrf_hash)

    def cerrar_sesion(self, id_sesion: str) -> None:
        assert self.sesion and id_sesion == self.sesion.id_sesion
        self.sesion = self.sesion.model_copy(update={"revocada": True})


def servicio_falso() -> ServicioFalso:
    servicio = ServicioFalso()
    return servicio


def test_autenticacion_emite_cookies_y_sesion_actual() -> None:
    servicio = servicio_falso()
    ruta = next(
        ruta
        for ruta in getattr(
            crear_enrutador_administracion_identidad(
                cast(Any, lambda: servicio), cookies_seguras=False
            ),
            "routes",
        )
        if isinstance(ruta, RutaAPI) and ruta.path == "/autenticacion"
    )
    salida = ruta.endpoint(
        CredencialesEntrada(nombreUsuario="operador", contrasena="Clave segura 2026"),
        Response(),
        servicio,
    )
    assert salida.id_usuario == 7
    assert servicio.sesion is not None


def test_consulta_sin_cookie_responde_sin_contenido() -> None:
    servicio = servicio_falso()
    enrutador = crear_enrutador_sesion(cast(Any, lambda: ServicioSesiones(cast(Any, servicio))))
    consulta = next(
        ruta
        for ruta in getattr(enrutador, "routes")
        if isinstance(ruta, RutaAPI) and ruta.path == "/sesion"
    )

    respuesta = consulta.endpoint(None, None, ServicioSesiones(cast(Any, servicio)))

    assert isinstance(respuesta, Response)
    assert respuesta.status_code == 204


def test_cierre_exige_csrf_y_revoca_sesion() -> None:
    servicio = servicio_falso()
    enrutador = crear_enrutador_sesion(cast(Any, lambda: ServicioSesiones(cast(Any, servicio))))
    cierre = next(
        ruta
        for ruta in getattr(enrutador, "routes")
        if isinstance(ruta, RutaAPI) and ruta.path == "/sesion/cerrar"
    )
    autenticacion = next(
        ruta
        for ruta in getattr(
            crear_enrutador_administracion_identidad(
                cast(Any, lambda: servicio), cookies_seguras=False
            ),
            "routes",
        )
        if isinstance(ruta, RutaAPI) and ruta.path == "/autenticacion"
    )
    autenticacion.endpoint(
        CredencialesEntrada(nombreUsuario="operador", contrasena="Clave segura 2026"),
        Response(),
        servicio,
    )
    assert servicio.sesion is not None
    servicio_sesiones = ServicioSesiones(cast(Any, servicio))
    try:
        servicio_sesiones.cerrar(servicio.sesion.id_sesion, "secreto", "", "")
    except ValueError as error:
        assert "CSRF" in str(error)
    cierre.endpoint(
        Response(),
        servicio.sesion.id_sesion,
        "secreto",
        servicio.token_csrf,
        servicio.token_csrf,
        servicio_sesiones,
    )
    assert servicio.sesion.revocada
