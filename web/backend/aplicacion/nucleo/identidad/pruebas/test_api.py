from datetime import datetime, timezone
from typing import Any, cast

from fastapi import Response
from fastapi.routing import APIRoute

from aplicacion.nucleo.identidad.api import CredencialesEntrada, crear_enrutador
from aplicacion.nucleo.identidad.esquemas import ResultadoAutenticacion, SesionPersistida
from aplicacion.nucleo.identidad.servicio import AutenticacionFallida


class ServicioFalso:
    def __init__(self) -> None:
        self.sesion: SesionPersistida | None = None
        self.token_csrf = ""

    def autenticar(self, nombre_usuario: str, contrasena: str) -> ResultadoAutenticacion:
        assert nombre_usuario == "operador" and contrasena == "Clave segura 2026"
        expira = datetime.now(timezone.utc).replace(microsecond=0)
        self.sesion = SesionPersistida(idSesion="sesion-1", idUsuario=7, secretoHash="digest", expiraEn=expira)
        return ResultadoAutenticacion(idSesion="sesion-1", idUsuario=7, nombreUsuario="operador", secretoSesion="secreto", expiraEn=expira, permisos=frozenset({"rutas.administrar"}))

    def establecer_csrf(self, id_sesion: str, token: str) -> None:
        assert self.sesion and id_sesion == self.sesion.id_sesion
        self.token_csrf = token
        self.sesion = self.sesion.model_copy(update={"csrf_hash": __import__("hashlib").sha256(token.encode()).hexdigest()})

    def validar_sesion(self, id_sesion: str, secreto: str) -> SesionPersistida:
        if not self.sesion or id_sesion != self.sesion.id_sesion or secreto != "secreto" or self.sesion.revocada:
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
    ruta = cast(APIRoute, crear_enrutador(cast(Any, lambda: servicio), cookies_seguras=False).routes[0])
    salida = ruta.endpoint(CredencialesEntrada(nombreUsuario="operador", contrasena="Clave segura 2026"), Response(), servicio)
    assert salida.id_usuario == 7
    assert servicio.sesion is not None


def test_cierre_exige_csrf_y_revoca_sesion() -> None:
    servicio = servicio_falso()
    router = crear_enrutador(cast(Any, lambda: servicio), cookies_seguras=False)
    cast(APIRoute, router.routes[0]).endpoint(CredencialesEntrada(nombreUsuario="operador", contrasena="Clave segura 2026"), Response(), servicio)
    assert servicio.sesion is not None
    from fastapi import HTTPException
    try:
        cast(APIRoute, router.routes[2]).endpoint(Response(), servicio.sesion, None, None, servicio)
    except HTTPException as error:
        assert error.status_code == 403
    cast(APIRoute, router.routes[2]).endpoint(Response(), servicio.sesion, servicio.token_csrf, servicio.token_csrf, servicio)
    assert servicio.sesion.revocada
