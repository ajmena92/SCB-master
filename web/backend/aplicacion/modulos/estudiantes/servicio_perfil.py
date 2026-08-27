"""Proyección del perfil estudiantil para la sesión web."""

from collections.abc import Callable
from typing import Any


def crear_perfil_sesion(
    id_estudiante: int, buscar_estudiante: Callable[[int], dict[str, Any] | None]
) -> dict[str, object]:
    """Construye únicamente la representación pública del estudiante autenticado."""

    datos = buscar_estudiante(id_estudiante) or {}
    return {
        "idEstudiante": id_estudiante,
        "carne": datos.get("carne"),
        "nombre": datos.get("nombre"),
        "nombreCompleto": " ".join(
            str(datos.get(c) or "") for c in ("nombre", "primer_apellido", "segundo_apellido")
        ).strip(),
        "tieneFoto": bool(datos.get("tiene_foto", False)),
        "debeCambiarPin": bool(datos.get("debe_cambiar_pin", False)),
    }
