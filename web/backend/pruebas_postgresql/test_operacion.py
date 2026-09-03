from datetime import date

from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import HorarioReserva, Matricula
from aplicacion.modelos.operacion import CuentaTiquete, MovimientoTiquete

from .conftest import crear_persona, preparar_estudiante


def _vender(cliente, h, cedula, cantidad=2):
    respuesta = cliente.post(
        "/api/v1/tiquetes/ventas",
        headers=h["operador"],
        json={
            "cedula": cedula,
            "cantidad": cantidad,
            "medioPago": "efectivo",
        },
    )
    assert respuesta.status_code == 201, respuesta.text


def test_reserva_inmoviliza_cancelar_libera_e_ingreso_consume(entorno):
    cliente, motor, h = entorno
    persona, _, _ = preparar_estudiante(cliente, h["admin"])
    _vender(cliente, h, persona["codigo"])
    token = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "cedula": persona["cedula"],
            "pin": "123456",
        },
    ).json()["token"]
    hp = {"Authorization": f"Bearer {token}"}
    cliente.post(
        "/api/v1/comedor/reservas",
        headers=hp,
        json={"fecha": "2026-09-01"},
    ).json()
    with Session(motor) as sesion:
        cuenta = sesion.get(CuentaTiquete, persona["id"])
        assert (cuenta.saldo, cuenta.reservados) == (1, 1)
    assert (
        cliente.delete(
            "/api/v1/comedor/reservas",
            headers=hp,
            json={"fecha": "2026-09-01"},
        ).status_code
        == 204
    )
    with Session(motor) as sesion:
        cuenta = sesion.get(CuentaTiquete, persona["id"])
        assert (cuenta.saldo, cuenta.reservados) == (2, 0)
    cliente.post(
        "/api/v1/comedor/reservas",
        headers=hp,
        json={"fecha": "2026-09-02"},
    ).json()
    ingreso = cliente.post(
        "/api/v1/comedor/operacion",
        headers=h["operador"],
        json={
            "cedula": persona["cedula"],
            "fecha": "2026-09-02",
        },
    )
    assert ingreso.status_code == 201 and ingreso.json()["modalidad"] == "reserva"
    with Session(motor) as sesion:
        cuenta = sesion.get(CuentaTiquete, persona["id"])
        assert (cuenta.saldo, cuenta.reservados) == (1, 0)
        assert [m.tipo for m in sesion.query(MovimientoTiquete).all()] == [
            "venta",
            "reserva",
            "liberacion",
            "reserva",
            "consumo",
        ]
    duplicado = cliente.post(
        "/api/v1/comedor/operacion",
        headers=h["operador"],
        json={"cedula": persona["cedula"], "fecha": "2026-09-02"},
    )
    assert duplicado.status_code == 409
    assert duplicado.json()["resultado"] == "duplicado"
    estado = cliente.get(
        "/api/v1/comedor/operacion/estado",
        headers=h["operador"],
        params={"fecha": "2026-09-02"},
    ).json()
    assert estado["ingresos"] == 1 and estado["duplicados"] == 1
    assert estado["recientes"][0]["codigo"] == persona["codigo"]


def test_estudiante_sin_reserva_exige_decision_y_profesor_no(entorno):
    cliente, _, h = entorno
    estudiante, _, _ = preparar_estudiante(cliente, h["admin"])
    _vender(cliente, h, estudiante["cedula"], 1)
    datos = {"cedula": estudiante["cedula"], "fecha": "2026-09-03"}
    sin_reserva = cliente.post("/api/v1/comedor/operacion", headers=h["operador"], json=datos)
    assert sin_reserva.status_code == 409
    assert sin_reserva.json()["resultado"] == "sin_reserva"
    cliente.post(
        "/api/v1/comedor/autorizaciones",
        headers=h["operador"],
        json={
            **datos,
            "decision": "aprobada",
            "motivo": "remanente disponible",
        },
    )
    assert (
        cliente.post("/api/v1/comedor/operacion", headers=h["operador"], json=datos).status_code
        == 201
    )
    profesor = crear_persona(cliente, h["admin"], tipo="profesor", cedula="9", nombres="Docente")
    _vender(cliente, h, profesor["cedula"], 1)
    directo = cliente.post(
        "/api/v1/comedor/operacion",
        headers=h["operador"],
        json={
            "cedula": profesor["cedula"],
            "fecha": "2026-09-03",
        },
    )
    assert directo.status_code == 201 and directo.json()["modalidad"] == "directo_profesor"


def test_confirmacion_sin_tiquetes_se_muestra_en_portal_y_no_autoriza_ingreso(entorno):
    cliente, motor, h = entorno
    persona, _, _ = preparar_estudiante(cliente, h["admin"])
    with Session(motor) as sesion:
        sesion.add(HorarioReserva(turno="general", hora_limite="23:59"))
        sesion.commit()
    token = cliente.post(
        "/api/v1/autenticacion/portal",
        json={"cedula": persona["cedula"], "pin": "123456"},
    ).json()["token"]
    portal = {"Authorization": f"Bearer {token}"}
    fecha = date.today().isoformat()

    reserva = cliente.post("/api/v1/comedor/reservas", headers=portal, json={"fecha": fecha})
    assert reserva.status_code == 201, reserva.text
    assert reserva.json()["sin_tiquete"] is True

    estado = cliente.get("/api/v1/portal/estado", headers=portal, params={"fecha": fecha})
    assert estado.status_code == 200, estado.text
    assert estado.json()["estado"]["horaLimite"] == "23:59"
    assert estado.json()["estado"]["sinTiquete"] is True

    ingreso = cliente.post(
        "/api/v1/comedor/operacion",
        headers=h["operador"],
        json={"cedula": persona["cedula"], "fecha": fecha},
    )
    assert ingreso.status_code == 409
    assert ingreso.json()["resultado"] == "sin_tiquete"


def test_foto_de_comedor_es_accesible_al_operador_y_no_expone_otros_datos(entorno):
    cliente, _, h = entorno
    persona, _, _ = preparar_estudiante(cliente, h["admin"])

    respuesta = cliente.get(
        f"/api/v1/comedor/personas/{persona['id']}/foto",
        headers=h["operador"],
    )

    assert respuesta.status_code == 404
    assert respuesta.content == b""


def test_beca_es_anual_y_no_consume_saldo(entorno):
    cliente, motor, h = entorno
    persona, _, _ = preparar_estudiante(cliente, h["admin"])
    with Session(motor) as sesion:
        sesion.query(Matricula).filter_by(persona_id=persona["id"]).one().becado = True
        sesion.commit()
    token = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "cedula": persona["cedula"],
            "pin": "123456",
        },
    ).json()["token"]
    hp = {"Authorization": f"Bearer {token}"}
    assert (
        cliente.post(
            "/api/v1/comedor/reservas",
            headers=hp,
            json={"fecha": "2026-09-04"},
        ).status_code
        == 201
    )
    assert (
        cliente.post(
            "/api/v1/comedor/operacion",
            headers=h["operador"],
            json={
                "cedula": persona["cedula"],
                "fecha": "2026-09-04",
            },
        ).json()["consumioTiquete"]
        is False
    )


def test_captura_transporte_no_se_publica_hasta_etapa_dos(entorno):
    cliente, _, h = entorno
    persona, _, matricula = preparar_estudiante(cliente, h["admin"])
    ruta = cliente.post(
        "/api/v1/rutas",
        headers=h["admin"],
        json={"codigo": "5369", "descripcion": "Ruta Sur", "colorHex": "#EF4444"},
    ).json()
    cliente.post(
        f"/api/v1/rutas/{ruta['idRuta']}/asignaciones",
        headers=h["admin"],
        json={
            "matriculaId": matricula["id"],
            "fechaInicio": "2026-01-01",
        },
    )
    datos = {"cedula": persona["cedula"], "fecha": "2026-09-05"}
    assert cliente.post("/api/v1/transporte/marcas", headers=h["operador"], json=datos).status_code == 404
    # Sin captura pública, se conserva la regla de estudiante sin reserva.
    assert (
        cliente.post(
            "/api/v1/comedor/operacion",
            headers=h["operador"],
            json={
                "cedula": persona["cedula"],
                "fecha": "2026-09-05",
            },
        ).status_code
        == 409
    )
