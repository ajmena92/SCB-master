"""Indicadores históricos para la revisión operativa del comedor.

Este módulo se ejecuta en la tarea analítica, no durante el arranque del API.
"""

from __future__ import annotations

from typing import Any

import pandas as pd


def proyectar_asistencia(
    marcas: pd.DataFrame,
    estudiantes: pd.DataFrame,
    consumos: pd.DataFrame | None = None,
) -> list[dict[str, Any]]:
    """Devuelve señales individuales con una muestra histórica suficiente.

    ``marcas`` requiere ``id_estudiante``, ``fecha`` y ``estado``.
    ``estudiantes`` requiere ``id_estudiante`` y ``estado_comedor``.
    ``consumos`` puede incluir ``modalidad`` (`beca` o `tiquete`); sin ese campo
    no se infieren compras a partir de conceptos de texto libre.
    """
    requeridas = {"id_estudiante", "fecha", "estado"}
    faltantes = requeridas - set(marcas.columns)
    if faltantes:
        raise ValueError(f"Faltan columnas de marcas: {', '.join(sorted(faltantes))}")
    if "id_estudiante" not in estudiantes.columns:
        raise ValueError("Falta la columna id_estudiante de estudiantes")

    historial = marcas.copy()
    historial["fecha"] = pd.to_datetime(historial["fecha"])
    historial = historial[historial["fecha"].dt.weekday < 5]
    historial["presente"] = historial["estado"].isin(["presente", "confirmada"])
    resumen = historial.groupby("id_estudiante", as_index=False).agg(
        dias_observados=("fecha", "nunique"),
        dias_presentes=("presente", "sum"),
        ultima_fecha=("fecha", "max"),
    )
    resumen["porcentaje_asistencia"] = (
        resumen["dias_presentes"] / resumen["dias_observados"] * 100
    ).round(1)
    resultado = estudiantes.merge(resumen, on="id_estudiante", how="left")
    resultado["dias_observados"] = resultado["dias_observados"].fillna(0).astype(int)
    resultado["dias_presentes"] = resultado["dias_presentes"].fillna(0).astype(int)
    resultado["porcentaje_asistencia"] = resultado["porcentaje_asistencia"].fillna(0)
    if "estado_comedor" not in resultado.columns:
        raise ValueError("Falta la columna canónica estado_comedor de estudiantes")
    resultado["becado"] = resultado["estado_comedor"].eq("becado_comedor")
    resultado["senal"] = "sin datos suficientes"
    muestra = resultado["dias_observados"] >= 3
    resultado.loc[muestra & resultado["becado"] & (resultado["porcentaje_asistencia"] < 50), "senal"] = "becado con baja asistencia"
    resultado["consumos_comedor"] = 0

    if consumos is not None and "id_estudiante" in consumos.columns:
        consumo_total = consumos.groupby("id_estudiante").size().rename("consumos_comedor")
        resultado["consumos_comedor"] = (
            resultado["id_estudiante"].map(consumo_total).fillna(0).astype(int)
        )
        sin_consumo = muestra & resultado["becado"] & (resultado["consumos_comedor"] == 0)
        resultado.loc[sin_consumo, "senal"] = "becado sin consumo reciente"

    if consumos is not None and {"id_estudiante", "modalidad"}.issubset(consumos.columns):
        tiquetes = consumos[consumos["modalidad"].eq("tiquete")]
        compradores = tiquetes.groupby("id_estudiante").size().rename("consumos_tiquete")
        resultado["consumos_tiquete"] = (
            resultado["id_estudiante"].map(compradores).fillna(0).astype(int)
        )
        candidatos = muestra & ~resultado["becado"] & (resultado["consumos_tiquete"] >= 3)
        resultado.loc[candidatos, "senal"] = "candidato para revisión de beca"
    else:
        resultado["consumos_tiquete"] = 0

    columnas = [
        "id_estudiante", "dias_observados", "dias_presentes", "porcentaje_asistencia",
        "ultima_fecha", "becado", "consumos_comedor", "consumos_tiquete", "senal",
    ]
    return resultado[columnas].to_dict(orient="records")
