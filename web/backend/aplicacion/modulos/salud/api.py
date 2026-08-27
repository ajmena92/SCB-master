"""Adaptador HTTP del modulo de salud."""

from fastapi import APIRouter, Depends

from aplicacion.modulos.salud.esquemas import EstadoSalud
from aplicacion.modulos.salud.servicio import ServicioSalud
from aplicacion.nucleo.reloj import RelojSistema

enrutador = APIRouter(prefix="/salud", tags=["salud"])


async def obtener_servicio_salud() -> ServicioSalud:
    return ServicioSalud(RelojSistema())


@enrutador.get("", response_model=EstadoSalud)
async def consultar_salud(
    servicio: ServicioSalud = Depends(obtener_servicio_salud),
) -> EstadoSalud:
    return servicio.consultar()
