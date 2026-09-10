"""Configuracion de la plataforma PostgreSQL."""

from __future__ import annotations

import os
from dataclasses import dataclass
from pathlib import Path
from urllib.parse import urlparse


def _origen_https(valor: str) -> str:
    origen = valor.strip().rstrip("/")
    parsed = urlparse(origen)
    if origen in {
        "http://localhost:8082",
        "http://127.0.0.1:8082",
        "http://localhost:3000",
        "http://127.0.0.1:3000",
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "http://localhost:8081",
        "http://127.0.0.1:8081",
    }:
        return origen
    if parsed.scheme != "https" or not parsed.netloc or parsed.path not in ("", "/"):
        raise RuntimeError("CORS_ORIGIN debe ser un origen HTTPS unico")
    return origen


def _origenes(valor: str) -> tuple[str, ...]:
    origenes = tuple(_origen_https(item) for item in valor.split(",") if item.strip())
    if not origenes:
        raise RuntimeError("CORS_ORIGIN debe definir al menos un origen")
    if len(origenes) != len(set(origenes)):
        raise RuntimeError("CORS_ORIGIN no debe repetir orígenes")
    return origenes


def _entero_en_rango(nombre: str, minimo: int, maximo: int) -> int:
    try:
        valor = int(os.getenv(nombre, "").strip())
    except ValueError as exc:
        raise RuntimeError(f"{nombre} debe ser un entero") from exc
    if not minimo <= valor <= maximo:
        raise RuntimeError(f"{nombre} debe estar entre {minimo} y {maximo}")
    return valor


def _secreto(nombre: str) -> str:
    """Obtiene un secreto desde un archivo montado o, solo para desarrollo, entorno."""
    ruta = os.getenv(f"{nombre}_FILE", "").strip()
    if ruta:
        try:
            valor = Path(ruta).read_text(encoding="utf-8").strip()
        except OSError as exc:
            raise RuntimeError(f"No se pudo leer {nombre}_FILE") from exc
    else:
        valor = os.getenv(nombre, "").strip()
    if not valor:
        raise RuntimeError(f"{nombre} es requerida")
    return valor


@dataclass(frozen=True)
class Settings:
    database_url: str
    cors_origin: str
    cookie_secure: bool
    csrf_secret: str
    cors_origins: tuple[str, ...] = ()
    app_env: str = "production"
    app_timezone: str = "America/Costa_Rica"
    carnet_qr_clave: str = ""
    student_max_login_attempts: int = 8
    student_lock_minutes: int = 5
    admin_max_login_attempts: int = 5
    admin_lock_minutes: int = 15
    student_session_days: int = 365
    admin_session_minutes: int = 60
    csrf_anonymous_ttl_seconds: int = 600
    importacion_resultados_key: str = ""

    @classmethod
    def from_environment(cls) -> "Settings":
        database_url = os.getenv("DATABASE_URL", "").strip()
        if not database_url:
            raise RuntimeError("DATABASE_URL es requerida")
        if not database_url.startswith(("postgresql+psycopg://", "postgresql://")):
            raise RuntimeError("DATABASE_URL debe usar PostgreSQL")
        origen = os.getenv("CORS_ORIGIN", "").strip()
        if not origen:
            raise RuntimeError("CORS_ORIGIN es requerida")
        seguro = os.getenv("COOKIE_SECURE", "true").lower()
        if seguro not in {"true", "false"}:
            raise RuntimeError("COOKIE_SECURE debe ser true o false")
        origenes = _origenes(origen)
        app_env = os.getenv("APP_ENV", "").strip().lower()
        if not app_env:
            app_env = (
                "development"
                if all(
                    item
                    in {
                        "http://localhost:5173",
                        "http://127.0.0.1:5173",
                        "http://localhost:8081",
                        "http://127.0.0.1:8081",
                        "http://localhost:8082",
                        "http://127.0.0.1:8082",
                        "http://localhost:3000",
                        "http://127.0.0.1:3000",
                    }
                    for item in origenes
                )
                else "production"
            )
        if app_env not in {"development", "production", "test"}:
            raise RuntimeError("APP_ENV debe ser development, production o test")
        if app_env == "production" and any(item.startswith("http://") for item in origenes):
            raise RuntimeError("Producción requiere orígenes HTTPS")
        origenes_locales = {
            "http://localhost:5173",
            "http://127.0.0.1:5173",
            "http://localhost:8081",
            "http://127.0.0.1:8081",
            "http://localhost:8082",
            "http://127.0.0.1:8082",
            "http://localhost:3000",
            "http://127.0.0.1:3000",
        }
        if seguro == "false" and any(item not in origenes_locales for item in origenes):
            raise RuntimeError(
                "COOKIE_SECURE solo puede ser false en orígenes localhost de desarrollo"
            )
        return cls(
            database_url=database_url,
            cors_origin=origenes[0],
            cookie_secure=seguro == "true",
            csrf_secret=_secreto("CSRF_SECRET"),
            cors_origins=origenes,
            app_env=app_env,
            carnet_qr_clave=_secreto("CARNET_QR_CLAVE"),
            student_max_login_attempts=_entero_en_rango("STUDENT_MAX_LOGIN_ATTEMPTS", 3, 20),
            student_lock_minutes=_entero_en_rango("STUDENT_LOCK_MINUTES", 1, 120),
            admin_max_login_attempts=_entero_en_rango("ADMIN_MAX_LOGIN_ATTEMPTS", 3, 20),
            admin_lock_minutes=_entero_en_rango("ADMIN_LOCK_MINUTES", 1, 120),
            student_session_days=_entero_en_rango("STUDENT_SESSION_DAYS", 1, 730),
            admin_session_minutes=_entero_en_rango("ADMIN_SESSION_MINUTES", 5, 720),
            csrf_anonymous_ttl_seconds=_entero_en_rango("CSRF_ANONYMOUS_TTL_SECONDS", 60, 3600),
            importacion_resultados_key=_secreto("IMPORTACION_RESULTADOS_KEY"),
        )
