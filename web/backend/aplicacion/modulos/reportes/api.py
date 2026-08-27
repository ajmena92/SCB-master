"""Adaptador HTTP del dominio de reportes."""

from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends
from fastapi.responses import Response

from .esquemas import ReporteEstudiantes, ReporteTransporte
from .repositorio import RepositorioReportes
from .servicio import ServicioReportes


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioReportes]],
    exigir_permiso: Callable[..., Callable],
) -> APIRouter:
    enrutador = APIRouter(prefix="/reportes", tags=["reportes"])

    def servicio(repo: RepositorioReportes = Depends(obtener_repositorio)) -> ServicioReportes:
        return ServicioReportes(repo)

    @enrutador.get("/estudiantes", response_model=ReporteEstudiantes, response_model_by_alias=True)
    def estudiantes(
        _usuario: dict = Depends(exigir_permiso("reportes.leer")),
        caso: ServicioReportes = Depends(servicio),
    ) -> ReporteEstudiantes:
        return caso.estudiantes()

    @enrutador.get("/transporte", response_model=ReporteTransporte, response_model_by_alias=True)
    def transporte(
        _usuario: dict = Depends(exigir_permiso("reportes.leer")),
        caso: ServicioReportes = Depends(servicio),
    ) -> ReporteTransporte:
        return caso.transporte()

    @enrutador.get("/estudiantes.csv", response_class=Response)
    def estudiantes_csv(
        _usuario: dict = Depends(exigir_permiso("reportes.exportar")),
        caso: ServicioReportes = Depends(servicio),
    ) -> Response:
        return Response(
            caso.estudiantes_csv(),
            media_type="text/csv; charset=utf-8",
            headers={"Content-Disposition": "attachment; filename=reporte_estudiantes.csv"},
        )

    @enrutador.get("/transporte.csv", response_class=Response)
    def transporte_csv(
        _usuario: dict = Depends(exigir_permiso("reportes.exportar")),
        caso: ServicioReportes = Depends(servicio),
    ) -> Response:
        return Response(
            caso.transporte_csv(),
            media_type="text/csv; charset=utf-8",
            headers={"Content-Disposition": "attachment; filename=reporte_transporte.csv"},
        )

    return enrutador
