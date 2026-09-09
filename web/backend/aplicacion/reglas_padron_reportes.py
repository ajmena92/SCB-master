"""Reglas puras compartidas por el tablero y las listas de control."""

import re
from collections.abc import Mapping
from typing import TypedDict


class FilaNominal(TypedDict, total=False):
    idPersona: int
    nombreCompleto: str
    identificacion: str | None
    seccion: str
    ruta: str
    idRuta: int | None
    apellidos: str
    nombres: str
    beneficio: str
    beneficioComedor: str
    beneficioServicio: str
    estadoClave: str
    estado: str
    confirmacionClave: str
    beneficioClave: str
    columnaServicio: str
    historico: bool


def partes_nombre(nombre: str | None) -> tuple[str, str]:
    partes = " ".join(str(nombre or "").split()).split(" ")
    if len(partes) > 2:
        return " ".join(partes[-2:]), " ".join(partes[:-2])
    return "", " ".join(partes)


def ordenar_lista_control(filas: list[FilaNominal]) -> list[FilaNominal]:
    def clave(fila: FilaNominal) -> tuple[int, int, str, str, str, str]:
        seccion = str(fila.get("seccion") or "")
        coincidencia = re.match(r"^(\d+)\s*-\s*(\d+)", seccion)
        grado = int(coincidencia.group(1)) if coincidencia else 999
        grupo = int(coincidencia.group(2)) if coincidencia else 999
        apellidos, nombres = partes_nombre(fila.get("nombreCompleto"))
        return (
            grado,
            grupo,
            seccion.casefold(),
            apellidos.casefold(),
            nombres.casefold(),
            str(fila.get("identificacion") or "").casefold(),
        )

    return sorted(filas, key=clave)


def filtrar_nominal(
    nominal: list[FilaNominal], filtros: Mapping[str, str | int]
) -> list[FilaNominal]:
    """Aplica únicamente los filtros propios del servicio seleccionado.

    El padrón de comedor y el de transporte comparten persona, sección y
    búsqueda, pero no comparten el significado de una marca. Mantenerlos
    separados evita que una asistencia al comedor se interprete como uso
    de transporte (o a la inversa).
    """
    busqueda = str(filtros.get("busqueda", "")).casefold().strip()
    ruta_filtro = str(filtros.get("ruta", "")).strip()
    seccion = str(filtros.get("seccion", "")).casefold().strip()
    servicio = str(filtros.get("servicio", "")).strip()
    asistencia = str(filtros.get("asistencia", filtros.get("estado", ""))).strip()
    confirmacion = str(filtros.get("confirmacion", "")).strip()
    beneficio = str(filtros.get("beneficio", "")).strip()
    asignacion = str(filtros.get("asignacion", "")).strip()
    if busqueda:
        nominal = [r for r in nominal if busqueda in r["nombreCompleto"].casefold()]
    if ruta_filtro:
        nominal = [r for r in nominal if str(r["idRuta"]) == ruta_filtro]
    if seccion:
        nominal = [r for r in nominal if seccion in r["seccion"].casefold()]
    if asistencia:
        nominal = [r for r in nominal if r["estadoClave"] == asistencia]
    if servicio == "comedor":
        if confirmacion:
            nominal = [r for r in nominal if r["confirmacionClave"] == confirmacion]
        if beneficio == "beneficiario":
            nominal = [r for r in nominal if r["beneficioClave"] == "beneficiario"]
        elif beneficio == "no_beneficiario":
            nominal = [r for r in nominal if r["beneficioClave"] == "no_beneficiario"]
    elif servicio == "transporte":
        if asignacion == "con_ruta":
            nominal = [r for r in nominal if r["idRuta"] is not None]
        elif asignacion == "sin_ruta":
            nominal = [r for r in nominal if r["idRuta"] is None]
    return nominal
