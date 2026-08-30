from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import SesionAcceso
from aplicacion.seguridad import token_hash

from .conftest import preparar_estudiante


def test_rutas_publicas_y_rbac(entorno):
    cliente, _, h = entorno
    rutas = set(cliente.app.openapi()["paths"])
    assert {
        "/api/v1/personas",
        "/api/v1/anios-lectivos",
        "/api/v1/matriculas",
        "/api/v1/menu/publicaciones",
        "/api/v1/comedor/operacion",
        "/api/v1/transporte/marcas",
        "/api/v1/reportes/ventas",
    } <= rutas
    assert cliente.get("/api/v1/personas").status_code == 401
    assert (
        cliente.post(
            "/api/v1/personas",
            headers=h["operador"],
            json={
                "nombres": "Sin permiso",
                "tipo": "profesor",
            },
        ).status_code
        == 403
    )


def test_identidad_codigo_pin_y_matricula_anual_unica(entorno):
    cliente, _, h = entorno
    persona, anio, matricula = preparar_estudiante(cliente, h["admin"])
    assert persona["codigo"].startswith("E-") and len(persona["codigo"]) == 10
    assert len(persona["pinTemporal"]) == 6
    token = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "codigo": persona["codigo"],
            "pin": "123456",
        },
    ).json()["token"]
    assert (
        cliente.get("/api/v1/sesion", headers={"Authorization": f"Bearer {token}"}).status_code
        == 200
    )
    duplicada = cliente.post(
        "/api/v1/matriculas",
        headers=h["admin"],
        json={
            "personaId": persona["id"],
            "anioLectivoId": anio["id"],
            "seccion": "8-1",
            "turno": "almuerzo",
            "becado": False,
        },
    )
    assert duplicada.status_code == 409
    assert matricula["persona_id"] == persona["id"]


def test_cambio_pin_revoca_sesion_y_desactiva_cambio_obligatorio(entorno):
    cliente, motor, h = entorno
    persona = cliente.post(
        "/api/v1/personas",
        headers=h["admin"],
        json={"cedula": "77", "nombres": "Pin Temporal", "tipo": "estudiante"},
    ).json()
    acceso = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "codigo": persona["codigo"],
            "pin": persona["pinTemporal"],
        },
    ).json()
    assert acceso["cambioObligatorio"] is True
    with Session(motor) as sesion:
        registro = sesion.get(SesionAcceso, token_hash(acceso["token"]))
        assert registro is not None and registro.cambio_obligatorio is True
    cabecera = {"Authorization": f"Bearer {acceso['token']}"}
    assert cliente.get("/api/v1/sesion", headers=cabecera).json()["cambioObligatorio"] is True
    assert (
        cliente.post(
            "/api/v1/comedor/reservas",
            headers=cabecera,
            json={"codigo": persona["codigo"], "fecha": "2026-09-01"},
        ).status_code
        == 403
    )
    cambio = cliente.post(
        "/api/v1/autenticacion/portal/pin",
        headers=cabecera,
        json={
            "pinActual": persona["pinTemporal"],
            "pinNuevo": "654321",
        },
    )
    assert cambio.status_code == 200
    assert cambio.json() == {"cambioObligatorio": False, "sesionesRevocadas": True}
    assert cliente.get("/api/v1/sesion", headers=cabecera).status_code == 401
    assert (
        cliente.post(
            "/api/v1/autenticacion/portal",
            json={
                "codigo": persona["codigo"],
                "pin": persona["pinTemporal"],
            },
        ).status_code
        == 401
    )
    nuevo = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "codigo": persona["codigo"],
            "pin": "654321",
        },
    )
    assert nuevo.status_code == 200 and nuevo.json()["cambioObligatorio"] is False
    nueva_cabecera = {"Authorization": f"Bearer {nuevo.json()['token']}"}
    assert cliente.post("/api/v1/autenticacion/logout", headers=nueva_cabecera).status_code == 204
    assert cliente.get("/api/v1/sesion", headers=nueva_cabecera).status_code == 401


def test_alta_siempre_genera_pin_temporal_y_rechaza_pin_elegido(entorno):
    cliente, _, h = entorno
    respuesta = cliente.post(
        "/api/v1/personas",
        headers=h["admin"],
        json={
            "cedula": "78",
            "nombres": "Alta segura",
            "tipo": "profesor",
            "pin": "111111",
        },
    )
    assert respuesta.status_code == 422


def test_un_solo_anio_vigente_y_rutas_sin_solape(entorno):
    cliente, _, h = entorno
    _, _, matricula = preparar_estudiante(cliente, h["admin"])
    segundo = cliente.post(
        "/api/v1/anios-lectivos", headers=h["admin"], json={"anio": 2027, "vigente": True}
    )
    assert segundo.status_code == 201
    vigentes = [
        a for a in cliente.get("/api/v1/anios-lectivos", headers=h["admin"]).json() if a["vigente"]
    ]
    assert [a["anio"] for a in vigentes] == [2027]
    ruta = cliente.post("/api/v1/rutas", headers=h["admin"], json={"nombre": "Ruta Norte"}).json()
    datos = {"matriculaId": matricula["id"], "fechaInicio": "2026-02-01", "fechaFin": "2026-06-30"}
    assert (
        cliente.post(
            f"/api/v1/rutas/{ruta['id']}/asignaciones", headers=h["admin"], json=datos
        ).status_code
        == 201
    )
    datos["fechaInicio"], datos["fechaFin"] = "2026-06-01", None
    assert (
        cliente.post(
            f"/api/v1/rutas/{ruta['id']}/asignaciones", headers=h["admin"], json=datos
        ).status_code
        == 409
    )


def test_publicacion_conserva_copia_de_componentes(entorno):
    cliente, _, h = entorno
    plantilla = cliente.post(
        "/api/v1/menu/plantillas",
        headers=h["admin"],
        json={
            "nombre": "Tipico",
            "componentes": ["Arroz", "Frijoles"],
        },
    ).json()
    publicada = cliente.post(
        "/api/v1/menu/publicaciones",
        headers=h["admin"],
        json={
            "plantillaId": plantilla["id"],
            "fecha": "2026-08-31",
        },
    )
    assert publicada.status_code == 201
    assert publicada.json()["componentes"] == ["Arroz", "Frijoles"]
    assert cliente.get("/api/v1/menu/publicaciones").json()[0]["nombre"] == "Tipico"
