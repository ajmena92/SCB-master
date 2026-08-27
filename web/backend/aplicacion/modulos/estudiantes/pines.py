"""Reglas puras para generar y presentar PIN de estudiantes."""

from __future__ import annotations

import secrets
from typing import Any

from .esquemas import PinGenerado


def generar_pin() -> str:
    """Genera un PIN numérico de seis dígitos."""
    return f"{secrets.randbelow(1_000_000):06d}"


def seleccionar_estudiantes(
    estudiantes: list[dict[str, Any]], seccion: str | None, turno: str | None
) -> list[dict[str, Any]]:
    """Filtra estudiantes activos según sección y turno solicitados."""
    return [
        estudiante
        for estudiante in estudiantes
        if (
            (seccion is None and not estudiante.get("seccion"))
            or estudiante.get("seccion") == seccion
        )
        and (not turno or estudiante.get("turno") == turno)
    ]


def construir_filas(
    estudiantes: list[dict[str, Any]], generados: list[PinGenerado], seccion: str | None
) -> list[dict[str, Any]]:
    """Construye las filas camelCase del reporte de PIN."""
    filas: list[dict[str, Any]] = []
    for estudiante, pin in zip(estudiantes, generados):
        nombre = " ".join(
            str(estudiante.get(campo, "") or "")
            for campo in ("nombre", "primer_apellido", "segundo_apellido")
        ).strip()
        filas.append(
            {
                "idEstudiante": pin.id_estudiante,
                "nombreCompleto": nombre,
                "cedula": estudiante.get("cedula", ""),
                "horario": estudiante.get("turno", ""),
                "seccion": estudiante.get("seccion", seccion),
                "pin": pin.pin,
            }
        )
    return filas
