import pytest


pd = pytest.importorskip("pandas")

from analitica.proyecciones import proyectar_asistencia


def test_detecta_becado_sin_asistencia_y_comprador_de_tiquetes() -> None:
    marcas = pd.DataFrame(
        [
            {"id_estudiante": 1, "fecha": "2026-08-24", "estado": "ausente"},
            {"id_estudiante": 1, "fecha": "2026-08-25", "estado": "ausente"},
            {"id_estudiante": 1, "fecha": "2026-08-26", "estado": "ausente"},
            {"id_estudiante": 2, "fecha": "2026-08-24", "estado": "presente"},
            {"id_estudiante": 2, "fecha": "2026-08-25", "estado": "presente"},
            {"id_estudiante": 2, "fecha": "2026-08-26", "estado": "presente"},
        ]
    )
    estudiantes = pd.DataFrame([{"id_estudiante": 1, "estado_comedor": "becado_comedor"}, {"id_estudiante": 2, "estado_comedor": "no_becado_comedor"}])
    consumos = pd.DataFrame([{"id_estudiante": 2, "modalidad": "tiquete"}] * 3)
    resultado = proyectar_asistencia(marcas, estudiantes, consumos)
    assert resultado[0]["senal"] == "becado sin consumo reciente"
    assert resultado[1]["senal"] == "candidato para revisión de beca"


def test_no_infiere_sin_consumo_si_no_se_entregan_consumos() -> None:
    marcas = pd.DataFrame(
        [
            {"id_estudiante": 1, "fecha": fecha, "estado": "ausente"}
            for fecha in ("2026-08-24", "2026-08-25", "2026-08-26")
        ]
    )
    estudiantes = pd.DataFrame([{"id_estudiante": 1, "estado_comedor": "becado_comedor"}])

    resultado = proyectar_asistencia(marcas, estudiantes)

    assert resultado[0]["senal"] == "becado con baja asistencia"
