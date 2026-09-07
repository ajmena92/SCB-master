import base64
import io
from datetime import datetime, timedelta, timezone

from openpyxl import Workbook
from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import Matricula, Persona
from aplicacion.nucleo.postgresql import crear_fabrica_sesiones
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.trabajos_importacion import procesar_un_trabajo

CLAVE_RESULTADOS = base64.urlsafe_b64encode(b"x" * 32).decode()


def encolar_y_procesar(cliente, motor, cabeceras, datos, huella):
    respuesta = cliente.post(
        "/api/v1/importaciones/confirmar", headers=cabeceras, json={**datos, "huella": huella}
    )
    assert respuesta.status_code == 202, respuesta.text
    trabajo_id = respuesta.json()["trabajoId"]
    assert procesar_un_trabajo(crear_fabrica_sesiones(motor), CLAVE_RESULTADOS) == trabajo_id
    return trabajo_id


def test_importacion_normaliza_seccion_y_rechaza_formato_ambiguo(entorno):
    cliente, motor, h = entorno
    datos = {
        "anio": 2026,
        "filas": [
            {"cedula": "seccion-normalizada", "nombres": "Sección", "tipo": "estudiante", "seccion": " 8 - 5 "}
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=datos)
    assert previa.status_code == 200, previa.text
    encolar_y_procesar(cliente, motor, h["admin"], datos, previa.json()["huella"])
    with Session(motor) as sesion:
        assert sesion.scalar(select(Matricula.seccion)) == "8-5"

    invalida = cliente.post(
        "/api/v1/importaciones/previsualizar",
        headers=h["admin"],
        json={
            "anio": 2026,
            "filas": [
                {"cedula": "seccion-invalida", "nombres": "Inválida", "tipo": "estudiante", "seccion": "8-A"}
            ],
        },
    )
    assert invalida.status_code == 422


def test_importacion_previsualiza_confirma_y_es_idempotente(entorno):
    cliente, motor, h = entorno
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
    trabajo_id = encolar_y_procesar(cliente, motor, h["admin"], datos, previa.json()["huella"])
    repetida = cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**datos, "huella": previa.json()["huella"]},
    )
    assert repetida.status_code == 202 and repetida.json()["trabajoId"] == trabajo_id
    assert repetida.json()["estado"] == "completado"


def test_confirmacion_encola_un_trabajo_idempotente(entorno):
    """Las altas costosas se apartan de la solicitud HTTP sin exponer PIN."""
    cliente, _, h = entorno
    datos = {
        "anio": 2026,
        "filas": [
            {"cedula": "201", "nombres": "Alta en cola", "tipo": "estudiante", "seccion": "9-1"}
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=datos)
    assert previa.status_code == 200, previa.text
    solicitud = {**datos, "huella": previa.json()["huella"]}

    primero = cliente.post("/api/v1/importaciones/confirmar", headers=h["admin"], json=solicitud)
    assert primero.status_code == 202, primero.text
    assert primero.json()["estado"] == "pendiente"
    assert "credenciales" not in primero.json()

    segundo = cliente.post("/api/v1/importaciones/confirmar", headers=h["admin"], json=solicitud)
    assert segundo.status_code == 202
    assert segundo.json()["trabajoId"] == primero.json()["trabajoId"]

    estado = cliente.get(
        f"/api/v1/importaciones/trabajos/{primero.json()['trabajoId']}", headers=h["admin"]
    )
    assert estado.status_code == 200
    assert estado.json()["estado"] == "pendiente"


def test_worker_confirma_el_trabajo_y_cifra_credenciales(entorno):
    cliente, motor, h = entorno
    datos = {
        "anio": 2026,
        "filas": [
            {"cedula": "202", "nombres": "Worker", "tipo": "estudiante", "seccion": "9-1"}
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=datos)
    encolado = cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**datos, "huella": previa.json()["huella"]},
    )
    assert encolado.status_code == 202
    assert procesar_un_trabajo(crear_fabrica_sesiones(motor), CLAVE_RESULTADOS) == encolado.json()["trabajoId"]

    with Session(motor) as sesion:
        from aplicacion.modelos.operacion import TrabajoImportacion

        trabajo = sesion.get(TrabajoImportacion, encolado.json()["trabajoId"])
        assert trabajo is not None and trabajo.estado == "completado"
        assert trabajo.resultado_cifrado is not None
        assert "pinTemporal" not in trabajo.resultado_cifrado

    primera_entrega = cliente.post(
        f"/api/v1/importaciones/trabajos/{encolado.json()['trabajoId']}/credenciales",
        headers=h["admin"],
    )
    assert primera_entrega.status_code == 200
    assert len(primera_entrega.json()["credenciales"]) == 1
    segunda_entrega = cliente.post(
        f"/api/v1/importaciones/trabajos/{encolado.json()['trabajoId']}/credenciales",
        headers=h["admin"],
    )
    assert segunda_entrega.status_code == 410


def test_recupera_un_trabajo_interrumpido(entorno):
    cliente, motor, h = entorno
    datos = {
        "anio": 2026,
        "filas": [
            {"cedula": "203", "nombres": "Interrumpida", "tipo": "estudiante", "seccion": "9-1"}
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=datos)
    encolado = cliente.post(
        "/api/v1/importaciones/confirmar",
        headers=h["admin"],
        json={**datos, "huella": previa.json()["huella"]},
    )
    with Session(motor) as sesion:
        from aplicacion.modelos.operacion import TrabajoImportacion

        trabajo = sesion.get(TrabajoImportacion, encolado.json()["trabajoId"])
        assert trabajo is not None
        trabajo.estado = "ejecutando"
        trabajo.iniciado_en = datetime.now(timezone.utc) - timedelta(minutes=31)
        sesion.commit()

    with Session(motor) as sesion:
        assert RepositorioImportacion(sesion).recuperar_trabajos_interrumpidos(
            datetime.now(timezone.utc) - timedelta(minutes=30)
        ) == 1
        sesion.commit()

    with Session(motor) as sesion:
        trabajo = sesion.get(TrabajoImportacion, encolado.json()["trabajoId"])
        assert trabajo is not None and trabajo.estado == "pendiente" and trabajo.iniciado_en is None


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
    encolar_y_procesar(cliente, motor, h["admin"], inicial, previa.json()["huella"])

    actualizado = {
        "anio": 2026,
        "filas": [{"cedula": "101", "nombres": "Permanece", "tipo": "estudiante", "seccion": "10-1"}],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=actualizado)
    assert previa.status_code == 200, previa.text
    assert previa.json()["desactivaciones"] == 1
    encolar_y_procesar(cliente, motor, h["admin"], actualizado, previa.json()["huella"])

    with Session(motor) as conexion:
        ausente = conexion.scalar(select(Persona).where(Persona.cedula == "102"))
        assert ausente is not None
        assert ausente.activo is False


def test_importacion_preserva_profesores_con_cuenta_administrativa(entorno):
    cliente, motor, h = entorno
    datos = {
        "anio": 2026,
        "filas": [
            {"cedula": "204", "nombres": "Profesor importado", "tipo": "profesor"}
        ],
    }
    previa = cliente.post("/api/v1/importaciones/previsualizar", headers=h["admin"], json=datos)
    assert previa.status_code == 200, previa.text
    assert previa.json()["desactivaciones"] == 0
    encolar_y_procesar(cliente, motor, h["admin"], datos, previa.json()["huella"])

    with Session(motor) as conexion:
        administrador = conexion.scalar(select(Persona).where(Persona.cedula == "900000001"))
        assert administrador is not None and administrador.activo is True


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


def test_importacion_xlsx_rechaza_archivo_mayor_al_limite(entorno):
    cliente, _, h = entorno
    respuesta = cliente.post(
        "/api/v1/importaciones/previsualizar",
        headers=h["admin"],
        data={"anio": "2026"},
        files={
            "archivo": (
                "padron.xlsx",
                b"x" * (12 * 1024 * 1024 + 1),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            )
        },
    )
    assert respuesta.status_code == 413


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
