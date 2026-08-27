import asyncio
from datetime import datetime, timezone

import httpx
from fastapi import FastAPI

from aplicacion.composicion import incluir_modulos
from aplicacion.modulos.salud.servicio import ServicioSalud


class RelojFijo:
    def __init__(self, instante: datetime) -> None:
        self._instante = instante

    def ahora_utc(self) -> datetime:
        return self._instante


def test_servicio_salud_entrega_estado_con_hora_utc() -> None:
    instante = datetime(2026, 8, 25, 12, 30, tzinfo=timezone.utc)

    estado = ServicioSalud(RelojFijo(instante)).consultar()

    assert estado.estado == "disponible"
    assert estado.fecha_hora_utc == instante


def test_composicion_publica_modulo_salud_en_api_v1() -> None:
    aplicacion = FastAPI()
    incluir_modulos(aplicacion)

    async def consultar() -> httpx.Response:
        transporte = httpx.ASGITransport(app=aplicacion)
        async with httpx.AsyncClient(transport=transporte, base_url="http://pruebas") as cliente:
            return await cliente.get("/api/v1/salud")

    respuesta = asyncio.run(consultar())

    assert respuesta.status_code == 200
    assert respuesta.json()["estado"] == "disponible"
    assert respuesta.json()["fechaHoraUtc"].endswith("Z")


def test_composicion_es_idempotente() -> None:
    aplicacion = FastAPI()

    incluir_modulos(aplicacion)
    cantidad_rutas = len(aplicacion.routes)
    incluir_modulos(aplicacion)

    assert len(aplicacion.routes) == cantidad_rutas
    assert "/api/v1/salud" in aplicacion.openapi()["paths"]
