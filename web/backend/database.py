"""Small pyodbc boundary. Connections are opened only per request/repository call."""

import time
from contextlib import AbstractContextManager, contextmanager
from typing import Iterator, Protocol


class SqlConnection(Protocol):
    """Contrato estructural mínimo de una conexión DB-API."""

    def cursor(self) -> object: ...
    def commit(self) -> None: ...
    def rollback(self) -> None: ...
    def close(self) -> None: ...


class SqlConnectionFactoryProtocol(Protocol):
    """Fábrica compatible con implementaciones reales y dobles de prueba."""

    def connection(self) -> AbstractContextManager[SqlConnection]: ...


_CONNECT_RETRIES = 3
_CONNECT_RETRY_DELAY_SECONDS = 0.2


class SqlConnectionFactory:
    def __init__(self, connection_string: str):
        self.connection_string = connection_string

    @contextmanager
    def connection(self) -> Iterator[object]:
        try:
            import pyodbc
        except ImportError as exc:  # keeps isolated unit tests independent of ODBC installation
            raise RuntimeError("pyodbc/ODBC Driver 18 is required to access SQL Server") from exc
        # The local Windows-to-WSL SQL relay can intermittently reject a TCP
        # handshake even though the SQL Server is healthy. Retrying only the
        # initial connection is safe: no SQL statement has run yet.
        last_error = None
        for attempt in range(_CONNECT_RETRIES):
            try:
                connection = pyodbc.connect(self.connection_string, autocommit=False)
                break
            except pyodbc.OperationalError as exc:
                last_error = exc
                if attempt == _CONNECT_RETRIES - 1:
                    raise
                time.sleep(_CONNECT_RETRY_DELAY_SECONDS * (attempt + 1))
        else:  # pragma: no cover - loop always breaks or raises
            raise last_error
        try:
            yield connection
            connection.commit()
        except Exception:
            connection.rollback()
            raise
        finally:
            connection.close()
