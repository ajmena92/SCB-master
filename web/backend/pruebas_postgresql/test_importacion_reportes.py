import io

from openpyxl import Workbook


def test_importacion_previsualiza_confirma_y_es_idempotente(entorno):
    cliente, _, h = entorno
    datos = {
        "anio": 2026,
        "filas": [
            {
                "cedula": "101",
                "nombres": "Importada",
                "tipo": "estudiante",
                "seccion": "9-1",
                "turno": "almuerzo",
                "becado": True,
                "ruta": "Centro",
            }
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=datos)
    assert previa.status_code == 200 and previa.json()["altas"] == 1
    confirmacion = cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**datos, "huella": previa.json()["huella"]},
    )
    assert confirmacion.status_code == 200 and confirmacion.json()["repetida"] is False
    credenciales = confirmacion.json()["credenciales"]
    assert len(credenciales) == 1
    assert set(credenciales[0]) == {"codigo", "nombre", "pinTemporal"}
    assert len(credenciales[0]["pinTemporal"]) == 6
    repetida = cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**datos, "huella": previa.json()["huella"]},
    )
    assert repetida.status_code == 200 and repetida.json()["repetida"] is True
    assert repetida.json()["credenciales"] == []


def test_importacion_xlsx_usa_multipart_y_solo_previsualiza(entorno):
    cliente, _, h = entorno
    libro = Workbook()
    hoja = libro.active
    hoja.append(["cedula", "nombres", "tipo", "seccion", "turno", "becado", "ruta"])
    hoja.append(["303", "Desde Excel", "estudiante", "10-1", "almuerzo", "si", "Centro"])
    contenido = io.BytesIO()
    libro.save(contenido)

    respuesta = cliente.post(
        "/api/v1/importaciones/previsualizar",
        headers=h["admin"],
        data={"anio": "2026"},
        files={
            "archivo": (
                "padron.xlsx",
                contenido.getvalue(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            )
        },
    )
    assert respuesta.status_code == 200, respuesta.text
    assert respuesta.json()["altas"] == 1
    assert respuesta.json()["datos"]["filas"][0]["nombres"] == "Desde Excel"


def test_importacion_bloquea_duplicados_y_reportes_exportan_csv(entorno):
    cliente, _, h = entorno
    fila = {"cedula": "202", "nombres": "Duplicada", "tipo": "profesor"}
    previa = cliente.post(
        "/api/v1/importaciones/previsualizar",
        headers=h["admin"],
        json={"anio": 2026, "filas": [fila, fila]},
    ).json()
    assert previa["aplicable"] is False and len(previa["errores"]) == 1
    csv = cliente.get(
        "/api/v1/reportes/ventas?desde=2026-01-01&hasta=2026-12-31&formato=csv",
        headers=h["operador"],
    )
    assert csv.status_code == 200 and csv.headers["content-type"].startswith("text/csv")
