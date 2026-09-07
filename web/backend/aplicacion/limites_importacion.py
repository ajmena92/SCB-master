"""Presupuesto de entrada del padrón anual (hasta 5.000 personas)."""

import io
from zipfile import BadZipFile, ZipFile

from fastapi import HTTPException

MAXIMO_FILAS = 5000
MAXIMO_BYTES = 12 * 1024 * 1024
MAXIMO_DESCOMPRIMIDO = 64 * 1024 * 1024
MAXIMO_ARCHIVOS = 256
MAXIMO_COLUMNAS = 32


def validar_excel(contenido: bytes) -> None:
    if len(contenido) > MAXIMO_BYTES:
        raise HTTPException(413, "El archivo supera el límite de 12 MiB")
    try:
        with ZipFile(io.BytesIO(contenido)) as archivo:
            entradas = archivo.infolist()
            if len(entradas) > MAXIMO_ARCHIVOS or sum(
                entrada.file_size for entrada in entradas
            ) > MAXIMO_DESCOMPRIMIDO:
                raise HTTPException(413, "El Excel supera el límite descomprimido de 64 MiB o 256 archivos")
    except BadZipFile as error:
        raise HTTPException(422, "El archivo no es un Excel válido") from error
