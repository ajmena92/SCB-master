"""Utilidades tipadas compartidas por los repositorios de estudiantes."""

from __future__ import annotations

from typing import cast

from aplicacion.nucleo.base_datos import CursorSql


def fila_desde_cursor(cursor: CursorSql) -> dict | None:
    fila = cursor.fetchone()
    if fila is None:
        return None
    return dict(zip((col[0] for col in cursor.description), fila))


def filas_desde_cursor(cursor: CursorSql) -> list[dict]:
    return [dict(zip((col[0] for col in cursor.description), fila)) for fila in cursor.fetchall()]


def entero(valor: object) -> int:
    return cast(int, valor)
