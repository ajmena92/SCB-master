"""Normalización segura de fotografías para los carnets web."""

from io import BytesIO

from PIL import Image, ImageOps, UnidentifiedImageError

ANCHO_CARNET = 600
ALTO_CARNET = 800
CALIDAD_JPEG = 84


class FotografiaInvalida(ValueError):
    """El archivo no puede convertirse en una fotografía de carnet."""


def preparar_fotografia(contenido: bytes) -> bytes:
    """Corrige orientación y centra un retrato vertical optimizado para web."""
    try:
        with Image.open(BytesIO(contenido)) as imagen:
            imagen = ImageOps.exif_transpose(imagen).convert("RGB")
            if imagen.width < 120 or imagen.height < 120:
                raise FotografiaInvalida("La fotografía es demasiado pequeña")
            retrato = ImageOps.fit(
                imagen,
                (ANCHO_CARNET, ALTO_CARNET),
                method=Image.Resampling.LANCZOS,
                centering=(0.5, 0.5),
            )
            salida = BytesIO()
            retrato.save(salida, format="JPEG", quality=CALIDAD_JPEG, optimize=True, progressive=True)
            return salida.getvalue()
    except (Image.DecompressionBombError, OSError, UnidentifiedImageError) as exc:
        raise FotografiaInvalida("El archivo no es una imagen válida") from exc
