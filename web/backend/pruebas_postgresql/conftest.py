import asyncio
from datetime import date
from decimal import Decimal

import httpx
import pytest
from sqlalchemy.orm import Session

import aplicacion.modelos  # noqa: F401
from aplicacion.entrada import crear_aplicacion
from aplicacion.modelos.maestros import CuentaAdministrativa
from aplicacion.modelos.operacion import Tarifa
from aplicacion.nucleo.modelos_base import BaseDeclarativa
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.seguridad import hash_secreto
from config import Settings


class ClienteASGI:
    def __init__(self, app):
        self.app = app

    def request(self, metodo, ruta, **opciones):
        async def ejecutar():
            transporte = httpx.ASGITransport(app=self.app)
            async with httpx.AsyncClient(
                transport=transporte, base_url="http://pruebas"
            ) as cliente:
                return await cliente.request(metodo, ruta, **opciones)

        return asyncio.run(ejecutar())

    def get(self, ruta, **opciones):
        return self.request("GET", ruta, **opciones)

    def post(self, ruta, **opciones):
        return self.request("POST", ruta, **opciones)

    def delete(self, ruta, **opciones):
        return self.request("DELETE", ruta, **opciones)


@pytest.fixture
def entorno():
    motor = crear_motor("sqlite://")
    BaseDeclarativa.metadata.create_all(motor)
    with Session(motor) as sesion:
        sesion.add_all(
            [
                CuentaAdministrativa(
                    usuario="admin",
                    contrasena_hash=hash_secreto("Clave-segura-2026"),
                    rol="administrador",
                    activo=True,
                ),
                CuentaAdministrativa(
                    usuario="operador",
                    contrasena_hash=hash_secreto("Clave-operador-2026"),
                    rol="operador",
                    activo=True,
                ),
                Tarifa(
                    tipo_persona="estudiante", monto=Decimal("700"), fecha_inicio=date(2026, 1, 1)
                ),
                Tarifa(
                    tipo_persona="profesor", monto=Decimal("1000"), fecha_inicio=date(2026, 1, 1)
                ),
            ]
        )
        sesion.commit()
    app = crear_aplicacion(
        motor=motor,
        configuracion=Settings("postgresql+psycopg://no-usada", "http://localhost:5173", False),
    )
    cliente = ClienteASGI(app)
    admin = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={
            "usuario": "admin",
            "contrasena": "Clave-segura-2026",
        },
    ).json()["token"]
    operador = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={
            "usuario": "operador",
            "contrasena": "Clave-operador-2026",
        },
    ).json()["token"]
    yield (
        cliente,
        motor,
        {
            "admin": {"Authorization": f"Bearer {admin}"},
            "operador": {"Authorization": f"Bearer {operador}"},
        },
    )


def crear_persona(cliente, cabecera, *, tipo="estudiante", cedula="1", nombres="Ana Perez"):
    respuesta = cliente.post(
        "/api/v1/personas",
        headers=cabecera,
        json={
            "cedula": cedula,
            "nombres": nombres,
            "tipo": tipo,
        },
    )
    assert respuesta.status_code == 201, respuesta.text
    persona = respuesta.json()
    acceso = cliente.post(
        "/api/v1/autenticacion/portal",
        json={"codigo": persona["codigo"], "pin": persona["pinTemporal"]},
    ).json()
    cambio = cliente.post(
        "/api/v1/autenticacion/portal/pin",
        headers={"Authorization": f"Bearer {acceso['token']}"},
        json={"pinActual": persona["pinTemporal"], "pinNuevo": "123456"},
    )
    assert cambio.status_code == 200, cambio.text
    return persona


def preparar_estudiante(cliente, cabecera, cedula="1"):
    persona = crear_persona(cliente, cabecera, cedula=cedula)
    anio = cliente.post(
        "/api/v1/anios-lectivos", headers=cabecera, json={"anio": 2026, "vigente": True}
    ).json()
    matricula = cliente.post(
        "/api/v1/matriculas",
        headers=cabecera,
        json={
            "personaId": persona["id"],
            "anioLectivoId": anio["id"],
            "seccion": "7-1",
            "turno": "almuerzo",
            "becado": False,
        },
    ).json()
    return persona, anio, matricula
