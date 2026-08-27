import pytest

from config import Settings


def test_missing_sql_connection_is_rejected(monkeypatch):
    monkeypatch.delenv("SQL_CONNECTION_STRING", raising=False)
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    with pytest.raises(RuntimeError):
        Settings.from_environment()


def test_configuration_requires_single_explicit_origin(monkeypatch):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    assert Settings.from_environment().cors_origin == "https://portal.example"


def test_configuration_usa_nombre_canonico_de_sesion_estudiante(monkeypatch):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    monkeypatch.setenv("STUDENT_SESSION_DAYS", "42")
    assert Settings.from_environment().dias_sesion_estudiante == 42


@pytest.mark.parametrize("valor", ["treu", "1", "yes"])
def test_configuration_rechaza_cookie_secure_ambiguo(monkeypatch, valor):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    monkeypatch.setenv("COOKIE_SECURE", valor)
    with pytest.raises(RuntimeError, match="COOKIE_SECURE"):
        Settings.from_environment()


def test_configuration_rechaza_proxy_inconsistente(monkeypatch):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    monkeypatch.setenv("TRUSTED_PROXY_CIDRS", "10.0.0.0/24")
    monkeypatch.setenv("FORWARDED_ALLOW_IPS", "10.0.1.0/24")
    with pytest.raises(RuntimeError, match="FORWARDED_ALLOW_IPS"):
        Settings.from_environment()


@pytest.mark.parametrize(
    "origin",
    [
        "*",
        "http://portal.example",
        "https://a.example,https://b.example",
        "https://portal.example/ruta",
    ],
)
def test_configuration_rejects_non_single_https_cors_origin(monkeypatch, origin):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", origin)
    with pytest.raises(RuntimeError, match="CORS_ORIGIN"):
        Settings.from_environment()


def test_configuration_parses_only_valid_trusted_proxy_cidrs(monkeypatch):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    monkeypatch.setenv("TRUSTED_PROXY_CIDRS", "172.30.83.0/24,127.0.0.1/32")
    assert Settings.from_environment().trusted_proxy_cidrs == ("172.30.83.0/24", "127.0.0.1/32")


def test_lock_policy_uses_the_configured_platform_defaults(monkeypatch):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    settings = Settings.from_environment()
    assert settings.student_max_login_attempts == 8
    assert settings.student_lock_minutes == 5
    assert settings.admin_max_login_attempts == 5
    assert settings.admin_lock_minutes == 15


@pytest.mark.parametrize(
    "name,value",
    [
        ("ADMIN_MAX_LOGIN_ATTEMPTS", "2"),
        ("ADMIN_MAX_LOGIN_ATTEMPTS", "21"),
        ("ADMIN_LOCK_MINUTES", "0"),
        ("ADMIN_LOCK_MINUTES", "121"),
    ],
)
def test_administrative_lock_policy_rejects_values_outside_desktop_bounds(monkeypatch, name, value):
    monkeypatch.setenv(
        "SQL_CONNECTION_STRING", "Driver={ODBC Driver 18 for SQL Server};Server=test"
    )
    monkeypatch.setenv("CORS_ORIGIN", "https://portal.example")
    monkeypatch.setenv(name, value)
    with pytest.raises(RuntimeError, match=name):
        Settings.from_environment()
