"""Formato canónico y agrupación de secciones académicas."""

import re

PATRON_SECCION = re.compile(r"^(7|8|9|10|11|12)\s*-\s*([1-9]\d*)$")
ETIQUETAS_GRADO = {
    7: "Séptimo",
    8: "Octavo",
    9: "Noveno",
    10: "Décimo",
    11: "Undécimo",
    12: "Duodécimo",
}


def normalizar_seccion(valor: str | None) -> str | None:
    if valor is None or not valor.strip():
        return None
    coincidencia = PATRON_SECCION.fullmatch(valor.strip())
    if coincidencia is None:
        raise ValueError("La sección debe usar el formato 7-1 a 12-n")
    return f"{coincidencia.group(1)}-{int(coincidencia.group(2))}"


def agrupar_secciones_activas(secciones: list[str]) -> list[dict]:
    grupos: dict[int, set[int]] = {}
    no_clasificadas: set[str] = set()
    for seccion in secciones:
        coincidencia = PATRON_SECCION.fullmatch(seccion.strip())
        if coincidencia is None:
            no_clasificadas.add(seccion)
            continue
        grado, grupo = int(coincidencia.group(1)), int(coincidencia.group(2))
        grupos.setdefault(grado, set()).add(grupo)
    resultado = [
        {
            "grado": grado,
            "etiqueta": ETIQUETAS_GRADO[grado],
            "secciones": [f"{grado}-{grupo}" for grupo in sorted(numeros)],
        }
        for grado, numeros in sorted(grupos.items())
    ]
    if no_clasificadas:
        resultado.append(
            {"grado": None, "etiqueta": "Otras secciones", "secciones": sorted(no_clasificadas)}
        )
    return resultado
