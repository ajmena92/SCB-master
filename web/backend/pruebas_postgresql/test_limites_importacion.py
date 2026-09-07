"""Rechazos del padrón antes de descompresión y escritura."""

import io
from zipfile import ZIP_DEFLATED, ZipFile

import pytest
from fastapi import HTTPException
from openpyxl import Workbook
from pydantic import ValidationError

from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.limites_importacion import validar_excel


def test_rechaza_zip_expandido_antes_de_openpyxl():
    salida = io.BytesIO()
    with ZipFile(salida, "w", ZIP_DEFLATED) as archivo:
        archivo.writestr("xl/sharedStrings.xml", b"x" * (65 * 1024 * 1024))
    with pytest.raises(HTTPException) as error:
        validar_excel(salida.getvalue())
    assert error.value.status_code == 413


def test_rechaza_exceso_de_archivos():
    salida = io.BytesIO()
    with ZipFile(salida, "w") as archivo:
        for indice in range(257):
            archivo.writestr(str(indice), b"")
    with pytest.raises(HTTPException) as error:
        validar_excel(salida.getvalue())
    assert error.value.status_code == 413


def test_rechaza_excel_con_mas_de_5000_filas():
    libro = Workbook(write_only=True)
    hoja = libro.create_sheet()
    hoja.append(["cedula", "nombres", "tipo"])
    for indice in range(5001):
        hoja.append([str(indice), "Prueba", "profesor"])
    salida = io.BytesIO()
    libro.save(salida)
    with pytest.raises(HTTPException) as error:
        ServicioImportacion(None).desde_excel(salida.getvalue(), 2026)
    assert error.value.status_code == 413


@pytest.mark.parametrize("cantidad", [0, 5001])
def test_json_respeta_limite_de_filas(cantidad):
    with pytest.raises(ValidationError):
        ImportacionEntrada(anio=2026, filas=[
            {"cedula": "prueba", "nombres": "Prueba", "tipo": "profesor"}
        ] * cantidad)
