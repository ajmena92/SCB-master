#!/usr/bin/env python3
"""Verifica límites arquitectónicos durante la migración a la plataforma web.

No intenta sustituir un analizador sintáctico. Aplica reglas pequeñas y explícitas
en el código activo y en las rutas objetivo, para evitar bloquear el legado que
todavía no se ha migrado.
"""

from __future__ import annotations

import json
import re
import sys
from collections import namedtuple
from pathlib import Path
from typing import Iterable


Hallazgo = namedtuple("Hallazgo", "regla ruta linea mensaje")
EXTENSIONES_CODIGO = {".py", ".js", ".jsx", ".ts", ".tsx"}
CARPETAS_FRONTEND_OBJETIVO = {"aplicacion", "funcionalidades", "compartido"}
TERMINOS_INGLES_PROPIOS = {
    "app", "apps", "feature", "features", "shared", "common", "core", "module", "modules",
    "identity", "auth", "student", "students", "meal", "meals", "attendance", "marks",
    "transport", "route", "routes", "benefit", "benefits", "account", "accounts", "balance",
    "balances", "topup", "topups", "report", "reports", "audit", "repository", "service", "schema",
}
PATRON_SQL = re.compile(
    r"\b(?:select|insert|update|delete|merge|create\s+table|alter\s+table|drop\s+table)\b|\.execute\s*\(",
    re.IGNORECASE,
)
PATRON_HTTP = re.compile(r"\b(?:fetch\s*\(|axios(?:\.[A-Za-z]+|\s*\())")
PATRON_ESCRITORIO = re.compile(r"\bescritorio\b", re.IGNORECASE)
PATRON_FRAGMENTO_IDENTIFICADOR = re.compile(
    r"[A-Z]+(?=[A-Z][a-z]|[^A-Za-z]|$)|[A-Z]?[a-z]+"
)


def rutas_codigo(raiz: Path, directorio: Path) -> Iterable[Path]:
    if not directorio.exists():
        return []
    return (ruta for ruta in directorio.rglob("*") if ruta.is_file() and ruta.suffix in EXTENSIONES_CODIGO)


def ruta_relativa(raiz: Path, ruta: Path) -> str:
    return ruta.relative_to(raiz).as_posix()


def lineas_con_patron(ruta: Path, patron: re.Pattern[str]) -> list[int]:
    try:
        return [numero for numero, linea in enumerate(ruta.read_text(encoding="utf-8").splitlines(), 1) if patron.search(linea)]
    except (OSError, UnicodeDecodeError):
        return []


def contiene_termino_ingles_propio(texto: str, terminos: set[str]) -> bool:
    """Busca términos completos, incluso en identificadores snake_case y camelCase."""
    return any(fragmento.lower() in terminos for fragmento in PATRON_FRAGMENTO_IDENTIFICADOR.findall(texto))


def buscar_referencias_escritorio(raiz: Path) -> list[Hallazgo]:
    objetivos = [raiz / "backend" / "aplicacion"]
    objetivos.extend(raiz / "frontend" / "src" / nombre for nombre in CARPETAS_FRONTEND_OBJETIVO)
    hallazgos: list[Hallazgo] = []
    for objetivo in objetivos:
        for ruta in rutas_codigo(raiz, objetivo):
            for linea in lineas_con_patron(ruta, PATRON_ESCRITORIO):
                hallazgos.append(Hallazgo("referencia-a-escritorio", ruta_relativa(raiz, ruta), linea, "Referencia de ejecución prohibida hacia escritorio."))
    return hallazgos


def buscar_sql_fuera_de_repositorios(raiz: Path) -> list[Hallazgo]:
    hallazgos: list[Hallazgo] = []
    for ruta in rutas_codigo(raiz, raiz / "backend" / "aplicacion"):
        if ruta.name == "repositorio.py" or ruta.parent.name == "pruebas":
            continue
        for linea in lineas_con_patron(ruta, PATRON_SQL):
            hallazgos.append(Hallazgo("sql-fuera-de-repositorio", ruta_relativa(raiz, ruta), linea, "SQL o execute fuera de repositorio.py."))
    return hallazgos


def es_componente(ruta: Path) -> bool:
    return bool({"componentes", "components"}.intersection(ruta.parts)) and not any(parte in {"ui", "__tests__"} for parte in ruta.parts) and ".test." not in ruta.name


def buscar_http_directo_en_componentes(raiz: Path) -> list[Hallazgo]:
    hallazgos: list[Hallazgo] = []
    for ruta in rutas_codigo(raiz, raiz / "frontend" / "src"):
        if not es_componente(ruta):
            continue
        for linea in lineas_con_patron(ruta, PATRON_HTTP):
            hallazgos.append(Hallazgo("http-directo-en-componente", ruta_relativa(raiz, ruta), linea, "fetch o axios directo en componente React."))
    return hallazgos


def buscar_archivos_largos(raiz: Path, exentos: set[str]) -> list[Hallazgo]:
    hallazgos: list[Hallazgo] = []
    for objetivo in (raiz / "frontend" / "src", raiz / "backend" / "aplicacion"):
        for ruta in rutas_codigo(raiz, objetivo):
            relativa = ruta_relativa(raiz, ruta)
            try:
                total = len(ruta.read_text(encoding="utf-8").splitlines())
            except (OSError, UnicodeDecodeError):
                continue
            if total > 300 and relativa not in exentos:
                hallazgos.append(Hallazgo("archivo-mayor-a-300-lineas", relativa, total, "Archivo de código mayor a 300 líneas sin excepción documentada."))
    return hallazgos


def buscar_ingles_no_permitido(raiz: Path, permitidos: set[str]) -> list[Hallazgo]:
    terminos = TERMINOS_INGLES_PROPIOS - {termino.lower() for termino in permitidos}
    if not terminos:
        return []
    objetivos = [raiz / "backend" / "aplicacion"]
    objetivos.extend(raiz / "frontend" / "src" / nombre for nombre in CARPETAS_FRONTEND_OBJETIVO)
    hallazgos: list[Hallazgo] = []
    for objetivo in objetivos:
        for ruta in rutas_codigo(raiz, objetivo):
            relativa = ruta_relativa(raiz, ruta)
            if contiene_termino_ingles_propio(relativa, terminos):
                hallazgos.append(Hallazgo("vocabulario-en-ingles", relativa, 0, "Término propio en inglés en la ruta."))
            try:
                lineas = ruta.read_text(encoding="utf-8").splitlines()
            except (OSError, UnicodeDecodeError):
                continue
            for numero, linea in enumerate(lineas, 1):
                if not contiene_termino_ingles_propio(linea, terminos):
                    continue
                hallazgos.append(Hallazgo("vocabulario-en-ingles", relativa, numero, "Término propio en inglés fuera de la lista permitida."))
    return hallazgos


def cargar_configuracion(raiz: Path) -> dict[str, list[str]]:
    ruta = raiz / "scripts" / "configuracion_verificadores.json"
    with ruta.open(encoding="utf-8") as archivo:
        return json.load(archivo)


def main() -> int:
    raiz = Path(__file__).resolve().parents[1]
    configuracion = cargar_configuracion(raiz)
    hallazgos = [
        *buscar_referencias_escritorio(raiz),
        *buscar_sql_fuera_de_repositorios(raiz),
        *buscar_http_directo_en_componentes(raiz),
        *buscar_archivos_largos(raiz, set(configuracion["archivos_largos_exentos"])),
        *buscar_ingles_no_permitido(raiz, set(configuracion["terminos_ingleses_propios_permitidos"])),
    ]
    if not hallazgos:
        print("Verificaciones arquitectónicas aprobadas.")
        return 0
    for hallazgo in hallazgos:
        ubicacion = f"{hallazgo.ruta}:{hallazgo.linea}" if hallazgo.linea else hallazgo.ruta
        print(f"[{hallazgo.regla}] {ubicacion}: {hallazgo.mensaje}")
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
