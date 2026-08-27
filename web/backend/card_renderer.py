"""Renderiza carnets sin exponer fotografías ni depender de un servicio externo."""

from __future__ import annotations

import re
from io import BytesIO
from pathlib import Path
from typing import Optional

from PIL import Image, ImageDraw, ImageFont, ImageOps

CARD_WIDTH = 900
CARD_HEIGHT = 1400
MAX_PHOTO_BYTES = 5 * 1024 * 1024
ALLOWED_MIME = {"image/jpeg", "image/png", "image/webp"}


def _safe_color(value: object, fallback: str = "#CBD5E1") -> str:
    return (
        value.upper()
        if isinstance(value, str) and re.fullmatch(r"#[0-9a-fA-F]{6}", value)
        else fallback
    )


def _contrast_text(color: str) -> str:
    red, green, blue = (int(color[index : index + 2], 16) for index in (1, 3, 5))
    return "#202522" if (red * 299 + green * 587 + blue * 114) / 1000 > 160 else "white"


def _font(size: int, bold: bool = False):
    candidates = (
        "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf"
        if bold
        else "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
    )
    for path in candidates:
        try:
            return ImageFont.truetype(path, size)
        except OSError:
            continue
    return ImageFont.load_default()


def _code128_b(value: str) -> list[int]:
    """Return Code 128-B symbol values for ASCII text."""
    if not value or any(ord(char) < 32 or ord(char) > 126 for char in value):
        raise ValueError("El código de barras solo admite texto ASCII imprimible")
    symbols = [104] + [ord(char) - 32 for char in value]
    checksum = symbols[0] + sum(index * symbol for index, symbol in enumerate(symbols[1:], 1))
    return symbols + [checksum % 103, 106]


_CODE128_PATTERNS = "212222 222122 222221 121223 121322 131222 122213 122312 132212 221213 221312 231212 112232 122132 122231 113222 123122 123221 223211 221132 221231 213212 223112 312131 311222 321122 321221 312212 322112 322211 212123 212321 232121 111323 131123 131321 112313 132113 132311 211313 231113 231311 112133 112331 132131 113123 113321 133121 313121 211331 231131 213113 213311 213131 311123 311321 331121 312113 312311 332111 314111 221411 431111 111224 111422 121124 121421 141122 141221 112214 112412 122114 122411 142112 142211 241211 221114 413111 241112 134111 111242 121142 121241 114212 124112 124211 411212 421112 421211 212141 214121 412121 111143 111341 131141 114113 114311 411113 411311 113141 114131 311141 411131 211412 211214 211232 2331112".split()


def _barcode(value: str, width: int = 780, height: int = 180) -> Image.Image:
    symbols = _code128_b(value)
    modules = sum(sum(int(part) for part in _CODE128_PATTERNS[symbol]) for symbol in symbols)
    scale = max(1, width // modules)
    rendered_width = modules * scale
    image = Image.new("RGB", (rendered_width + 24, height), "white")
    draw = ImageDraw.Draw(image)
    x = 12
    for symbol in symbols:
        black = True
        for part in _CODE128_PATTERNS[symbol]:
            size = int(part) * scale
            if black:
                draw.rectangle((x, 0, x + size - 1, height), fill="black")
            x += size
            black = not black
    return image


def _fit_photo(photo: Optional[bytes], size: tuple[int, int]) -> Image.Image:
    if not photo:
        return Image.new("RGB", size, "#f2e6dc")
    try:
        source = Image.open(BytesIO(photo)).convert("RGB")
        return ImageOps.fit(source, size, method=Image.Resampling.LANCZOS)
    except Exception:
        return Image.new("RGB", size, "#f2e6dc")


def render_card(student: dict, photo: Optional[bytes], barcode_value: str, output: str) -> bytes:
    image = Image.new("RGB", (CARD_WIDTH, CARD_HEIGHT), "#fbf8f3")
    draw = ImageDraw.Draw(image)
    neutral, orange, ink, muted = "#475569", "#dc5b36", "#202522", "#5d665f"
    route_color = _safe_color(student.get("RutaColor"), "#CBD5E1")
    header_text = _contrast_text(route_color)
    draw.rectangle((0, 0, CARD_WIDTH, 220), fill=route_color)
    draw.ellipse((CARD_WIDTH - 210, -150, CARD_WIDTH + 110, 170), fill=route_color)
    logo_path = Path(__file__).with_name("assets") / "escudo-ctp-platanares.png"
    if logo_path.exists():
        logo = Image.open(logo_path).convert("RGBA")
        logo.thumbnail((110, 110), Image.Resampling.LANCZOS)
        image.paste(logo, (72, 48), logo)
        title_x = 204
    else:
        title_x = 72
    draw.text((title_x, 60), "COMEDOR SCSC", font=_font(46, True), fill=header_text)
    draw.text((title_x, 126), "CARNET DIGITAL ESTUDIANTIL", font=_font(24, True), fill=header_text)

    photo_box = (72, 280, 388, 596)
    image.paste(_fit_photo(photo, (316, 316)), (72, 280))
    draw.rectangle(photo_box, outline=neutral, width=5)
    if not photo:
        draw.multiline_text(
            (104, 394), "FOTO\nPENDIENTE", font=_font(32, True), fill=neutral, align="center"
        )

    name = " ".join(
        filter(
            None,
            [student.get("Nombre"), student.get("PrimerApellido"), student.get("SegundoApellido")],
        )
    )
    draw.text((450, 300), "ESTUDIANTE", font=_font(22, True), fill=orange)
    name_font = _font(35, True)
    name_words = (name or "Sin nombre").split()
    name_lines, line = [], ""
    for word in name_words:
        candidate = f"{line} {word}".strip()
        if draw.textbbox((0, 0), candidate, font=name_font)[2] > 380 and line:
            name_lines.append(line)
            line = word
        else:
            line = candidate
    if line:
        name_lines.append(line)
    draw.multiline_text((450, 346), "\n".join(name_lines[:3]), font=name_font, fill=ink, spacing=8)
    draw.text(
        (450, 485),
        f"CARNÉ / CÉDULA  {student.get('Cedula') or 'Pendiente'}",
        font=_font(25, True),
        fill=muted,
    )
    draw.text(
        (450, 536),
        f"SECCIÓN  {student.get('Seccion') or 'Sin sección'}",
        font=_font(25, True),
        fill=muted,
    )
    route_name = student.get("RutaDescripcion") or student.get("RutaCodigo") or "Sin ruta"
    draw.text((450, 585), f"RUTA  {route_name}", font=_font(25, True), fill=route_color)

    draw.rounded_rectangle((72, 660, 828, 790), radius=28, fill="#f1e1d7")
    draw.text((108, 692), "BENEFICIO DE COMEDOR", font=_font(22, True), fill=orange)
    draw.text(
        (108, 735),
        str(student.get("TipoBecaDescripcion") or "Sin beca"),
        font=_font(32, True),
        fill=route_color,
    )

    barcode = _barcode(barcode_value, 720, 168)
    image.paste(barcode, (90, 875))
    draw.text(
        (CARD_WIDTH // 2, 1158),
        "Presentá este código ante el lector del comedor",
        font=_font(25),
        fill=neutral,
        anchor="ma",
    )
    draw.text(
        (CARD_WIDTH // 2, 1258),
        "Carnet provisional" if not photo else "Documento institucional",
        font=_font(22, True),
        fill=orange,
        anchor="ma",
    )
    draw.text((CARD_WIDTH // 2, 1310), "Año lectivo 2026", font=_font(20), fill=muted, anchor="ma")

    buffer = BytesIO()
    image.save(buffer, format=output, resolution=150.0)
    return buffer.getvalue()
