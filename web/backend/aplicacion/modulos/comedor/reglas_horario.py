"""Reglas puras para la hora límite de marcación del comedor."""

from __future__ import annotations

from datetime import time


def normalizar_horario(valor: str | None) -> str | None:
    """Normaliza el turno del padrón al catálogo operativo web."""
    if not valor:
        return None
    normalizado = valor.strip().lower()
    return normalizado if normalizado in {"diurno", "nocturno"} else None


def esta_dentro_de_hora_limite(hora_actual: time, hora_limite: time) -> bool:
    """Permite marcar hasta la hora límite inclusive."""
    return hora_actual <= hora_limite
