import importlib.util
from pathlib import Path

import pytest


def cargar_generador():
    ruta = Path(__file__).parents[2] / "scripts" / "generar_cliente_openapi.py"
    especificacion = importlib.util.spec_from_file_location("generador_cliente_openapi", ruta)
    assert especificacion and especificacion.loader
    modulo = importlib.util.module_from_spec(especificacion)
    especificacion.loader.exec_module(modulo)
    return modulo


def test_generador_separa_esquemas_y_operaciones_por_dominio() -> None:
    generador = cargar_generador()
    openapi = {
        "components": {
            "schemas": {
                "EstudianteSalida": {
                    "type": "object",
                    "properties": {"nombre": {"type": "string"}},
                    "required": ["nombre"],
                },
                "PaginaEstudiantes": {
                    "type": "object",
                    "properties": {
                        "elementos": {
                            "type": "array",
                            "items": {"$ref": "#/components/schemas/EstudianteSalida"},
                        }
                    },
                },
            }
        },
        "paths": {
            "/api/v1/estudiantes": {
                "get": {"operationId": "listar_estudiantes"}
            },
            "/api/v1/menu/plantillas": {
                "get": {"operationId": "listar_menu"}
            },
        },
    }

    salidas = generador._generar(openapi)
    estudiantes = salidas[generador.DIRECTORIO_SALIDA / "estudiantes.ts"]
    operaciones = salidas[generador.DIRECTORIO_SALIDA / "operaciones.ts"]
    operaciones_estudiantes = salidas[
        generador.DIRECTORIO_OPERACIONES / "estudiantes.ts"
    ]
    indice = salidas[generador.SALIDA_INDICE]

    assert "export interface EstudianteSalida" in estudiantes
    assert "export interface PaginaEstudiantes" in estudiantes
    assert "./estudiantes" not in estudiantes
    assert "...OPERACIONES_ESTUDIANTES" in operaciones
    assert 'dominio: "estudiantes"' in operaciones_estudiantes
    assert 'dominio: "menu"' in salidas[generador.DIRECTORIO_OPERACIONES / "menu.ts"]
    assert 'export * from "./estudiantes";' in indice
    assert 'export * from "./operaciones";' in indice


def test_generador_exige_propietario_para_nuevos_esquemas() -> None:
    generador = cargar_generador()
    with pytest.raises(RuntimeError, match="Esquemas sin dominio asignado"):
        generador._generar({"components": {"schemas": {"EsquemaNuevo": {}}}, "paths": {}})
