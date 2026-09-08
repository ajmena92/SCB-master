"""Hash Argon2, codigos institucionales y sesiones opacas."""

from __future__ import annotations

import hashlib
import hmac
import secrets
from datetime import datetime, timedelta, timezone

from argon2 import PasswordHasher
from argon2.exceptions import VerifyMismatchError

from aplicacion.modelos.maestros import SesionAcceso

_HASHER = PasswordHasher()


def hash_secreto(secreto: str) -> str:
    return _HASHER.hash(secreto)


def verificar_secreto(hash_guardado: str, secreto: str) -> bool:
    try:
        return _HASHER.verify(hash_guardado, secreto)
    except VerifyMismatchError:
        return False


def generar_codigo(codigo_existe, tipo: str) -> str:
    prefijo = "E" if tipo == "estudiante" else "P"
    while True:
        base = f"{secrets.randbelow(10_000_000):07d}"
        verificador = str(sum((i + 2) * int(n) for i, n in enumerate(base)) % 10)
        codigo = f"{prefijo}-{base}{verificador}"
        if not codigo_existe(codigo):
            return codigo


def nueva_sesion(
    *,
    tipo: str,
    persona_id: int | None = None,
    cuenta_id: int | None = None,
    cambio_obligatorio: bool = False,
    student_session_days: int,
    admin_session_minutes: int,
) -> tuple[str, SesionAcceso]:
    if tipo not in {"portal", "administracion"}:
        raise ValueError("Tipo de sesion no valido")
    token = secrets.token_urlsafe(32)
    duracion = (
        timedelta(days=student_session_days)
        if tipo == "portal"
        else timedelta(minutes=admin_session_minutes)
    )
    registro = SesionAcceso(
        token_hash=hashlib.sha256(token.encode()).hexdigest(),
        tipo=tipo,
        persona_id=persona_id,
        cuenta_id=cuenta_id,
        cambio_obligatorio=cambio_obligatorio,
        expira_en=datetime.now(timezone.utc) + duracion,
    )
    return token, registro


def token_hash(token: str) -> str:
    return hashlib.sha256(token.encode()).hexdigest()


def csrf_anonimo(secreto: str, *, ttl_seconds: int = 600, ahora: datetime | None = None) -> str:
    """Token de login firmado, anónimo y con vencimiento explícito."""
    instante = ahora or datetime.now(timezone.utc)
    emitido = int(instante.timestamp())
    vence = emitido + ttl_seconds
    nonce = secrets.token_urlsafe(24)
    cuerpo = f"anonimo:{emitido}:{vence}:{nonce}"
    firma = hmac.new(secreto.encode(), cuerpo.encode(), hashlib.sha256).hexdigest()
    return f"a.{emitido}.{vence}.{nonce}.{firma}"


def csrf_sesion(token: str, secreto: str) -> str:
    """Vincula el CSRF a la sesión opaca que solo viaja en cookie HttpOnly."""
    firma = hmac.new(secreto.encode(), f"sesion:{token}".encode(), hashlib.sha256).hexdigest()
    return f"s.{firma}"


def csrf_valido(
    valor: str | None,
    *,
    token: str | None,
    secreto: str,
    ahora: datetime | None = None,
) -> bool:
    if not valor:
        return False
    esperado = csrf_sesion(token, secreto) if token else None
    if esperado is not None:
        return hmac.compare_digest(valor, esperado)
    try:
        prefijo, emitido, vence, nonce, firma = valor.split(".", 4)
        emitido_entero, vence_entero = int(emitido), int(vence)
    except (ValueError, AttributeError):
        return False
    instante = int((ahora or datetime.now(timezone.utc)).timestamp())
    if prefijo != "a" or not nonce or emitido_entero > instante or vence_entero < instante:
        return False
    cuerpo = f"anonimo:{emitido_entero}:{vence_entero}:{nonce}"
    esperado = hmac.new(secreto.encode(), cuerpo.encode(), hashlib.sha256).hexdigest()
    return hmac.compare_digest(firma, esperado)
