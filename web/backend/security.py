"""Password/PIN and opaque-session primitives; no browser-readable auth tokens."""

from __future__ import annotations

import base64
import hashlib
import hmac
import secrets
from typing import Optional

PBKDF2_ITERATIONS = 120_000


def make_pin_record(pin: str) -> tuple[bytes, bytes, int]:
    if not pin.isdigit() or len(pin) != 6:
        raise ValueError("El PIN debe tener 6 dígitos")
    salt = secrets.token_bytes(16)
    digest = hashlib.pbkdf2_hmac("sha256", pin.encode(), salt, PBKDF2_ITERATIONS)
    return digest, salt, PBKDF2_ITERATIONS


def verify_pin(pin: str, digest: bytes, salt: bytes, iterations: int) -> bool:
    candidate = hashlib.pbkdf2_hmac("sha256", pin.encode(), salt, iterations)
    return hmac.compare_digest(candidate, digest)


def verify_admin_password(password: str, stored_hash: str, stored_salt: Optional[str]) -> bool:
    """Matches the two formats used by the desktop SeguridadRbacService."""
    if stored_hash.startswith("LEGACY_SHA2_512:"):
        expected = stored_hash.split(":", 1)[1]
        actual = hashlib.sha512(f"{password}:{stored_salt or ''}".encode()).hexdigest()
        return hmac.compare_digest(expected.lower(), actual.lower())
    if stored_hash.startswith("PBKDF2$"):
        try:
            _, iterations, salt_b64, expected_b64 = stored_hash.split("$", 3)
            expected = base64.b64decode(expected_b64)
            actual = hashlib.pbkdf2_hmac(
                "sha1",
                password.encode(),
                base64.b64decode(salt_b64),
                int(iterations),
                dklen=len(expected),
            )
            return hmac.compare_digest(actual, expected)
        except (ValueError, TypeError, base64.binascii.Error):
            return False
    return False


def new_session_secret() -> str:
    return secrets.token_urlsafe(48)


def hash_session_secret(secret: str) -> bytes:
    return hashlib.sha256(secret.encode()).digest()


def new_csrf_token() -> str:
    return secrets.token_urlsafe(32)
