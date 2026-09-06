"""Guardas contra consultas por fila y regresiones del lector XLSX."""

import io

from openpyxl import Workbook
from sqlalchemy import event
from sqlalchemy.orm import Session

from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.repositorios_importacion import RepositorioImportacion


def test_previsualizacion_consulta_por_bloques(entorno):
    _, motor, _ = entorno
    datos = ImportacionEntrada.model_validate(
        {
            "anio": 2026,
            "filas": [
                {"cedula": f"SINT-{i}", "nombres": "Sintética", "tipo": "profesor"}
                for i in range(1001)
            ],
        }
    )
    consultas = []

    def contar(_conn, _cursor, sentencia, *_args):
        consultas.append(sentencia)

    event.listen(motor, "before_cursor_execute", contar)
    try:
        with Session(motor) as sesion:
            resultado = ServicioImportacion(RepositorioImportacion(sesion)).previsualizar(datos)
    finally:
        event.remove(motor, "before_cursor_execute", contar)
    assert resultado["altas"] == 1001
    assert resultado["aplicable"]
    assert len(consultas) == 4  # tres bloques de 500 y un conteo


def test_excel_ignora_filas_vacias_y_preserva_huella(entorno):
    _, motor, _ = entorno
    libro = Workbook()
    libro.active.append(["cedula", "nombres", "tipo", "seccion"])
    libro.active.append([None, None, None, None])
    libro.active.append(["SINT-1", "Sintética", "estudiante", "9-1"])
    salida = io.BytesIO()
    libro.save(salida)
    libro.close()
    with Session(motor) as sesion:
        servicio = ServicioImportacion(RepositorioImportacion(sesion))
        datos = servicio.desde_excel(salida.getvalue(), 2026)
        esperado = ImportacionEntrada.model_validate(
            {
                "anio": 2026,
                "filas": [
                    {
                        "cedula": "SINT-1",
                        "nombres": "Sintética",
                        "tipo": "estudiante",
                        "seccion": "9-1",
                    }
                ],
            }
        )
        assert servicio.previsualizar(datos) == servicio.previsualizar(esperado)


def test_listado_personas_no_consulta_por_cuenta(entorno):
    cliente, motor, cabeceras = entorno
    consultas = []

    def contar(_conn, _cursor, sentencia, *_args):
        consultas.append(sentencia)

    event.listen(motor, "before_cursor_execute", contar)
    try:
        respuesta = cliente.get("/api/v1/personas?tamano=100", headers=cabeceras["admin"])
    finally:
        event.remove(motor, "before_cursor_execute", contar)
    assert respuesta.status_code == 200
    cuentas = [s for s in consultas if "FROM cuenta_tiquete" in s]
    assert len(cuentas) == 1
