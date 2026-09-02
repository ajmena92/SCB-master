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
        "/api/v1/menu/plantillas",
        "/api/v1/menu/calendario",
        "/api/v1/comedor/operacion",
        "/api/v1/reportes/ventas",
    } <= rutas
    assert "/api/v1/transporte/marcas" not in rutas
    assert "/api/v1/transporte/rutas" not in rutas
    assert cliente.get("/api/v1/transporte/rutas", headers=h["admin"]).status_code == 404
    assert cliente.get("/api/v1/personas").status_code == 401
    assert cliente.post("/api/v1/personas", headers=h["operador"], json={"nombres": "Sin permiso", "tipo": "profesor"}).status_code == 403
    assert cliente.post("/api/v1/personas", headers=h["admin"], json={"nombres": "No permitido", "tipo": "profesor"}).status_code == 409
    assert cliente.post("/api/v1/matriculas", headers=h["admin"], json={"personaId": 1, "anioLectivoId": 1, "seccion": "7-1"}).status_code == 409


def test_resumen_personas_es_global_y_requiere_permiso_administrar(entorno):
    cliente, _, h = entorno
    estudiante = crear_persona(cliente, h["admin"], cedula="801", nombres="Estudiante Activa")
    profesor = crear_persona(
        cliente, h["admin"], tipo="profesor", cedula="802", nombres="Profesor Activo"
    )
    inactiva = crear_persona(cliente, h["admin"], cedula="803", nombres="Estudiante Inactiva")
    assert cliente.post(
        f"/api/v1/personas/{inactiva['id']}/desactivar", headers=h["admin"]
    ).status_code == 200

    respuesta = cliente.get("/api/v1/personas/resumen", headers=h["admin"])

    assert respuesta.status_code == 200
    assert respuesta.json() == {
        "total": 5,
        "estudiantesActivos": 1,
        "profesoresActivos": 3,
        "inactivos": 1,
    }
    assert cliente.get("/api/v1/personas/resumen", headers=h["operador"]).status_code == 403


def test_identidad_cedula_pin_y_matricula_anual_unica(entorno):
    cliente, _, h = entorno
    persona, anio, matricula = preparar_estudiante(cliente, h["admin"])
    assert persona["codigo"].startswith("E-") and len(persona["codigo"]) == 10
    assert len(persona["pinTemporal"]) == 6
    token = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "cedula": persona["cedula"],
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
            "becado": False,
        },
    )
    assert duplicada.status_code == 409
    assert matricula["persona_id"] == persona["id"]
    assert matricula["turno"] == "diurno"


def test_expediente_busqueda_estados_y_reinicio_de_pin(entorno):
    cliente, motor, h = entorno
    persona, anio, matricula = preparar_estudiante(cliente, h["admin"], cedula="701")
    ruta = cliente.post(
        "/api/v1/rutas",
        headers=h["admin"],
        json={"codigo": "0125", "descripcion": "Ruta San Jose", "colorHex": "#2563EB"},
    ).json()
    editada = cliente.put(
        f"/api/v1/personas/{persona['id']}",
        headers=h["admin"],
        json={"cedula": "701-A", "nombres": "Ana Maria Perez"},
    )
    assert editada.status_code == 409
    assert (
        cliente.put(
            f"/api/v1/matriculas/{matricula['id']}/beneficio-comedor",
            headers=h["admin"],
            json={"becado": True},
        ).status_code
        == 200
    )
    assert (
        cliente.put(
            f"/api/v1/matriculas/{matricula['id']}/ruta",
            headers=h["admin"],
            json={"rutaId": ruta["idRuta"]},
        ).status_code
        == 200
    )
    listado = cliente.get(
        "/api/v1/personas",
        headers=h["admin"],
        params={"buscar": "Ana", "estado": "activos", "tipo": "estudiante"},
    )
    fila = listado.json()["elementos"][0]
    assert fila["beneficioComedor"] == "Beneficiario"
    assert fila["beneficioTransporte"] == "Beneficiario – Ruta San Jose"
    assert fila["descripcionRuta"] == "Ruta San Jose"

    acceso = cliente.post(
        "/api/v1/autenticacion/portal", json={"cedula": "701", "pin": "123456"}
    ).json()
    reinicio = cliente.post(
        f"/api/v1/personas/{persona['id']}/reiniciar-pin", headers=h["admin"]
    )
    assert reinicio.status_code == 200 and len(reinicio.json()["pinTemporal"]) == 6
    assert cliente.get(
        "/api/v1/sesion", headers={"Authorization": f"Bearer {acceso['token']}"}
    ).status_code == 401
    assert cliente.post(
        "/api/v1/autenticacion/portal",
        json={"cedula": "701-A", "pin": reinicio.json()["pinTemporal"]},
    ).json()["cambioObligatorio"] is True
    assert cliente.post(
            "/api/v1/personas/pines/seccion",
            headers=h["admin"],
            json={"anioLectivoId": anio["id"], "seccion": "7-1"},
    ).status_code == 200
    assert cliente.post(
        f"/api/v1/personas/{persona['id']}/desactivar", headers=h["admin"]
    ).status_code == 200
    assert not cliente.get(
        "/api/v1/personas", headers=h["admin"], params={"estado": "activos"}
    ).json()["elementos"]
    assert cliente.get(
        "/api/v1/personas", headers=h["admin"], params={"estado": "inactivos"}
    ).json()["elementos"][0]["id"] == persona["id"]


def test_beneficios_atomicos_exigen_matricula_vigente_y_ruta_operativa(entorno):
    cliente, _, h = entorno
    _, _, matricula = preparar_estudiante(cliente, h["admin"], cedula="990")
    ruta = cliente.post(
        "/api/v1/rutas",
        headers=h["admin"],
        json={"codigo": "0990", "descripcion": "Ruta Beneficios", "colorHex": "#2563EB"},
    ).json()

    respuesta = cliente.put(
        f"/api/v1/matriculas/{matricula['id']}/beneficios",
        headers=h["admin"],
        json={"becado": True, "rutaId": ruta["idRuta"]},
    )
    assert respuesta.status_code == 200
    assert respuesta.json() == {
        "matriculaId": matricula["id"], "becado": True, "rutaId": ruta["idRuta"]
    }
    assert cliente.put(
        f"/api/v1/matriculas/{matricula['id']}/beneficios",
        headers=h["operador"], json={"becado": False, "rutaId": None},
    ).status_code == 403

    assert cliente.put(
        f"/api/v1/rutas/{ruta['idRuta']}",
        headers=h["admin"],
        json={"codigo": "0990", "descripcion": "Ruta Beneficios", "colorHex": "#2563EB", "activa": False},
    ).status_code == 200
    assert cliente.put(
        f"/api/v1/matriculas/{matricula['id']}/beneficios",
        headers=h["admin"], json={"becado": False, "rutaId": ruta["idRuta"]},
    ).status_code == 409

    assert cliente.post(
        "/api/v1/anios-lectivos", headers=h["admin"], json={"anio": 2027, "vigente": True}
    ).status_code == 201
    assert cliente.put(
        f"/api/v1/matriculas/{matricula['id']}/beneficios",
        headers=h["admin"], json={"becado": False, "rutaId": None},
    ).status_code == 409


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
            "cedula": persona["cedula"],
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
                "cedula": persona["cedula"],
                "pin": persona["pinTemporal"],
            },
        ).status_code
        == 401
    )
    nuevo = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "cedula": persona["cedula"],
            "pin": "654321",
        },
    )
    assert nuevo.status_code == 200 and nuevo.json()["cambioObligatorio"] is False
    nueva_cabecera = {"Authorization": f"Bearer {nuevo.json()['token']}"}
    assert cliente.post("/api/v1/autenticacion/logout", headers=nueva_cabecera).status_code == 204
    assert cliente.get("/api/v1/sesion", headers=nueva_cabecera).status_code == 401


def test_alta_manual_se_rechaza_aunque_los_datos_sean_validos(entorno):
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
    assert respuesta.status_code == 409


def test_un_solo_anio_vigente_y_rutas_sin_solape(entorno):
    cliente, _, h = entorno
    _, _, matricula = preparar_estudiante(cliente, h["admin"])
    segundo = cliente.post(
        "/api/v1/anios-lectivos",
        headers=h["admin"], json={"anio": 2027, "vigente": True},
    )
    assert segundo.status_code == 201
    vigentes = [
        a for a in cliente.get("/api/v1/anios-lectivos", headers=h["admin"]).json() if a["vigente"]
    ]
    assert [a["anio"] for a in vigentes] == [2027]
    ruta = cliente.post(
        "/api/v1/rutas",
        headers=h["admin"],
        json={"codigo": "1115306", "descripcion": "Ruta Norte MEP", "colorHex": "#F59E0B"},
    ).json()
    assert ruta["codigo"] == "1115306" and ruta["colorCarnetHex"] == "#F59E0B"
    datos = {"matriculaId": matricula["id"], "fechaInicio": "2026-02-01", "fechaFin": "2026-06-30"}
    assert (
        cliente.post(
            f"/api/v1/rutas/{ruta['idRuta']}/asignaciones", headers=h["admin"], json=datos
        ).status_code
        == 201
    )
    datos["fechaInicio"], datos["fechaFin"] = "2026-06-01", None
    assert (
        cliente.post(
            f"/api/v1/rutas/{ruta['idRuta']}/asignaciones", headers=h["admin"], json=datos
        ).status_code
        == 409
    )


def test_plantilla_semanal_es_menu_efectivo_y_sustitucion_tiene_prioridad(entorno):
    cliente, _, h = entorno
    ciclo = cliente.put(
        "/api/v1/menu/ciclo", headers=h["admin"], json={"inicioCicloMenu": "2026-08-03"}
    )
    assert ciclo.status_code == 200
    assert ciclo.json() == {"inicioCicloMenu": "2026-08-03"}
    plantilla = cliente.post(
        "/api/v1/menu/plantillas",
        headers=h["admin"],
        json={
            "semana": 5,
            "dia": 1,
            "titulo": "Tipico",
            "observaciones": "Menú de prueba",
            "activo": True,
            "componentes": [
                {"nombre": "Arroz", "tipo": "Principal", "orden": 1},
                {"nombre": "Frijoles", "tipo": "Acompañamiento", "orden": 2},
            ],
        },
    ).json()
    calendario = cliente.get(
        "/api/v1/menu/calendario?desde=2026-08-31&hasta=2026-08-31",
        headers=h["admin"],
    )
    assert calendario.status_code == 200
    assert calendario.json()[0]["origen"] == "plantilla"
    assert calendario.json()[0]["semana"] == 5
    assert calendario.json()[0]["dia"] == 1
    sustitucion = cliente.put(
        "/api/v1/menu/sustituciones/2026-08-31",
        headers=h["admin"],
        json={
            "fecha": "2026-08-31",
            "titulo": "Menú sustituido",
            "observaciones": "Cambio aprobado",
            "componentes": [{"nombre": "Pasta", "tipo": "Principal", "orden": 1}],
        },
    )
    assert sustitucion.status_code == 200
    assert plantilla["id"] > 0
    calendario = cliente.get(
        "/api/v1/menu/calendario?desde=2026-08-31&hasta=2026-08-31",
        headers=h["admin"],
    ).json()
    assert calendario[0]["origen"] == "sustitucion"
    assert calendario[0]["titulo"] == "Menú sustituido"

    septiembre = cliente.post(
        "/api/v1/menu/plantillas",
        headers=h["admin"],
        json={
            "semana": 1,
            "dia": 2,
            "titulo": "Primera semana de septiembre",
            "activo": True,
            "componentes": [{"nombre": "Gallo pinto", "tipo": "Principal", "orden": 1}],
        },
    )
    assert septiembre.status_code == 201
    calendario_septiembre = cliente.get(
        "/api/v1/menu/calendario?desde=2026-09-01&hasta=2026-09-01",
        headers=h["admin"],
    ).json()
    assert calendario_septiembre[0]["semana"] == 1
    assert calendario_septiembre[0]["titulo"] == "Primera semana de septiembre"
    final_mes = cliente.get(
        "/api/v1/menu/calendario?desde=2026-09-28&hasta=2026-09-30",
        headers=h["admin"],
    ).json()
    assert [dia["semana"] for dia in final_mes] == [4, 5, 5]


def test_calendario_menu_registra_excepcion_habilitada(entorno):
    cliente, _, h = entorno
    respuesta = cliente.put(
        "/api/v1/menu/calendario",
        headers=h["admin"],
        json={"fecha": "2026-09-15", "habilitado": False},
    )
    assert respuesta.status_code == 200
    dias = cliente.get(
        "/api/v1/menu/calendario?desde=2026-09-01&hasta=2026-09-30", headers=h["admin"]
    )
    assert dias.status_code == 200
    fin_de_semana = next(dia for dia in dias.json() if dia["fecha"] == "2026-09-05")
    assert fin_de_semana["esLectivo"] is False
    assert fin_de_semana["origen"] == "no_lectivo"
    assert fin_de_semana["titulo"] is None
    assert [{clave: valor for clave, valor in dia.items() if clave not in {"semana", "dia", "diaMes", "esLectivo", "componentes"}} for dia in dias.json() if dia["esLectivo"]] == [{
        "fecha": "2026-09-01", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-02", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-03", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-04", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-07", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-08", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-09", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-10", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-11", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-14", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-15", "habilitado": False, "motivo": None,
        "origen": "cerrado", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-16", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-17", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-18", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-21", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-22", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-23", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-24", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-25", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-28", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-29", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }, {
        "fecha": "2026-09-30", "habilitado": True, "motivo": None,
        "origen": "sin_menu", "titulo": None, "publicado": False, "tieneSustitucion": False,
    }]
