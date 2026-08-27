"""Casos de uso de autenticación y autorización de la plataforma web."""

from __future__ import annotations

import hashlib
import secrets
from collections.abc import Callable
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from threading import Lock
from typing import Protocol

from .esquemas import ResultadoAutenticacion, SesionPersistida
from .repositorio import RepositorioSesiones, RepositorioUsuarios
from .seguridad import (
    comparar_secreto_sesion,
    crear_secreto_sesion,
    hash_contrasena,
    hash_secreto_sesion,
    verificar_contrasena,
)

PERMISO_RUTAS_ADMINISTRAR = "rutas.administrar"


class AutenticacionFallida(ValueError):
    """Error uniforme para credenciales inexistentes, inválidas o inactivas."""


class AutenticacionBloqueada(AutenticacionFallida):
    """Indica que el identificador superó el límite temporal de intentos."""


@dataclass(frozen=True)
class PoliticaBloqueo:
    max_intentos: int
    minutos_bloqueo: int


class ControlIntentos(Protocol):
    def verificar(self, identificador: str, ahora: datetime, politica: PoliticaBloqueo) -> None: ...

    def registrar_fallo(
        self, identificador: str, ahora: datetime, politica: PoliticaBloqueo
    ) -> None: ...

    def registrar_exito(self, identificador: str) -> None: ...


class ControlIntentosAutenticacion:
    """Control local para pruebas y ejecuciones sin persistencia."""

    def __init__(self) -> None:
        self._intentos: dict[str, tuple[int, datetime]] = {}
        self._candado = Lock()

    def verificar(self, identificador: str, ahora: datetime, politica: PoliticaBloqueo) -> None:
        with self._candado:
            registro = self._intentos.get(identificador)
            if registro is None:
                return
            intentos, bloqueado_hasta = registro
            if intentos < politica.max_intentos:
                return
            if bloqueado_hasta > ahora:
                raise AutenticacionBloqueada(
                    "Demasiados intentos. Intente nuevamente más tarde"
                )
            del self._intentos[identificador]

    def registrar_fallo(
        self, identificador: str, ahora: datetime, politica: PoliticaBloqueo
    ) -> None:
        with self._candado:
            intentos, bloqueado_hasta = self._intentos.get(identificador, (0, ahora))
            if bloqueado_hasta > ahora:
                return
            intentos += 1
            if intentos >= politica.max_intentos:
                self._intentos[identificador] = (
                    intentos,
                    ahora + timedelta(minutes=politica.minutos_bloqueo),
                )
            else:
                self._intentos[identificador] = (intentos, ahora)

    def registrar_exito(self, identificador: str) -> None:
        with self._candado:
            self._intentos.pop(identificador, None)


class ServicioIdentidad:
    def __init__(
        self,
        usuarios: RepositorioUsuarios,
        sesiones: RepositorioSesiones,
        duracion_sesion: timedelta = timedelta(hours=8),
        reloj: Callable[[], datetime] | None = None,
        politica_bloqueo: PoliticaBloqueo | None = None,
        control_intentos: ControlIntentos | None = None,
    ) -> None:
        self._usuarios = usuarios
        self._sesiones = sesiones
        self._duracion_sesion = duracion_sesion
        self._reloj = reloj or (lambda: datetime.now(timezone.utc))
        self._politica_bloqueo = politica_bloqueo
        self._control_intentos = control_intentos

    def autenticar(self, nombre_usuario: str, contrasena: str) -> ResultadoAutenticacion:
        ahora = self._reloj()
        self._verificar_bloqueo(nombre_usuario, ahora)
        usuario = self._usuarios.buscar_por_nombre(nombre_usuario)
        if (
            usuario is None
            or not usuario.activo
            or not verificar_contrasena(contrasena, usuario.hash_contrasena)
        ):
            self._registrar_fallo(nombre_usuario, ahora)
            raise AutenticacionFallida("Las credenciales no son válidas")

        self._registrar_exito(nombre_usuario)
        expira_en = ahora + self._duracion_sesion
        secreto = crear_secreto_sesion()
        id_sesion = secrets.token_urlsafe(24)
        self._sesiones.guardar(
            SesionPersistida(
                idSesion=id_sesion,
                idUsuario=usuario.id_usuario,
                secretoHash=hash_secreto_sesion(secreto),
                expiraEn=expira_en,
            )
        )
        return ResultadoAutenticacion(
            idSesion=id_sesion,
            idUsuario=usuario.id_usuario,
            nombreUsuario=usuario.nombre_usuario,
            secretoSesion=secreto,
            expiraEn=expira_en,
            permisos=usuario.permisos,
        )

    def verificar_bloqueo(self, identificador: str) -> None:
        self._verificar_bloqueo(identificador, self._reloj())

    def registrar_fallo_autenticacion(self, identificador: str) -> None:
        self._registrar_fallo(identificador, self._reloj())

    def registrar_exito_autenticacion(self, identificador: str) -> None:
        self._registrar_exito(identificador)

    def _verificar_bloqueo(self, identificador: str, ahora: datetime) -> None:
        if self._politica_bloqueo and self._control_intentos:
            self._control_intentos.verificar(identificador, ahora, self._politica_bloqueo)

    def _registrar_fallo(self, identificador: str, ahora: datetime) -> None:
        if self._politica_bloqueo and self._control_intentos:
            self._control_intentos.registrar_fallo(identificador, ahora, self._politica_bloqueo)

    def _registrar_exito(self, identificador: str) -> None:
        if self._control_intentos:
            self._control_intentos.registrar_exito(identificador)

    def crear_sesion(
        self, id_usuario: int, nombre_usuario: str, permisos: frozenset[str] = frozenset()
    ) -> ResultadoAutenticacion:
        ahora = self._reloj()
        expira_en = ahora + self._duracion_sesion
        secreto = crear_secreto_sesion()
        id_sesion = secrets.token_urlsafe(24)
        self._sesiones.guardar(
            SesionPersistida(
                idSesion=id_sesion,
                idUsuario=id_usuario,
                secretoHash=hash_secreto_sesion(secreto),
                expiraEn=expira_en,
            )
        )
        return ResultadoAutenticacion(
            idSesion=id_sesion,
            idUsuario=id_usuario,
            nombreUsuario=nombre_usuario,
            secretoSesion=secreto,
            expiraEn=expira_en,
            permisos=permisos,
        )

    def validar_sesion(self, id_sesion: str, secreto: str) -> SesionPersistida:
        sesion = self._sesiones.buscar_vigente(id_sesion, self._reloj())
        if (
            sesion is None
            or sesion.revocada
            or not comparar_secreto_sesion(secreto, sesion.secreto_hash)
        ):
            raise AutenticacionFallida("La sesión no es válida")
        return sesion

    def cerrar_sesion(self, id_sesion: str) -> None:
        self._sesiones.revocar(id_sesion, self._reloj())

    def establecer_csrf(self, id_sesion: str, token: str) -> None:
        """Asocia un token CSRF al ciclo de vida de una sesión."""
        if not token:
            raise ValueError("El token CSRF no puede estar vacío")
        self._sesiones.actualizar_csrf(id_sesion, hashlib.sha256(token.encode("utf-8")).hexdigest())

    def validar_csrf(self, sesion: SesionPersistida, token: str) -> bool:
        """Valida CSRF sin comparar secretos en claro ni aceptar sesiones sin token."""
        if not token or not sesion.csrf_hash:
            return False
        return secrets.compare_digest(
            hashlib.sha256(token.encode("utf-8")).hexdigest(), sesion.csrf_hash
        )

    def permisos_de_sesion(self, sesion: SesionPersistida) -> frozenset[str]:
        """Obtiene permisos actuales; nunca confía en datos enviados por el cliente."""
        usuario = self._usuarios.buscar_por_id(sesion.id_usuario)
        if usuario is None or not usuario.activo:
            raise AutenticacionFallida("El usuario no está disponible")
        return usuario.permisos


class ServicioPermisos:
    """Evalúa permisos entregados por el repositorio, sin confiar en el frontend."""

    @staticmethod
    def tiene(permisos: frozenset[str] | set[str], permiso: str) -> bool:
        return permiso in permisos


@dataclass(frozen=True)
class SesionResuelta:
    """Sesión autenticada sin exponer el mecanismo de selección de identidad."""

    sesion: SesionPersistida
    tipo: str
    usuario: dict[str, object]


class ServicioSesiones:
    """Caso de uso común del ciclo de vida de sesiones web."""

    def __init__(
        self,
        administracion: ServicioIdentidad,
        estudiantes: ServicioIdentidad | None = None,
        perfil_estudiante: Callable[[int], dict[str, object]] | None = None,
    ) -> None:
        self._administracion = administracion
        self._estudiantes = estudiantes
        self._perfil_estudiante = perfil_estudiante

    def validar(self, id_sesion: str, secreto: str) -> SesionResuelta:
        """Valida una sesión y devuelve su proyección pública."""

        try:
            sesion = self._administracion.validar_sesion(id_sesion, secreto)
            permisos = self._administracion.permisos_de_sesion(sesion)
            return SesionResuelta(
                sesion,
                "admin",
                {
                    "idUsuario": sesion.id_usuario,
                    "permisos": list(permisos),
                    "roles": ["Administrador"]
                    if "administracion.usuarios.editar" in permisos
                    else [],
                },
            )
        except AutenticacionFallida:
            if self._estudiantes is None:
                raise AutenticacionFallida("La sesión no es válida")
            sesion = self._estudiantes.validar_sesion(id_sesion, secreto)
            perfil: dict[str, object] = (
                self._perfil_estudiante(sesion.id_usuario)
                if self._perfil_estudiante
                else {"idEstudiante": sesion.id_usuario}
            )
            return SesionResuelta(sesion, "estudiante", perfil)

    def cerrar(self, id_sesion: str, secreto: str, token_csrf: str, csrf_cookie: str) -> None:
        """Valida CSRF y revoca la sesión correspondiente."""

        try:
            servicio = self._administracion
            sesion = servicio.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida:
            if self._estudiantes is None:
                raise AutenticacionFallida("La sesión no es válida")
            servicio = self._estudiantes
            sesion = servicio.validar_sesion(id_sesion, secreto)
        if (
            not token_csrf
            or token_csrf != csrf_cookie
            or not servicio.validar_csrf(sesion, token_csrf)
        ):
            raise ValueError("El token CSRF no es válido")
        servicio.cerrar_sesion(sesion.id_sesion)


def preparar_hash_contrasena(contrasena: str) -> str:
    """Punto explícito para alta o restablecimiento de credenciales web."""
    return hash_contrasena(contrasena)
