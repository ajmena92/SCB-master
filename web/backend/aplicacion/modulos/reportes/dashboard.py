"""Contratos y rutas del dashboard operativo de asistencia."""

from collections.abc import Callable, Iterator
from datetime import date
from typing import Literal

from fastapi import APIRouter, Depends, Query
from pydantic import BaseModel, ConfigDict, Field


class MetricaAsistencia(BaseModel):
    total: int
    presentes: int
    ausentes: int
    tardanzas: int
    justificadas: int
    sin_registro: int = Field(serialization_alias="sinRegistro")
    cobertura_registro: float = Field(serialization_alias="coberturaRegistro")
    porcentaje: float


class GrupoDashboard(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    nombre: str
    total: int
    presentes: int = 0
    ausentes: int = 0
    sin_registro: int = Field(default=0, serialization_alias="sinRegistro")
    consumo: int = 0
    porcentaje: float = 0


class TendenciaDia(BaseModel):
    fecha: date
    dia: str
    total: int
    presentes: int
    ausentes: int
    sin_registro: int = Field(serialization_alias="sinRegistro")
    porcentaje: float


class RutaDashboard(GrupoDashboard):
    id_ruta: int | None = Field(default=None, serialization_alias="idRuta")


class AlertaDashboard(BaseModel):
    tipo: str
    titulo: str
    cantidad: int


class RegistroNominal(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_persona: int = Field(serialization_alias="idPersona")
    id_estudiante: int | None = Field(default=None, serialization_alias="idEstudiante")
    nombre_completo: str = Field(serialization_alias="nombreCompleto")
    cedula: str | None
    horario: str
    seccion: str
    tipo_persona: str = Field(serialization_alias="tipoPersona")
    id_estado_comedor: Literal[1, 2] = Field(serialization_alias="idEstadoComedor")
    beneficio_comedor: str = Field(serialization_alias="beneficioComedor")
    ruta: str = "Sin ruta"
    estado: str
    origen: str
    historico: bool = False


class NominalPaginado(BaseModel):
    elementos: list[RegistroNominal]
    total: int
    pagina: int
    por_pagina: int = Field(serialization_alias="porPagina")


class DashboardSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    fecha: date
    tipo_persona: Literal["estudiante", "profesor"] = Field(serialization_alias="tipoPersona")
    asistencia: MetricaAsistencia
    consumo_comedor: int = Field(serialization_alias="consumoComedor")
    becados_comedor: int = Field(serialization_alias="becadosComedor")
    no_becados: int = Field(serialization_alias="noBecados")
    por_horario: list[GrupoDashboard] = Field(serialization_alias="porHorario")
    horarios: list[Literal["diurno", "nocturno"]] = Field(default_factory=list)
    alertas: list[AlertaDashboard] = Field(default_factory=list)
    saldo_tiquetes: int = Field(default=0, serialization_alias="saldoTiquetes")
    tiquetes_reservados: int = Field(default=0, serialization_alias="tiquetesReservados")
    tiquetes_consumidos: int = Field(default=0, serialization_alias="tiquetesConsumidos")
    ingresos_historicos: int = Field(default=0, serialization_alias="ingresosHistoricos")
    por_seccion: list[GrupoDashboard] = Field(serialization_alias="porSeccion")
    por_estado_comedor: list[GrupoDashboard] = Field(serialization_alias="porEstadoComedor")
    por_ruta: list[RutaDashboard] = Field(serialization_alias="porRuta")
    semana: list[TendenciaDia]
    ultimos_cinco_dias: list[TendenciaDia] = Field(serialization_alias="ultimosCincoDias")
    nominal: NominalPaginado


def crear_enrutador_dashboard(
    obtener_repositorio: Callable[[], Iterator], exigir_permiso: Callable[..., Callable]
) -> APIRouter:
    r = APIRouter(prefix="/dashboard", tags=["dashboard"])

    @r.get("", response_model=DashboardSalida, response_model_by_alias=True)
    def consultar(
        fecha: date = Query(...),
        pagina: int = Query(1, ge=1),
        por_pagina: int = Query(25, ge=1, le=100, alias="porPagina"),
        busqueda: str | None = Query(None),
        ruta: int | None = Query(None),
        id_estado_comedor: Literal[1, 2] | None = Query(None, alias="idEstadoComedor"),
        tipo_persona: Literal["estudiante", "profesor"] = Query(
            "estudiante", alias="tipoPersona"
        ),
        seccion: str | None = Query(None),
        estado: str | None = Query(None),
        horario: Literal["diurno", "nocturno"] | None = Query(None),
        _=Depends(exigir_permiso("reportes.dashboard.leer")),
        repo=Depends(obtener_repositorio),
    ) -> DashboardSalida:
        return DashboardSalida(
            **repo.dashboard(
                fecha,
                pagina=pagina,
                por_pagina=por_pagina,
                busqueda=busqueda,
                id_ruta=ruta,
                id_estado_comedor=id_estado_comedor,
                tipo_persona=tipo_persona,
                seccion=seccion,
                estado=estado,
                horario=horario,
            )
        )

    @r.get("/rutas/{ruta}", response_model=DashboardSalida, response_model_by_alias=True)
    def detalle_ruta(
        ruta: int,
        fecha: date = Query(...),
        pagina: int = Query(1, ge=1),
        por_pagina: int = Query(25, ge=1, le=100, alias="porPagina"),
        horario: Literal["diurno", "nocturno"] | None = Query(None),
        _=Depends(exigir_permiso("reportes.dashboard.leer")),
        repo=Depends(obtener_repositorio),
    ) -> DashboardSalida:
        return DashboardSalida(
            **repo.dashboard(
                fecha,
                pagina=pagina,
                por_pagina=por_pagina,
                id_ruta=ruta,
                horario=horario,
            )
        )

    return r
