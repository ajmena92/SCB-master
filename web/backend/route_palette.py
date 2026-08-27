"""Default palette and validation for CTP Platanares transport routes."""

import re

RUTA_COLORES = {
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

HEX_COLOR = re.compile(r"^#[0-9A-F]{6}$")


def palette_options() -> list[dict]:
    return [{"clave": key, **value} for key, value in RUTA_COLORES.items()]


def validate_route_color(value: str) -> str:
    value = (value or "").strip().upper()
    if not HEX_COLOR.fullmatch(value):
        raise ValueError("El color debe tener formato HEX, por ejemplo #38BDF8")
    return value
