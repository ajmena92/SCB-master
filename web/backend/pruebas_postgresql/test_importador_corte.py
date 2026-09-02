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


def test_sustituciones_validas_se_pueden_recuperar_en_aislamiento() -> None:
    sustituciones = [
        {
            "Fecha": "2026-09-07",
            "Titulo": "Arroz con pollo",
            "Observaciones": "Cambio por disponibilidad.",
            "Componente": "Ensalada verde",
            "TipoComponente": "Acompañamiento",
            "Orden": 1,
        }
    ]

    reporte = MODULO.validar_sustituciones(sustituciones)

    assert reporte == {"sustituciones": 1, "componentes_sustitucion": 1, "errores": []}


def test_sustituciones_con_orden_repetido_se_rechazan() -> None:
    sustituciones = [
        {"Fecha": "2026-09-07", "Titulo": "Cambio", "Componente": "A", "Orden": 1},
        {"Fecha": "2026-09-07", "Titulo": "Cambio", "Componente": "B", "Orden": 1},
    ]

    tipos = {error["tipo"] for error in MODULO.validar_sustituciones(sustituciones)["errores"]}

    assert tipos == {"orden_componente_sustitucion_duplicado"}
