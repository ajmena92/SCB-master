"""Reportes operativos JSON/CSV."""

import csv
import io
from datetime import date

from fastapi import APIRouter, Depends, Response


def _respuesta(filas: list[dict], formato: str):
    if formato != "csv":
        return filas
    salida = io.StringIO()
    if filas:
        escritor = csv.DictWriter(salida, fieldnames=filas[0].keys())
        escritor.writeheader()
        escritor.writerows(filas)
    return Response(salida.getvalue(), media_type="text/csv; charset=utf-8")


def crear_router(obtener_servicio, administrativo) -> APIRouter:
    router = APIRouter(prefix="/reportes", dependencies=[Depends(administrativo)])

    @router.get("/comedor")
    async def comedor(
        desde: date, hasta: date, formato: str = "json", servicio=Depends(obtener_servicio)
    ):
        registros = servicio.comedor(desde, hasta)
        return _respuesta(
            [
                {
                    "fecha": r.fecha,
                    "codigo": r.codigo,
                    "nombres": r.nombres,
                    "modalidad": r.modalidad,
                    "consumioTiquete": r.consumio_tiquete,
                }
                for r in registros
            ],
            formato,
        )

    @router.get("/transporte")
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

    @router.get("/ventas")
    async def ventas(
        desde: date, hasta: date, formato: str = "json", servicio=Depends(obtener_servicio)
    ):
        registros = servicio.ventas(desde, hasta)
        return _respuesta(
            [
                {
                    "fecha": r.creado_en,
                    "codigo": r.codigo,
                    "cantidad": r.cantidad,
                    "tarifa": r.tarifa_aplicada,
                    "total": r.total,
                    "medioPago": r.medio_pago,
                }
                for r in registros
            ],
            formato,
        )

    return router
