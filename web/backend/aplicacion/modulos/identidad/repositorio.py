"""Puertos e implementaciones SQL de persistencia de identidad.

Las implementaciones solo conocen los contratos de :mod:`nucleo.base_datos` y el
esquema ``identidad``. No contienen decisiones de autenticación ni dependencias
del sistema local.
"""

from __future__ import annotations

from datetime import datetime, timedelta, timezone
import hashlib
from typing import TYPE_CHECKING
from typing import Protocol, Sequence

from aplicacion.nucleo.base_datos import FabricaConexionSql

from .esquemas import CredencialesUsuario, SesionPersistida
if TYPE_CHECKING:
    from .servicio import PoliticaBloqueo


class RepositorioUsuarios(Protocol):
    def buscar_por_nombre(self, nombre_usuario: str) -> CredencialesUsuario | None: ...

    def buscar_por_id(self, id_usuario: int) -> CredencialesUsuario | None: ...


class RepositorioSesiones(Protocol):
    def guardar(self, sesion: SesionPersistida) -> None: ...

    def buscar_vigente(self, id_sesion: str, ahora: datetime) -> SesionPersistida | None: ...

    def revocar(self, id_sesion: str, ahora: datetime) -> None: ...

    def actualizar_csrf(self, id_sesion: str, csrf_hash: str) -> None: ...


class RepositorioIntentosAutenticacion(Protocol):
    def verificar(self, identificador: str, ahora: datetime, politica: PoliticaBloqueo) -> None: ...

    def registrar_fallo(
        self, identificador: str, ahora: datetime, politica: PoliticaBloqueo
    ) -> None: ...

    def registrar_exito(self, identificador: str) -> None: ...


class RepositorioSqlUsuarios:
    """Lee usuarios y sus permisos del esquema canónico de identidad."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def buscar_por_nombre(self, nombre_usuario: str) -> CredencialesUsuario | None:
        consulta = """
            WITH permisos_usuario AS (
                SELECT up.id_usuario, p.clave
                FROM identidad.usuario_permiso AS up
                INNER JOIN identidad.permiso AS p ON p.id_permiso = up.id_permiso
                WHERE p.activo = 1
                UNION
                SELECT ur.id_usuario, p.clave
                FROM identidad.usuario_rol AS ur
                INNER JOIN identidad.rol AS r ON r.id_rol = ur.id_rol AND r.activo = 1
                INNER JOIN identidad.rol_permiso AS rp ON rp.id_rol = r.id_rol
                INNER JOIN identidad.permiso AS p ON p.id_permiso = rp.id_permiso AND p.activo = 1
            )
            SELECT u.id_usuario, u.nombre_usuario, u.hash_contrasena, u.activo,
                   permisos_usuario.clave AS permiso
            FROM identidad.usuario AS u
            LEFT JOIN permisos_usuario ON permisos_usuario.id_usuario = u.id_usuario
            WHERE u.nombre_usuario = ?
            ORDER BY permisos_usuario.clave
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta, nombre_usuario)
            filas = cursor.fetchall()
        if not filas:
            return None
        primera = filas[0]
        permisos = frozenset(str(fila[4]) for fila in filas if fila[4] is not None)
        return CredencialesUsuario(
            idUsuario=int(str(primera[0])),
            nombreUsuario=str(primera[1]),
            hashContrasena=str(primera[2]),
            activo=bool(primera[3]),
            permisos=permisos,
        )

    def buscar_por_id(self, id_usuario: int) -> CredencialesUsuario | None:
        consulta = """
            WITH permisos_usuario AS (
                SELECT up.id_usuario, p.clave
                FROM identidad.usuario_permiso AS up
                INNER JOIN identidad.permiso AS p ON p.id_permiso = up.id_permiso
                WHERE p.activo = 1
                UNION
                SELECT ur.id_usuario, p.clave
                FROM identidad.usuario_rol AS ur
                INNER JOIN identidad.rol AS r ON r.id_rol = ur.id_rol AND r.activo = 1
                INNER JOIN identidad.rol_permiso AS rp ON rp.id_rol = r.id_rol
                INNER JOIN identidad.permiso AS p ON p.id_permiso = rp.id_permiso AND p.activo = 1
            )
            SELECT u.id_usuario, u.nombre_usuario, u.hash_contrasena, u.activo,
                   permisos_usuario.clave AS permiso
            FROM identidad.usuario AS u
            LEFT JOIN permisos_usuario ON permisos_usuario.id_usuario = u.id_usuario
            WHERE u.id_usuario = ?
            ORDER BY permisos_usuario.clave
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta, id_usuario)
            filas = cursor.fetchall()
        if not filas:
            return None
        primera = filas[0]
        permisos = frozenset(str(fila[4]) for fila in filas if fila[4] is not None)
        return CredencialesUsuario(
            idUsuario=int(str(primera[0])),
            nombreUsuario=str(primera[1]),
            hashContrasena=str(primera[2]),
            activo=bool(primera[3]),
            permisos=permisos,
        )


class RepositorioSqlSesiones:
    """Persiste sesiones opacas y nunca almacena el secreto en claro."""

    def __init__(self, fabrica: FabricaConexionSql, tabla: str = "identidad.sesion") -> None:
        self._fabrica = fabrica
        if tabla not in {"identidad.sesion", "identidad.sesion_estudiante"}:
            raise ValueError("Tabla de sesión no permitida")
        self._tabla = tabla

    def guardar(self, sesion: SesionPersistida) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                f"""
                INSERT INTO {self._tabla}
                    (id_sesion, id_usuario, secreto_hash, expira_en, csrf_hash, revocada)
                VALUES (?, ?, ?, ?, ?, ?)
                """,
                sesion.id_sesion,
                sesion.id_usuario,
                sesion.secreto_hash,
                sesion.expira_en,
                sesion.csrf_hash,
                sesion.revocada,
            )

    def buscar_vigente(self, id_sesion: str, ahora: datetime) -> SesionPersistida | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                f"""
                SELECT id_sesion, id_usuario, secreto_hash, expira_en, csrf_hash, revocada
                FROM {self._tabla}
                WHERE id_sesion = ? AND revocada = 0 AND expira_en > ?
                """,
                id_sesion,
                ahora,
            )
            fila = cursor.fetchone()
        return self._convertir(fila)

    def revocar(self, id_sesion: str, ahora: datetime) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                f"""
                UPDATE {self._tabla}
                SET revocada = 1, fecha_revocacion = ?
                WHERE id_sesion = ? AND revocada = 0
                """,
                ahora,
                id_sesion,
            )

    def actualizar_csrf(self, id_sesion: str, csrf_hash: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                f"""
                UPDATE {self._tabla}
                SET csrf_hash = ?
                WHERE id_sesion = ? AND revocada = 0
                """,
                csrf_hash,
                id_sesion,
            )

    @staticmethod
    def _convertir(fila: Sequence[object] | None) -> SesionPersistida | None:
        if fila is None:
            return None
        expira_en = fila[3]
        if not isinstance(expira_en, datetime):
            raise TypeError("La fecha de expiración de sesión no es válida")
        if expira_en.tzinfo is None:
            expira_en = expira_en.replace(tzinfo=timezone.utc)
        return SesionPersistida(
            idSesion=str(fila[0]),
            idUsuario=int(str(fila[1])),
            secretoHash=str(fila[2]),
            expiraEn=expira_en,
            csrfHash=str(fila[4]) if fila[4] is not None else None,
            revocada=bool(fila[5]),
        )


class RepositorioSqlSesionesEstudiante(RepositorioSqlSesiones):
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        super().__init__(fabrica, "identidad.sesion_estudiante")


class RepositorioSqlIntentosAutenticacion:
    """Persiste los bloqueos para que sean comunes a workers y réplicas."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _identificador_hash(identificador: str) -> str:
        return hashlib.sha256(identificador.encode("utf-8")).hexdigest()

    def verificar(self, identificador: str, ahora: datetime, politica: PoliticaBloqueo) -> None:
        from aplicacion.modulos.identidad.servicio import AutenticacionBloqueada

        clave = self._identificador_hash(identificador)
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """
                SELECT intentos_fallidos, bloqueado_hasta
                FROM identidad.intento_autenticacion WITH (UPDLOCK, HOLDLOCK)
                WHERE identificador_hash = ?
                """,
                clave,
            )
            fila = cursor.fetchone()
            if fila is None:
                return
            intentos, bloqueado_hasta = int(fila[0]), fila[1]
            if bloqueado_hasta is not None and bloqueado_hasta > ahora:
                raise AutenticacionBloqueada(
                    "Demasiados intentos. Intente nuevamente más tarde"
                )
            if intentos >= politica.max_intentos:
                cursor.execute(
                    "DELETE FROM identidad.intento_autenticacion WHERE identificador_hash = ?",
                    clave,
                )

    def registrar_fallo(
        self, identificador: str, ahora: datetime, politica: PoliticaBloqueo
    ) -> None:
        clave = self._identificador_hash(identificador)
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """
                SELECT intentos_fallidos, bloqueado_hasta
                FROM identidad.intento_autenticacion WITH (UPDLOCK, HOLDLOCK)
                WHERE identificador_hash = ?
                """,
                clave,
            )
            fila = cursor.fetchone()
            if fila is not None and fila[1] is not None and fila[1] > ahora:
                return
            intentos = (int(fila[0]) if fila is not None else 0) + 1
            bloqueado_hasta = (
                ahora + timedelta(minutes=politica.minutos_bloqueo)
                if intentos >= politica.max_intentos
                else None
            )
            if fila is None:
                cursor.execute(
                    """
                    INSERT INTO identidad.intento_autenticacion
                        (identificador_hash, intentos_fallidos, bloqueado_hasta, fecha_actualizacion)
                    VALUES (?, ?, ?, ?)
                    """,
                    clave,
                    intentos,
                    bloqueado_hasta,
                    ahora,
                )
            else:
                cursor.execute(
                    """
                    UPDATE identidad.intento_autenticacion
                    SET intentos_fallidos = ?, bloqueado_hasta = ?, fecha_actualizacion = ?
                    WHERE identificador_hash = ?
                    """,
                    intentos,
                    bloqueado_hasta,
                    ahora,
                    clave,
                )

    def registrar_exito(self, identificador: str) -> None:
        clave = self._identificador_hash(identificador)
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "DELETE FROM identidad.intento_autenticacion WHERE identificador_hash = ?",
                clave,
            )
