from datetime import date, time

from .conftest import preparar_estudiante


def _token_portal(cliente, cedula):
    respuesta = cliente.post(
        "/api/v1/autenticacion/portal",
        json={"cedula": cedula, "pin": "123456"},
    )
    assert respuesta.status_code == 200, respuesta.text
    return {"Authorization": f"Bearer {respuesta.json()['token']}"}


def test_dashboard_usa_padron_anual_postgresql(entorno):
    cliente, _, auth = entorno
    persona, anio, _ = preparar_estudiante(cliente, auth["admin"], "dashboard-1")

    respuesta = cliente.get(
        "/api/v1/reportes/dashboard",
        headers=auth["admin"],
        params={"fecha": f"{anio['anio']}-08-30", "tipoPersona": "estudiante"},
    )

    assert respuesta.status_code == 200, respuesta.text
    datos = respuesta.json()
    assert datos["asistencia"]["total"] == 1
    assert datos["nominal"]["elementos"][0]["idPersona"] == persona["id"]
    assert datos["cobertura"]["conMatricula"] == 1
    assert len(datos["ultimosCincoDias"]) == 5


def test_portal_muestra_plantilla_semanal_y_carnet(entorno):
    cliente, _, auth = entorno
    persona, _, matricula = preparar_estudiante(cliente, auth["admin"], "portal-1")
    ruta = cliente.post(
        "/api/v1/rutas",
        headers=auth["admin"],
        json={
            "codigo": "1115308",
            "descripcion": "SIERRA",
            "colorHex": "#38BDF8",
        },
    ).json()
    asignacion = cliente.post(
        f"/api/v1/rutas/{ruta['idRuta']}/asignaciones",
        headers=auth["admin"],
        json={
            "matriculaId": matricula["id"],
            "fechaInicio": f"{date.today().year}-01-01",
        },
    )
    assert asignacion.status_code == 201, asignacion.text
    fecha = date.today()
    semana_panea = (fecha.day - 1) // 7 + 1
    plantilla = cliente.post(
        "/api/v1/menu/plantillas",
        headers=auth["admin"],
        json={
            "semana": semana_panea,
            "dia": fecha.isoweekday(),
            "titulo": "Almuerzo tradicional",
            "activo": True,
            "componentes": [
                {"nombre": "Arroz", "tipo": "Principal", "orden": 1},
                {"nombre": "Frijoles", "tipo": "Acompañamiento", "orden": 2},
            ],
        },
    ).json()
    assert plantilla["id"] > 0
    portal = _token_portal(cliente, persona["cedula"])

    estado = cliente.get("/api/v1/portal/estado", headers=portal)
    carnet = cliente.get("/api/v1/portal/carnet", headers=portal)

    assert estado.status_code == 200, estado.text
    estado_portal = estado.json()
    estado_comedor = estado_portal["estado"]
    assert estado_portal["menu"]["Titulo"] == "Almuerzo tradicional"
    assert [c["Nombre"] for c in estado_portal["menu"]["Componentes"]] == [
        "Arroz",
        "Frijoles",
    ]
    hora_servidor = time.fromisoformat(estado_comedor["horaServidor"])
    hora_limite = time.fromisoformat(estado_comedor["horaLimite"])
    esperados = max(0, int((
        hora_limite.hour * 3600 + hora_limite.minute * 60 + hora_limite.second
    ) - (
        hora_servidor.hour * 3600 + hora_servidor.minute * 60 + hora_servidor.second
    )))
    assert abs(estado_comedor["segundosParaCierre"] - esperados) <= 1
    assert estado_comedor["segundosParaApertura"] == 0
    assert carnet.status_code == 200, carnet.text
    assert carnet.json()["barcode"] == persona["codigo"]
    assert carnet.json()["seccion"] == "7-1"
    assert carnet.json()["rutaCodigo"] == "1115308"
    assert carnet.json()["rutaDescripcion"] == "SIERRA"
    assert carnet.json()["rutaColor"] == "#38BDF8"
    assert carnet.json()["anioLectivo"] == date.today().year


def test_reserva_portal_no_requiere_repetir_codigo(entorno):
    cliente, _, auth = entorno
    persona, _, _ = preparar_estudiante(cliente, auth["admin"], "reserva-portal-1")
    portal = _token_portal(cliente, persona["cedula"])

    respuesta = cliente.post(
        "/api/v1/comedor/reservas",
        headers=portal,
        json={"fecha": date.today().isoformat()},
    )

    assert respuesta.status_code in {201, 409}, respuesta.text
