"""Colores permitidos para identificar rutas en la interfaz web."""

import re

COLORES_RUTA = {
    "5369": {"nombre": "Rojo coral", "hex": "#EF4444"},
    "5370": {"nombre": "Rosa moderno", "hex": "#F472B6"},
    "5371": {"nombre": "Magenta", "hex": "#D946EF"},
    "1115306": {"nombre": "Ámbar", "hex": "#F59E0B"},
    "1115307": {"nombre": "Amarillo cálido", "hex": "#FACC15"},
    "1115308": {"nombre": "Azul cielo", "hex": "#38BDF8"},
    "1115309": {"nombre": "Coral", "hex": "#FB8C6A"},
    "1115311": {"nombre": "Lavanda", "hex": "#A78BFA"},
    "1115336": {"nombre": "Verde", "hex": "#4ADE80"},
    "EDUCACION_ESPECIAL": {"nombre": "Blanco", "hex": "#FFFFFF"},
}
_COLOR_HEX = re.compile(r"^#[0-9A-F]{6}$")


def opciones() -> list[dict]:
    return [{"clave": clave, **valor} for clave, valor in COLORES_RUTA.items()]


def validar(valor: str) -> str:
    normalizado = (valor or "").strip().upper()
    if not _COLOR_HEX.fullmatch(normalizado):
        raise ValueError("El color debe tener formato HEX, por ejemplo #38BDF8")
    return normalizado
