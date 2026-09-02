import io

from openpyxl import Workbook
from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import Persona


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


def test_importacion_anual_desactiva_ausentes_y_no_modifica_beca_ni_ruta(entorno):
    cliente, motor, h = entorno
    inicial = {
        "anio": 2026,
        "filas": [
            {"cedula": "101", "nombres": "Permanece", "tipo": "estudiante", "seccion": "9-1"},
            {"cedula": "102", "nombres": "Ausente", "tipo": "estudiante", "seccion": "9-2"},
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=inicial)
    assert previa.status_code == 200, previa.text
    assert cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**inicial, "huella": previa.json()["huella"]},
    ).status_code == 200

    actualizado = {
        "anio": 2026,
        "filas": [{"cedula": "101", "nombres": "Permanece", "tipo": "estudiante", "seccion": "10-1"}],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=actualizado)
    assert previa.status_code == 200, previa.text
    assert previa.json()["desactivaciones"] == 1
    assert cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**actualizado, "huella": previa.json()["huella"]},
    ).status_code == 200

    with Session(motor) as conexion:
        ausente = conexion.scalar(select(Persona).where(Persona.cedula == "102"))
        assert ausente is not None
        assert ausente.activo is False


def test_importacion_rechaza_beca_y_ruta_como_datos_del_padron(entorno):
    cliente, _, h = entorno
    respuesta = cliente.post(
        "/api/v1/importaciones/previsualizar",
        headers=h["admin"],
        json={
            "anio": 2026,
            "filas": [
                {
                    "cedula": "101",
                    "nombres": "No importa beneficios",
                    "tipo": "estudiante",
                    "seccion": "9-1",
                    "becado": True,
                    "ruta": "Centro",
                }
            ],
        },
    )
    assert respuesta.status_code == 422


def test_importacion_xlsx_usa_multipart_y_solo_previsualiza(entorno):
    cliente, _, h = entorno
    libro = Workbook()
    hoja = libro.active
    hoja.append(["cedula", "nombres", "tipo", "seccion"])
    hoja.append(["303", "Desde Excel", "estudiante", "10-1"])
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
