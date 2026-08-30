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


@dataclass(frozen=True)
class Settings:
    database_url: str
    cors_origin: str
    cookie_secure: bool
    app_timezone: str = "America/Costa_Rica"

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
        return cls(database_url, _origen_https(origen), seguro == "true")
