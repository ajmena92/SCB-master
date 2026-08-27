"""Primitivas criptográficas de identidad.

Este módulo no conoce HTTP, SQL Server ni el modelo de usuarios del sistema local.
"""

from __future__ import annotations

import hashlib
import secrets

from argon2 import PasswordHasher
from argon2.exceptions import InvalidHashError, VerificationError, VerifyMismatchError
from argon2.low_level import Type

_HASHEADOR = PasswordHasher(
    time_cost=3,
    memory_cost=65_536,
    parallelism=4,
    hash_len=32,
    salt_len=16,
    type=Type.ID,
)


def hash_contrasena(contrasena: str) -> str:
    """Genera un hash Argon2id para una contraseña no vacía."""
    if not contrasena:
        raise ValueError("La contraseña no puede estar vacía")
    return _HASHEADOR.hash(contrasena)


def verificar_contrasena(contrasena: str, hash_almacenado: str) -> bool:
    """Verifica exclusivamente hashes Argon2id; nunca acepta formatos legacy."""
    try:
        return _HASHEADOR.verify(hash_almacenado, contrasena)
    except (VerifyMismatchError, VerificationError, InvalidHashError, ValueError):
        return False


def requiere_rehash(hash_almacenado: str) -> bool:
    """Indica si los parámetros del hash deben actualizarse."""
    try:
        return _HASHEADOR.check_needs_rehash(hash_almacenado)
    except (InvalidHashError, ValueError):
        return True


def crear_secreto_sesion() -> str:
    """Genera un secreto opaco que solo se entrega al cliente una vez."""
    return secrets.token_urlsafe(48)


def hash_secreto_sesion(secreto: str) -> str:
    """Devuelve el digest persistible del secreto de sesión."""
    return hashlib.sha256(secreto.encode("utf-8")).hexdigest()


def comparar_secreto_sesion(secreto: str, digest_almacenado: str) -> bool:
    """Compara secretos sin filtrar diferencias de tiempo."""
    return secrets.compare_digest(hash_secreto_sesion(secreto), digest_almacenado)
