"""Lectura acotada de archivos recibidos por la API."""

from collections.abc import Awaitable
from typing import Protocol

TAMANO_BLOQUE_ARCHIVO = 64 * 1024


class ArchivoSubido(Protocol):
    def read(self, size: int = -1) -> Awaitable[bytes]: ...


class ArchivoExcedeLimite(ValueError):
    """Indica que una carga supera el límite de su funcionalidad."""


async def leer_archivo_limitado(archivo: ArchivoSubido, limite_bytes: int) -> bytes:
    """Lee por bloques y rechaza la carga antes de conservar bytes adicionales."""
    if limite_bytes <= 0:
        raise ValueError("El límite del archivo debe ser positivo")

    contenido = bytearray()
    while True:
        restante = limite_bytes - len(contenido)
        bloque = await archivo.read(min(TAMANO_BLOQUE_ARCHIVO, restante + 1))
        if not bloque:
            return bytes(contenido)
        if len(bloque) > restante:
            raise ArchivoExcedeLimite("El archivo supera el tamaño máximo permitido")
        contenido.extend(bloque)
