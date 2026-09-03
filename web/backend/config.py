"""Configuracion de la plataforma PostgreSQL."""

from __future__ import annotations

import os
from dataclasses import dataclass
from urllib.parse import urlparse


def _origen_https(valor: str) -> str:
    origen = valor.strip().rstrip("/")
    parsed = urlparse(origen)
    if origen == "http://localhost:5173":
        return origen
    if parsed.scheme != "https" or not parsed.netloc or parsed.path not in ("", "/"):
        raise RuntimeError("CORS_ORIGIN debe ser un origen HTTPS unico")
    if "," in origen:
        raise RuntimeError("CORS_ORIGIN debe ser un origen HTTPS unico")
    return origen


def _entero_en_rango(nombre: str, minimo: int, maximo: int) -> int:
    try:
        valor = int(os.getenv(nombre, "").strip())
    except ValueError as exc:
        raise RuntimeError(f"{nombre} debe ser un entero") from exc
    if not minimo <= valor <= maximo:
        raise RuntimeError(f"{nombre} debe estar entre {minimo} y {maximo}")
    return valor


@dataclass(frozen=True)
class Settings:
    database_url: str
    cors_origin: str
    cookie_secure: bool
    app_timezone: str = "America/Costa_Rica"
    carnet_qr_clave: str = ""
    student_max_login_attempts: int = 8
    student_lock_minutes: int = 5
    admin_max_login_attempts: int = 5
    admin_lock_minutes: int = 15

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
        return cls(
            database_url,
            _origen_https(origen),
            seguro == "true",
            carnet_qr_clave=os.getenv("CARNET_QR_CLAVE", "").strip(),
            student_max_login_attempts=_entero_en_rango("STUDENT_MAX_LOGIN_ATTEMPTS", 3, 20),
            student_lock_minutes=_entero_en_rango("STUDENT_LOCK_MINUTES", 1, 120),
            admin_max_login_attempts=_entero_en_rango("ADMIN_MAX_LOGIN_ATTEMPTS", 3, 20),
            admin_lock_minutes=_entero_en_rango("ADMIN_LOCK_MINUTES", 1, 120),
        )
