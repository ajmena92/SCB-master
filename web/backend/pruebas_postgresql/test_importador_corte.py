"""Regresiones del corte único desde SQL Server."""

import importlib.util
import sys
from pathlib import Path

RUTA = Path(__file__).parents[2] / "scripts" / "importar_sqlserver_postgresql.py"
ESPECIFICACION = importlib.util.spec_from_file_location("importador_corte", RUTA)
assert ESPECIFICACION and ESPECIFICACION.loader
MODULO = importlib.util.module_from_spec(ESPECIFICACION)
sys.modules[ESPECIFICACION.name] = MODULO
ESPECIFICACION.loader.exec_module(MODULO)


def test_menu_historico_de_129_caracteres_es_valido() -> None:
    menu = [
        {
            "IdMenuPlantilla": 1,
            "Titulo": "M" * 129,
            "Componente": "C" * 129,
        }
    ]

    reporte = MODULO.validar([], [], menu)

    assert reporte["errores"] == []


def test_menu_mayor_al_contrato_se_rechaza_en_simulacion() -> None:
    menu = [
        {
            "IdMenuPlantilla": 2,
            "Titulo": "M" * 181,
            "Componente": "C" * 181,
        }
    ]

    tipos = {error["tipo"] for error in MODULO.validar([], [], menu)["errores"]}

    assert tipos == {"titulo_menu_muy_largo", "componente_menu_muy_largo"}
