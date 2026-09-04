"""Base común para contratos HTTP con aliases camelCase."""

from pydantic import BaseModel, ConfigDict


def _camel(nombre: str) -> str:
    partes = nombre.split("_")
    return partes[0] + "".join(parte.title() for parte in partes[1:])


class Contrato(BaseModel):
    model_config = ConfigDict(
        alias_generator=_camel,
        populate_by_name=True,
        from_attributes=True,
        extra="forbid",
    )
