"""Contrato de configuración para cookies y vigencia absoluta."""

import pytest

from config import Settings


def _entorno_valido(monkeypatch: pytest.MonkeyPatch) -> None:
    valores = {
        "DATABASE_URL": "postgresql://usuario:clave@localhost/scb",
        "CORS_ORIGIN": "http://localhost:5173",
        "COOKIE_SECURE": "false",
        "CSRF_SECRET": "csrf-pruebas",
        "CARNET_QR_CLAVE": "qr-pruebas",
        "STUDENT_MAX_LOGIN_ATTEMPTS": "8",
        "STUDENT_LOCK_MINUTES": "5",
        "ADMIN_MAX_LOGIN_ATTEMPTS": "5",
        "ADMIN_LOCK_MINUTES": "15",
        "STUDENT_SESSION_DAYS": "365",
        "ADMIN_SESSION_MINUTES": "60",
        "CSRF_ANONYMOUS_TTL_SECONDS": "600",
    }
    for nombre, valor in valores.items():
        monkeypatch.setenv(nombre, valor)
        monkeypatch.delenv(f"{nombre}_FILE", raising=False)


def test_configuracion_lee_vigencias_absolutas(monkeypatch: pytest.MonkeyPatch) -> None:
    _entorno_valido(monkeypatch)
    configuracion = Settings.from_environment()

    assert configuracion.student_session_days == 365
    assert configuracion.admin_session_minutes == 60
    assert configuracion.csrf_anonymous_ttl_seconds == 600
    assert not configuracion.cookie_secure


def test_cookie_no_segura_se_rechaza_fuera_de_localhost(monkeypatch: pytest.MonkeyPatch) -> None:
    _entorno_valido(monkeypatch)
    monkeypatch.setenv("CORS_ORIGIN", "https://comedor.institucion.ac.cr")

    with pytest.raises(RuntimeError, match="COOKIE_SECURE"):
        Settings.from_environment()
