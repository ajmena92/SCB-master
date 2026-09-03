"""Credenciales QR cifradas para el carnet digital."""

from __future__ import annotations

import json
from datetime import date

from cryptography.fernet import Fernet, InvalidToken


PREFIJO_QR = "SCBQR1."
INSTITUCION = "ctp-platanares"


class ErrorCodigoQrCarnet(ValueError):
    """El token QR no puede usarse como credencial de carnet."""


class CodigoQrCarnet:
    def __init__(self, clave: str) -> None:
        try:
            self._fernet = Fernet(clave.encode("ascii"))
        except (ValueError, TypeError) as error:
            raise ErrorCodigoQrCarnet("La clave de QR no es válida") from error

    def emitir(self, *, id_persona: int, anio_lectivo: int, hoy: date) -> str:
        datos = {
            "v": 1,
            "i": INSTITUCION,
            "p": id_persona,
            "a": anio_lectivo,
            "e": f"{anio_lectivo}-12-31",
            "m": hoy.isoformat(),
        }
        contenido = json.dumps(datos, sort_keys=True, separators=(",", ":")).encode("utf-8")
        return f"{PREFIJO_QR}{self._fernet.encrypt(contenido).decode('ascii')}"

    def resolver(self, token: str, *, hoy: date) -> int:
        if not token.startswith(PREFIJO_QR):
            raise ErrorCodigoQrCarnet("El código no usa el formato QR del carnet")
        try:
            contenido = self._fernet.decrypt(token.removeprefix(PREFIJO_QR).encode("ascii"))
            datos = json.loads(contenido)
            vencimiento = date.fromisoformat(datos["e"])
        except (InvalidToken, UnicodeError, ValueError, KeyError, TypeError, json.JSONDecodeError) as error:
            raise ErrorCodigoQrCarnet("Código QR inválido") from error
        if datos.get("v") != 1 or datos.get("i") != INSTITUCION or not isinstance(datos.get("p"), int):
            raise ErrorCodigoQrCarnet("Código QR inválido")
        if hoy > vencimiento:
            raise ErrorCodigoQrCarnet("Código QR vencido")
        return datos["p"]
