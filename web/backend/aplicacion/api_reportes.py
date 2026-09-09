"""Reportes operativos JSON/CSV."""

import csv
import io
from datetime import date
from typing import Annotated

from fastapi import APIRouter, Depends, Query, Response

from aplicacion.exportaciones_reportes import (
    csv_lista_control,
    filas_exportacion,
    html_lista_control,
    xlsx_lista_control,
)


def _respuesta(filas: list[dict], formato: str):
    if formato != "csv":
        return filas
    salida = io.StringIO()
    if filas:
        escritor = csv.DictWriter(salida, fieldnames=filas[0].keys())
        escritor.writeheader()
        escritor.writerows(filas)
    return Response(salida.getvalue(), media_type="text/csv; charset=utf-8")


def _nombre_archivo(servicio: str, fecha: date, extension: str) -> str:
    return f"lista-control-{servicio}-{fecha.isoformat()}.{extension}"


def _cabecera_descarga(nombre: str) -> dict[str, str]:
    return {"Content-Disposition": f'attachment; filename="{nombre}"'}


def crear_router(obtener_servicio, exigir_permiso) -> APIRouter:
    router = APIRouter(prefix="/reportes")

    @router.get("/comedor", dependencies=[Depends(exigir_permiso("reportes.leer"))])
    def comedor(
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
    def transporte(
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
    def ventas(desde: date, hasta: date, formato: str = "json", servicio=Depends(obtener_servicio)):
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
    def dashboard(
        fecha: date,
        tipo_persona: Annotated[str, Query(alias="tipoPersona")] = "estudiante",
        busqueda: str = "",
        ruta: str = "",
        seccion: str = "",
        estado: str = "",
        servicio_nominal: Annotated[str, Query(alias="servicio")] = "comedor",
        confirmacion: str = "",
        asistencia: str = "",
        beneficio: str = "",
        asignacion: str = "",
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
                "servicio": servicio_nominal,
                "confirmacion": confirmacion,
                "asistencia": asistencia,
                "beneficio": beneficio,
                "asignacion": asignacion,
                "pagina": pagina,
                "porPagina": por_pagina,
            },
        )

    @router.get("/lista-control")
    def lista_control(
        fecha: date,
        servicio_control: Annotated[str, Query(alias="servicio")] = "comedor",
        formato: str = "xlsx",
        busqueda: str = "",
        ruta: str = "",
        seccion: str = "",
        estado: str = "",
        confirmacion: str = "",
        asistencia: str = "",
        beneficio: str = "",
        asignacion: str = "",
        servicio=Depends(obtener_servicio),
        identidad=Depends(exigir_permiso("reportes.leer")),
    ):
        if formato not in {"csv", "xlsx", "pdf"}:
            return Response(status_code=422, content="Formato de exportación no válido")
        try:
            datos = servicio.lista_control(
                fecha,
                servicio_control,
                {
                    "busqueda": busqueda,
                    "ruta": ruta,
                    "seccion": seccion,
                    "estado": estado,
                    "confirmacion": confirmacion,
                    "asistencia": asistencia,
                    "beneficio": beneficio,
                    "asignacion": asignacion,
                },
            )
        except ValueError as error:
            return Response(status_code=422, content=str(error))
        filtros = {
            "busqueda": busqueda,
            "ruta": ruta,
            "seccion": seccion,
            "estado": estado,
            "confirmacion": confirmacion,
            "asistencia": asistencia,
            "beneficio": beneficio,
            "asignacion": asignacion,
        }
        servicio.registrar_exportacion_lista_control(
            identidad["cuenta"].id, servicio_control, formato, fecha, filtros, len(datos)
        )
        institucion = servicio.configuracion_institucional()
        columnas, filas = filas_exportacion(datos)
        titulo = f"Lista de control — {servicio_control.capitalize()}"
        if formato == "csv":
            return Response(
                csv_lista_control(columnas, filas),
                media_type="text/csv; charset=utf-8",
                headers=_cabecera_descarga(_nombre_archivo(servicio_control, fecha, "csv")),
            )
        if formato == "xlsx":
            return Response(
                xlsx_lista_control(titulo, fecha, columnas, filas, filtros, institucion),
                media_type=("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"),
                headers=_cabecera_descarga(_nombre_archivo(servicio_control, fecha, "xlsx")),
            )
        return Response(
            html_lista_control(titulo, fecha, columnas, filas, filtros, institucion),
            media_type="text/html; charset=utf-8",
            headers={
                "Content-Disposition": f'inline; filename="{_nombre_archivo(servicio_control, fecha, "pdf")}"'
            },
        )

    return router
