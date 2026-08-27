from aplicacion.modulos.reportes.servicio import ServicioReportes


class RepositorioFalso:
    def estudiantes(self) -> list[dict]:
        return [
            {
                "id_estudiante": 1,
                "carne": "A1",
                "nombre_completo": "Ana Sol",
                "seccion": "7-1",
                "activo": True,
            }
        ]

    def transporte(self) -> list[dict]:
        return [
            {
                "id_ruta": 2,
                "codigo": "R-2",
                "descripcion": "Norte",
                "estudiantes_asignados": 4,
                "activo": True,
            }
        ]

    def resumen(self) -> dict:
        return {"estudiantes": 1, "confirmaciones": 0, "cancelaciones": 0}


def test_reporte_estudiantes_y_csv() -> None:
    servicio = ServicioReportes(RepositorioFalso())
    assert servicio.estudiantes().total == 1
    assert "nombreCompleto" in servicio.estudiantes_csv()


def test_reporte_transporte_y_csv() -> None:
    servicio = ServicioReportes(RepositorioFalso())
    assert servicio.transporte().elementos[0].codigo == "R-2"
    assert "estudiantesAsignados" in servicio.transporte_csv()
