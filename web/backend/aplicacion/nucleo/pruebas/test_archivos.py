import asyncio

import pytest

from aplicacion.nucleo.archivos import (
    TAMANO_BLOQUE_ARCHIVO,
    ArchivoExcedeLimite,
    leer_archivo_limitado,
)


class ArchivoFalso:
    def __init__(self, contenido: bytes) -> None:
        self._contenido = contenido
        self._posicion = 0
        self.tamanos_solicitados: list[int] = []

    async def read(self, size: int = -1) -> bytes:
        self.tamanos_solicitados.append(size)
        if self._posicion >= len(self._contenido):
            return b""
        siguiente = len(self._contenido) if size < 0 else self._posicion + size
        bloque = self._contenido[self._posicion : siguiente]
        self._posicion += len(bloque)
        return bloque


def test_lee_archivo_por_bloques_y_conserva_el_contenido() -> None:
    archivo = ArchivoFalso(b"a" * (TAMANO_BLOQUE_ARCHIVO * 2 + 3))

    resultado = asyncio.run(leer_archivo_limitado(archivo, len(archivo._contenido)))

    assert resultado == archivo._contenido
    assert max(archivo.tamanos_solicitados) <= TAMANO_BLOQUE_ARCHIVO
    assert len(archivo.tamanos_solicitados) > 2


def test_rechaza_archivo_antes_de_conservar_bytes_fuera_del_limite() -> None:
    archivo = ArchivoFalso(b"a" * (TAMANO_BLOQUE_ARCHIVO + 1))

    with pytest.raises(ArchivoExcedeLimite):
        asyncio.run(leer_archivo_limitado(archivo, TAMANO_BLOQUE_ARCHIVO))

    assert archivo._posicion == TAMANO_BLOQUE_ARCHIVO + 1
