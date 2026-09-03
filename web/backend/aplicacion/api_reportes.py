"""Reportes operativos JSON/CSV."""

import csv
import io
from datetime import date
from typing import Annotated

from fastapi import APIRouter, Depends, Query, Response


def _respuesta(filas: list[dict], formato: str):
    if formato != "csv":
        return filas
    salida = io.StringIO()
    if filas:
        escritor = csv.DictWriter(salida, fieldnames=filas[0].keys())
        escritor.writeheader()
        escritor.writerows(filas)
    return Response(salida.getvalue(), media_type="text/csv; charset=utf-8")


def crear_router(obtener_servicio, exigir_permiso) -> APIRouter:
    router = APIRouter(prefix="/reportes")

    @router.get("/comedor", dependencies=[Depends(exigir_permiso("reportes.leer"))])
    async def comedor(
        desde: date, hasta: date, formato: str = "json", servicio=Depends(obtener_servicio)
    ):
        registros = servicio.comedor(desde, hasta)
        return _respuesta(
            [
                {
                    "fecha": r.fecha,
                    "cedula": r.cedula,
                    "nombres": r.nombres,
                    "modalidad": r.modalidad,
                    "consumioTiquete": r.consumio_tiquete,
                }
                for r in registros
            ],
            formato,
        )

    @router.get("/transporte", dependencies=[Depends(exigir_permiso("reportes.leer"))])
    async def transporte(
        desde: date, hasta: date, formato: str = "json", servicio=Depends(obtener_servicio)
    ):
        registros = servicio.transporte(desde, hasta)
        return _respuesta(
            [
                {"fecha": r.fecha, "matriculaId": r.matricula_id, "ruta": r.nombre}
                for r in registros
            ],
            formato,
        )

    @router.get("/ventas", dependencies=[Depends(exigir_permiso("reportes.leer"))])
    async def ventas(
        desde: date, hasta: date, formato: str = "json", servicio=Depends(obtener_servicio)
    ):
        registros = servicio.ventas(desde, hasta)
        return _respuesta(
            [
                {
                    "fecha": r.creado_en,
                    "cedula": r.cedula,
                    "cantidad": r.cantidad,
                    "tarifa": r.tarifa_aplicada,
                    "total": r.total,
                    "medioPago": r.medio_pago,
                }
                for r in registros
            ],
            formato,
        )

    @router.get("/dashboard", dependencies=[Depends(exigir_permiso("dashboard.leer"))])
    async def dashboard(
        fecha: date,
        tipo_persona: Annotated[str, Query(alias="tipoPersona")] = "estudiante",
        busqueda: str = "",
        ruta: str = "",
        seccion: str = "",
        estado: str = "",
        beneficio_transporte: Annotated[str, Query(alias="beneficioTransporte")] = "",
        pagina: int = 1,
        por_pagina: Annotated[int, Query(alias="porPagina")] = 25,
        servicio=Depends(obtener_servicio),
    ):
        return servicio.dashboard(
            fecha,
            {
                "tipoPersona": tipo_persona,
                "busqueda": busqueda,
                "ruta": ruta,
                "seccion": seccion,
                "estado": estado,
                "beneficioTransporte": beneficio_transporte,
                "pagina": pagina,
                "porPagina": por_pagina,
            },
        )

    return router
