"""Regresión concurrente optativa en PostgreSQL local exclusivamente sintético."""

import os
from concurrent.futures import ThreadPoolExecutor
from datetime import date
from threading import Barrier
from uuid import uuid4

import pytest
from fastapi import HTTPException
from sqlalchemy import func, select
from sqlalchemy.orm import Session

from aplicacion.esquemas import IngresoEntrada, ReservaEntrada
from aplicacion.modelos.maestros import Persona
from aplicacion.modelos.operacion import CuentaTiquete, IngresoComedor, MovimientoTiquete
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.repositorios_operacion import RepositorioOperacion
from aplicacion.servicios import ServicioOperacion


@pytest.fixture
def base_concurrente():
    puerto = os.environ.get("SCB_PUERTO_MEDICION")
    if not puerto:
        pytest.skip("Requiere PostgreSQL sintético: SCB_PUERTO_MEDICION")
    motor = crear_motor(f"postgresql+psycopg://postgres@127.0.0.1:{int(puerto)}/scb_medicion")
    codigo = "CONT-" + uuid4().hex[:20]
    with Session(motor) as sesion:
        persona = Persona(cedula=codigo, nombres="Contención sintética", tipo="profesor")
        sesion.add(persona)
        sesion.flush()
        persona_id = persona.id
        sesion.add(CuentaTiquete(persona_id=persona_id, saldo=10, reservados=0))
        sesion.commit()
    yield motor, codigo, persona_id
    motor.dispose()


def ejecutar(motor, codigo, acciones):
    barrera = Barrier(len(acciones))

    def operacion(accion):
        with Session(motor) as sesion:
            servicio = ServicioOperacion(RepositorioOperacion(sesion))
            datos = ReservaEntrada(cedula=codigo, fecha=date(2026, 8, 20))
            barrera.wait(timeout=15)
            try:
                if accion == "ingresar":
                    servicio.ingresar(IngresoEntrada(cedula=codigo, fecha=datos.fecha), 1)
                else:
                    getattr(servicio, accion)(datos, {"tipo": "administracion"})
                sesion.commit()
                return 200
            except HTTPException as error:
                sesion.rollback()
                return error.status_code

    with ThreadPoolExecutor(max_workers=len(acciones)) as pool:
        return list(pool.map(operacion, acciones))


def saldo(motor, persona_id):
    with Session(motor) as sesion:
        cuenta = sesion.get(CuentaTiquete, persona_id)
        movimientos = sesion.scalar(select(func.count()).select_from(MovimientoTiquete).where(
            MovimientoTiquete.persona_id == persona_id))
        ingresos = sesion.scalar(select(func.count()).select_from(IngresoComedor).where(
            IngresoComedor.persona_id == persona_id))
        return cuenta.saldo, cuenta.reservados, movimientos, ingresos


def test_confirmaciones_simultaneas(base_concurrente):
    motor, codigo, persona_id = base_concurrente
    estados = ejecutar(motor, codigo, ["reservar"] * 4)
    assert sorted(estados) == [200, 409, 409, 409]
    assert saldo(motor, persona_id) == (9, 1, 1, 0)


def test_cancelaciones_simultaneas(base_concurrente):
    motor, codigo, persona_id = base_concurrente
    ejecutar(motor, codigo, ["reservar"])
    assert ejecutar(motor, codigo, ["cancelar"] * 4) == [200] * 4
    assert saldo(motor, persona_id) == (10, 0, 2, 0)


def test_ingresos_simultaneos(base_concurrente):
    motor, codigo, persona_id = base_concurrente
    ejecutar(motor, codigo, ["reservar"])
    assert sorted(ejecutar(motor, codigo, ["ingresar"] * 4)) == [200, 409, 409, 409]
    assert saldo(motor, persona_id) == (9, 0, 2, 1)
    assert ejecutar(motor, codigo, ["reservar"]) == [409]


def test_cancelar_e_ingresar_simultaneamente(base_concurrente):
    motor, codigo, persona_id = base_concurrente
    ejecutar(motor, codigo, ["reservar"])
    assert ejecutar(motor, codigo, ["cancelar", "ingresar"]) == [200, 200]
    disponible, reservados, movimientos, ingresos = saldo(motor, persona_id)
    assert (disponible, reservados, ingresos) == (9, 0, 1)
    assert movimientos in {2, 3}
