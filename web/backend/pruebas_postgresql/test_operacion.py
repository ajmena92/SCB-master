from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import Matricula
from aplicacion.modelos.operacion import CuentaTiquete, MovimientoTiquete

from .conftest import crear_persona, preparar_estudiante


def _vender(cliente, h, codigo, cantidad=2):
    respuesta = cliente.post(
        "/api/v1/tiquetes/ventas",
        headers=h["operador"],
        json={
            "codigo": codigo,
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
            "codigo": persona["codigo"],
            "pin": "123456",
        },
    ).json()["token"]
    hp = {"Authorization": f"Bearer {token}"}
    cliente.post(
        "/api/v1/comedor/reservas",
        headers=hp,
        json={"codigo": persona["codigo"], "fecha": "2026-09-01"},
    ).json()
    with Session(motor) as sesion:
        cuenta = sesion.get(CuentaTiquete, persona["id"])
        assert (cuenta.saldo, cuenta.reservados) == (1, 1)
    assert (
        cliente.delete(
            "/api/v1/comedor/reservas",
            headers=hp,
            json={"codigo": persona["codigo"], "fecha": "2026-09-01"},
        ).status_code
        == 204
    )
    with Session(motor) as sesion:
        cuenta = sesion.get(CuentaTiquete, persona["id"])
        assert (cuenta.saldo, cuenta.reservados) == (2, 0)
    cliente.post(
        "/api/v1/comedor/reservas",
        headers=hp,
        json={"codigo": persona["codigo"], "fecha": "2026-09-02"},
    ).json()
    ingreso = cliente.post(
        "/api/v1/comedor/operacion",
        headers=h["operador"],
        json={
            "codigo": persona["codigo"],
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


def test_estudiante_sin_reserva_exige_decision_y_profesor_no(entorno):
    cliente, _, h = entorno
    estudiante, _, _ = preparar_estudiante(cliente, h["admin"])
    _vender(cliente, h, estudiante["codigo"], 1)
    datos = {"codigo": estudiante["codigo"], "fecha": "2026-09-03"}
    assert (
        cliente.post("/api/v1/comedor/operacion", headers=h["operador"], json=datos).status_code
        == 409
    )
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
    _vender(cliente, h, profesor["codigo"], 1)
    directo = cliente.post(
        "/api/v1/comedor/operacion",
        headers=h["operador"],
        json={
            "codigo": profesor["codigo"],
            "fecha": "2026-09-03",
        },
    )
    assert directo.status_code == 201 and directo.json()["modalidad"] == "directo_profesor"


def test_beca_es_anual_y_no_consume_saldo(entorno):
    cliente, motor, h = entorno
    persona, _, _ = preparar_estudiante(cliente, h["admin"])
    with Session(motor) as sesion:
        sesion.query(Matricula).filter_by(persona_id=persona["id"]).one().becado = True
        sesion.commit()
    token = cliente.post(
        "/api/v1/autenticacion/portal",
        json={
            "codigo": persona["codigo"],
            "pin": "123456",
        },
    ).json()["token"]
    hp = {"Authorization": f"Bearer {token}"}
    assert (
        cliente.post(
            "/api/v1/comedor/reservas",
            headers=hp,
            json={"codigo": persona["codigo"], "fecha": "2026-09-04"},
        ).status_code
        == 201
    )
    assert (
        cliente.post(
            "/api/v1/comedor/operacion",
            headers=h["operador"],
            json={
                "codigo": persona["codigo"],
                "fecha": "2026-09-04",
            },
        ).json()["consumio_tiquete"]
        is False
    )


def test_transporte_es_informativo_y_rechaza_doble_marca(entorno):
    cliente, _, h = entorno
    persona, _, matricula = preparar_estudiante(cliente, h["admin"])
    ruta = cliente.post("/api/v1/rutas", headers=h["admin"], json={"nombre": "Ruta Sur"}).json()
    cliente.post(
        f"/api/v1/rutas/{ruta['id']}/asignaciones",
        headers=h["admin"],
        json={
            "matriculaId": matricula["id"],
            "fechaInicio": "2026-01-01",
        },
    )
    datos = {"codigo": persona["codigo"], "fecha": "2026-09-05"}
    assert (
        cliente.post("/api/v1/transporte/marcas", headers=h["operador"], json=datos).status_code
        == 201
    )
    assert (
        cliente.post("/api/v1/transporte/marcas", headers=h["operador"], json=datos).status_code
        == 409
    )
    # La marca no autoriza comedor: sigue aplicando la regla de estudiante sin reserva.
    assert (
        cliente.post(
            "/api/v1/comedor/operacion",
            headers=h["operador"],
            json={
                "codigo": persona["codigo"],
                "fecha": "2026-09-05",
            },
        ).status_code
        == 409
    )
