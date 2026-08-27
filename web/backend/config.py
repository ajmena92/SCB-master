"""Configuration for the institutional deployment.

This module intentionally has no development defaults for database credentials.
"""

import ipaddress
import os
import re
from dataclasses import dataclass
from urllib.parse import urlparse


def _required(name: str) -> str:
    value = os.getenv(name, "").strip()
    if not value:
        raise RuntimeError(f"Missing required environment variable: {name}")
    return value


def _sql_connection_string() -> str:
    value = _required("SQL_CONNECTION_STRING")
    # Local-only routing override keeps credentials in the ignored env file while
    # allowing the developer environment to target the authorized SQL host.
    override = os.getenv("SQL_SERVER_OVERRIDE", "").strip()
    if override:
        value = re.sub(r"(?i)(?<=Server=)[^;]+", override, value, count=1)
    user_override = os.getenv("SQL_USER_OVERRIDE", "").strip()
    if user_override:
        value = re.sub(r"(?i)(?<=Uid=)[^;]+", user_override, value, count=1)
    password_override = os.getenv("SQL_PASSWORD_OVERRIDE", "")
    if password_override:
        value = re.sub(r"(?i)(?<=Pwd=)[^;]+", lambda _: password_override, value, count=1)
    return value


def _single_https_origin(value: str) -> str:
    """Accept exactly one browser origin, never a wildcard or a list."""
    origin = value.strip().rstrip("/")
    parsed = urlparse(origin)
    if (
        not origin
        or "," in origin
        or parsed.scheme != "https"
        or not parsed.netloc
        or parsed.path not in ("", "/")
        or parsed.params
        or parsed.query
        or parsed.fragment
    ):
        raise RuntimeError(
            "CORS_ORIGIN debe ser un único origen HTTPS, por ejemplo https://comedor.institucion.ac.cr"
        )
    return origin


def _trusted_proxy_cidrs(value: str) -> tuple[str, ...]:
    entries = tuple(item.strip() for item in value.split(",") if item.strip())
    if not entries:
        raise RuntimeError("TRUSTED_PROXY_CIDRS debe indicar la red del proxy inverso")
    try:
        return tuple(str(ipaddress.ip_network(item, strict=False)) for item in entries)
    except ValueError as exc:
        raise RuntimeError("TRUSTED_PROXY_CIDRS debe contener redes CIDR válidas") from exc


@dataclass(frozen=True)
class Settings:
    sql_connection_string: str
    cookie_secure: bool
    cors_origin: str
    app_timezone: str
    dias_sesion_estudiante: int
    admin_session_minutes: int
    student_max_login_attempts: int
    student_lock_minutes: int
    admin_max_login_attempts: int
    admin_lock_minutes: int
    trusted_proxy_cidrs: tuple[str, ...]

    @classmethod
    def from_environment(cls) -> "Settings":
        # SQL_CONNECTION_STRING must use ODBC Driver 18 and Encrypt=yes in production.
        return cls(
            sql_connection_string=_sql_connection_string(),
            cookie_secure=os.getenv("COOKIE_SECURE", "true").lower() == "true",
            cors_origin=_single_https_origin(_required("CORS_ORIGIN")),
            app_timezone=os.getenv("APP_TZ", "America/Costa_Rica"),
            dias_sesion_estudiante=int(os.getenv("DIAS_SESION_ESTUDIANTE", "365")),
            admin_session_minutes=int(os.getenv("ADMIN_SESSION_MINUTES", "60")),
            student_max_login_attempts=_bounded_int(
                "STUDENT_MAX_LOGIN_ATTEMPTS", "5", minimum=3, maximum=20
            ),
            student_lock_minutes=_bounded_int("STUDENT_LOCK_MINUTES", "15", minimum=1, maximum=120),
            # These defaults and bounds intentionally match SeguridadRbacService
            # in escritorio. Set them from the institutional desktop configuration
            # at deployment; never inherit the student-PIN policy.
            admin_max_login_attempts=_bounded_int(
                "ADMIN_MAX_LOGIN_ATTEMPTS", "8", minimum=3, maximum=20
            ),
            admin_lock_minutes=_bounded_int("ADMIN_LOCK_MINUTES", "5", minimum=1, maximum=120),
            trusted_proxy_cidrs=_trusted_proxy_cidrs(
                os.getenv("TRUSTED_PROXY_CIDRS", "127.0.0.1/32")
            ),
        )


def _bounded_int(name: str, default: str, *, minimum: int, maximum: int) -> int:
    try:
        value = int(os.getenv(name, default))
    except ValueError as exc:
        raise RuntimeError(f"{name} debe ser un número entero") from exc
    if not minimum <= value <= maximum:
        raise RuntimeError(f"{name} debe estar entre {minimum} y {maximum}")
    return value
