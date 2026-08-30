"""Hash Argon2, codigos institucionales y sesiones opacas."""

from __future__ import annotations

import hashlib
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
) -> tuple[str, SesionAcceso]:
    token = secrets.token_urlsafe(32)
    registro = SesionAcceso(
        token_hash=hashlib.sha256(token.encode()).hexdigest(),
        tipo=tipo,
        persona_id=persona_id,
        cuenta_id=cuenta_id,
        cambio_obligatorio=cambio_obligatorio,
        expira_en=datetime.now(timezone.utc) + timedelta(hours=12),
    )
    return token, registro


def token_hash(token: str) -> str:
    return hashlib.sha256(token.encode()).hexdigest()
