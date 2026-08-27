"""Cálculos temporales transversales de la aplicación."""

from collections.abc import Callable
from datetime import date, datetime
from zoneinfo import ZoneInfo


def fecha_local(zona_horaria: str, reloj: Callable[[], datetime] | None = None) -> date:
    """Devuelve la fecha civil de la zona configurada, no la del contenedor."""
    zona = ZoneInfo(zona_horaria)
    ahora = reloj() if reloj else datetime.now(zona)
    return ahora.astimezone(zona).date()
