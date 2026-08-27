import sys
from types import SimpleNamespace

from database import SqlConnectionFactory


class _Connection:
    def commit(self):
        pass

    def rollback(self):
        pass

    def close(self):
        pass


def test_connection_retries_transient_odbc_failure_before_yielding(monkeypatch):
    class OperationalError(Exception):
        pass

    attempts = []

    def connect(*_args, **_kwargs):
        attempts.append(1)
        if len(attempts) == 1:
            raise OperationalError("transient handshake failure")
        return _Connection()

    monkeypatch.setitem(
        sys.modules, "pyodbc", SimpleNamespace(connect=connect, OperationalError=OperationalError)
    )
    monkeypatch.setattr("database.time.sleep", lambda _delay: None)

    with SqlConnectionFactory("test").connection() as connection:
        assert isinstance(connection, _Connection)

    assert len(attempts) == 2
