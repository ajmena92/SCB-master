"""Reglas canónicas de presentación de beneficios estudiantiles."""

from __future__ import annotations

from typing import Any

RUTA_SIN_BENEFICIO = "0000"


def normalizar_beneficio_transporte(datos: dict[str, Any]) -> dict[str, Any]:
    """Deriva el beneficio visible desde una ruta activa y distinta de 0000."""
    normalizados = dict(datos)
    codigo_crudo = normalizados.get("ruta_codigo")
    codigo = str(codigo_crudo).strip() if codigo_crudo is not None else ""
    descripcion_cruda = normalizados.get("ruta_descripcion")
    descripcion = str(descripcion_cruda).strip() if descripcion_cruda is not None else ""
    ruta_activa = bool(normalizados.pop("ruta_activa", False))
    tiene_beneficio = bool(codigo and codigo != RUTA_SIN_BENEFICIO and ruta_activa)

    if tiene_beneficio:
        normalizados["ruta_codigo"] = codigo
        normalizados["ruta_descripcion"] = descripcion
        normalizados["tiene_beneficio_transporte"] = True
        normalizados["beneficio_transporte"] = f"Beneficiario – {descripcion}"
        return normalizados

    normalizados["id_ruta"] = None
    normalizados["ruta_codigo"] = None
    normalizados["ruta_descripcion"] = None
    normalizados["ruta_color"] = None
    normalizados["tiene_beneficio_transporte"] = False
    normalizados["beneficio_transporte"] = "No beneficiario"
    return normalizados
