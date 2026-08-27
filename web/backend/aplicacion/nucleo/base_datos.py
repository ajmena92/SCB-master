"""Límite de conexión SQL Server para la aplicación web."""

import time
from contextlib import contextmanager
from typing import Iterator, Protocol, Sequence

_INTENTOS_CONEXION = 3
_ESPERA_REINTENTO_SEGUNDOS = 0.2


class CursorSql(Protocol):
    """Operaciones DB-API que necesitan los repositorios web."""

    description: Sequence[tuple[str, object, object, object, object, object, object]]
    rowcount: int

    def execute(self, consulta: str, *parametros: object) -> "CursorSql": ...

    def fetchall(self) -> Sequence[Sequence[object]]: ...

    def fetchone(self) -> Sequence[object] | None: ...


class ConexionSql(Protocol):
    """Contrato mínimo de una conexión SQL, independiente de pyodbc."""

    def cursor(self) -> CursorSql: ...

    def commit(self) -> None: ...

    def rollback(self) -> None: ...

    def close(self) -> None: ...


class FabricaConexionSql:
    """Abre conexiones por unidad de trabajo y revierte fallos automáticamente."""

    def __init__(self, cadena_conexion: str) -> None:
        self.cadena_conexion = cadena_conexion

    @contextmanager
    def conexion(self) -> Iterator[ConexionSql]:
        try:
            import pyodbc
        except ImportError as exc:  # permite pruebas unitarias sin ODBC instalado
            raise RuntimeError("pyodbc/ODBC Driver 18 es necesario para SQL Server") from exc

        conexion: ConexionSql | None = None
        ultimo_error = None
        for intento in range(_INTENTOS_CONEXION):
            try:
                conexion = pyodbc.connect(self.cadena_conexion, autocommit=False)
                break
            except pyodbc.OperationalError as exc:
                ultimo_error = exc
                if intento == _INTENTOS_CONEXION - 1:
                    raise
                time.sleep(_ESPERA_REINTENTO_SEGUNDOS * (intento + 1))
        if conexion is None:  # pragma: no cover - el bucle siempre retorna o lanza
            raise RuntimeError("No se pudo abrir la conexión SQL") from ultimo_error
        try:
            yield conexion
            conexion.commit()
        except Exception:
            conexion.rollback()
            raise
        finally:
            conexion.close()
