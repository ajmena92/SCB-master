"""Puertos e implementaciones SQL de persistencia de identidad.

Las implementaciones solo conocen los contratos de :mod:`nucleo.base_datos` y el
esquema ``identidad``. No contienen decisiones de autenticación ni dependencias
del sistema local.
"""

from __future__ import annotations

from datetime import datetime, timezone
from typing import Protocol, Sequence

from aplicacion.nucleo.base_datos import FabricaConexionSql

from .esquemas import CredencialesUsuario, SesionPersistida


class RepositorioUsuarios(Protocol):
    def buscar_por_nombre(self, nombre_usuario: str) -> CredencialesUsuario | None: ...

    def buscar_por_id(self, id_usuario: int) -> CredencialesUsuario | None: ...


class RepositorioSesiones(Protocol):
    def guardar(self, sesion: SesionPersistida) -> None: ...

    def buscar_vigente(self, id_sesion: str, ahora: datetime) -> SesionPersistida | None: ...

    def revocar(self, id_sesion: str, ahora: datetime) -> None: ...

    def actualizar_csrf(self, id_sesion: str, csrf_hash: str) -> None: ...


class RepositorioSqlUsuarios:
    """Lee usuarios y sus permisos del esquema canónico de identidad."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def buscar_por_nombre(self, nombre_usuario: str) -> CredencialesUsuario | None:
        consulta = """
            SELECT u.id_usuario, u.nombre_usuario, u.hash_contrasena, u.activo,
                   p.clave AS permiso
            FROM identidad.usuario AS u
            LEFT JOIN identidad.usuario_permiso AS up ON up.id_usuario = u.id_usuario
            LEFT JOIN identidad.permiso AS p ON p.id_permiso = up.id_permiso
            WHERE u.nombre_usuario = ?
            ORDER BY p.clave
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
            SELECT u.id_usuario, u.nombre_usuario, u.hash_contrasena, u.activo,
                   p.clave AS permiso
            FROM identidad.usuario AS u
            LEFT JOIN identidad.usuario_permiso AS up ON up.id_usuario = u.id_usuario
            LEFT JOIN identidad.permiso AS p ON p.id_permiso = up.id_permiso
            WHERE u.id_usuario = ?
            ORDER BY p.clave
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
