"""Abstraccion de tiempo compartida por los modulos."""

from datetime import datetime, timezone
from typing import Protocol


class Reloj(Protocol):
    def ahora_utc(self) -> datetime:
        """Devuelve un instante consciente de zona horaria en UTC."""


class RelojSistema:
    def ahora_utc(self) -> datetime:
        return datetime.now(timezone.utc)
