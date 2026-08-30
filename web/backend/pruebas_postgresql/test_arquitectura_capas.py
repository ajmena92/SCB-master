import ast
from pathlib import Path


def test_adaptadores_y_casos_de_uso_no_ejecutan_sql():
    raiz = Path(__file__).parents[1] / "aplicacion"
    archivos = [*raiz.glob("api_*.py"), *raiz.glob("casos_*.py"), raiz / "servicios.py"]
    llamadas_sql = {"execute", "exec", "scalar", "scalars", "query", "select", "text"}
    hallazgos = []

    for archivo in archivos:
        arbol = ast.parse(archivo.read_text(encoding="utf-8"), filename=str(archivo))
        for nodo in ast.walk(arbol):
            if not isinstance(nodo, ast.Call):
                continue
            nombre = (
                nodo.func.attr
                if isinstance(nodo.func, ast.Attribute)
                else getattr(nodo.func, "id", "")
            )
            if nombre in llamadas_sql:
                hallazgos.append(f"{archivo.name}:{nodo.lineno}:{nombre}")

    assert not hallazgos, "SQL fuera de repositorios: " + ", ".join(hallazgos)
