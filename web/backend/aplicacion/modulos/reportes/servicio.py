"""Casos de uso de reportes y exportación CSV."""

from __future__ import annotations

import csv
import io

from .esquemas import ReporteEstudiante, ReporteEstudiantes, ReporteRuta, ReporteTransporte
from .repositorio import RepositorioReportes


class ServicioReportes:
    def __init__(self, repositorio: RepositorioReportes) -> None:
        self._repositorio = repositorio

    def estudiantes(self) -> ReporteEstudiantes:
        elementos = [ReporteEstudiante(**fila) for fila in self._repositorio.estudiantes()]
        return ReporteEstudiantes(total=len(elementos), elementos=elementos)

    def transporte(self) -> ReporteTransporte:
        elementos = [ReporteRuta(**fila) for fila in self._repositorio.transporte()]
        return ReporteTransporte(total=len(elementos), elementos=elementos)

    def estudiantes_csv(self) -> str:
        reporte = self.estudiantes()
        return self._csv(
            ["idEstudiante", "carne", "nombreCompleto", "seccion", "activo"],
            [
                [e.id_estudiante, e.carne, e.nombre_completo, e.seccion or "", e.activo]
                for e in reporte.elementos
            ],
        )

    def transporte_csv(self) -> str:
        reporte = self.transporte()
        return self._csv(
            ["idRuta", "codigo", "descripcion", "estudiantesAsignados", "activo"],
            [
                [r.id_ruta, r.codigo, r.descripcion, r.estudiantes_asignados, r.activo]
                for r in reporte.elementos
            ],
        )

    @staticmethod
    def _csv(cabeceras: list[str], filas: list[list[object]]) -> str:
        salida = io.StringIO()
        escritor = csv.writer(salida, lineterminator="\n")
        escritor.writerow(cabeceras)
        escritor.writerows(filas)
        return salida.getvalue()
