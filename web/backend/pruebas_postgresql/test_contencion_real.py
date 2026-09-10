"""Regresión concurrente optativa en PostgreSQL local exclusivamente sintético."""

import json
import os
from concurrent.futures import ThreadPoolExecutor
from datetime import date, datetime, timedelta, timezone
from threading import Barrier
from uuid import uuid4

import pytest
from fastapi import HTTPException
from sqlalchemy import func, select
from sqlalchemy.orm import Session

from aplicacion.esquemas import IngresoEntrada, ReservaEntrada
from aplicacion.modelos.maestros import CuentaAdministrativa, Persona
from aplicacion.modelos.operacion import (
    CuentaTiquete,
    IngresoComedor,
    MovimientoTiquete,
    TrabajoImportacion,
)
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.repositorios_operacion import RepositorioOperacion
from aplicacion.servicios import ServicioOperacion


@pytest.fixture
def base_concurrente():
    url = os.environ.get("SCB_DATABASE_URL")
    puerto = os.environ.get("SCB_PUERTO_MEDICION")
    if not url and not puerto:
        pytest.skip("Requiere PostgreSQL sintético: SCB_DATABASE_URL o SCB_PUERTO_MEDICION")
    motor = crear_motor(
        url or f"postgresql+psycopg://postgres@127.0.0.1:{int(puerto)}/scb_medicion"
    )
    codigo = "CONT-" + uuid4().hex[:20]
    with Session(motor) as sesion:
        persona = Persona(cedula=codigo, nombres="Contención sintética", tipo="profesor")
        sesion.add(persona)
        sesion.flush()
        persona_id = persona.id
        sesion.add(CuentaTiquete(persona_id=persona_id, saldo=10, reservados=0))
        operador = CuentaAdministrativa(
            persona_id=persona_id,
            usuario=f"operador-{uuid4().hex[:16]}",
            contrasena_hash="solo-prueba",
            rol="administrador",
            activo=True,
            vinculacion_pendiente=False,
        )
        sesion.add(operador)
        sesion.flush()
        operador_id = operador.id
        sesion.commit()
    yield motor, codigo, persona_id, operador_id
    motor.dispose()


def ejecutar(motor, codigo, operador_id, acciones):
    barrera = Barrier(len(acciones))

    def operacion(accion):
        with Session(motor) as sesion:
            servicio = ServicioOperacion(RepositorioOperacion(sesion))
            persona = servicio._persona(cedula=codigo)
            datos = ReservaEntrada(fecha=date(2026, 8, 20))
            barrera.wait(timeout=15)
            try:
                if accion == "ingresar":
                    servicio.ingresar(IngresoEntrada(cedula=codigo, fecha=datos.fecha), operador_id)
                else:
                    getattr(servicio, accion)(datos, {"tipo": "portal", "persona": persona})
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
        movimientos = sesion.scalar(
            select(func.count())
            .select_from(MovimientoTiquete)
            .where(MovimientoTiquete.persona_id == persona_id)
        )
        ingresos = sesion.scalar(
            select(func.count())
            .select_from(IngresoComedor)
            .where(IngresoComedor.persona_id == persona_id)
        )
        return cuenta.saldo, cuenta.reservados, movimientos, ingresos


def test_confirmaciones_simultaneas(base_concurrente):
    motor, codigo, persona_id, operador_id = base_concurrente
    estados = ejecutar(motor, codigo, operador_id, ["reservar"] * 4)
    assert sorted(estados) == [200, 409, 409, 409]
    assert saldo(motor, persona_id) == (9, 1, 1, 0)


def test_cancelaciones_simultaneas(base_concurrente):
    motor, codigo, persona_id, operador_id = base_concurrente
    ejecutar(motor, codigo, operador_id, ["reservar"])
    assert ejecutar(motor, codigo, operador_id, ["cancelar"] * 4) == [200] * 4
    assert saldo(motor, persona_id) == (10, 0, 2, 0)


def test_ingresos_simultaneos(base_concurrente):
    motor, codigo, persona_id, operador_id = base_concurrente
    ejecutar(motor, codigo, operador_id, ["reservar"])
    assert sorted(ejecutar(motor, codigo, operador_id, ["ingresar"] * 4)) == [200, 409, 409, 409]
    assert saldo(motor, persona_id) == (9, 0, 2, 1)
    assert ejecutar(motor, codigo, operador_id, ["reservar"]) == [409]


def test_cancelar_e_ingresar_simultaneamente(base_concurrente):
    motor, codigo, persona_id, operador_id = base_concurrente
    ejecutar(motor, codigo, operador_id, ["reservar"])
    assert ejecutar(motor, codigo, operador_id, ["cancelar", "ingresar"]) == [200, 200]
    disponible, reservados, movimientos, ingresos = saldo(motor, persona_id)
    assert (disponible, reservados, ingresos) == (9, 0, 1)
    assert movimientos in {2, 3}


def test_confirmar_y_cancelar_simultaneamente(base_concurrente):
    motor, codigo, persona_id, operador_id = base_concurrente
    assert ejecutar(motor, codigo, operador_id, ["reservar", "cancelar"]) == [200, 200]
    assert saldo(motor, persona_id) in {(10, 0, 2, 0), (9, 1, 1, 0)}


def test_confirmar_e_ingresar_simultaneamente(base_concurrente):
    motor, codigo, persona_id, operador_id = base_concurrente
    estados = ejecutar(motor, codigo, operador_id, ["reservar", "ingresar"])
    assert estados in ([200, 200], [409, 200])
    disponible, reservados, movimientos, ingresos = saldo(motor, persona_id)
    assert (disponible, reservados, ingresos) == (9, 0, 1)
    assert movimientos in {1, 2}


def test_un_solo_worker_reclama_un_trabajo_de_importacion(base_concurrente):
    motor, codigo, _, operador_id = base_concurrente
    with Session(motor) as sesion:
        trabajo = TrabajoImportacion(
            huella=uuid4().hex,
            cuenta_solicitante_id=operador_id,
            entrada_json=json.dumps({"anio": 2030, "filas": []}),
            resumen_json=json.dumps({"total": 0, "altas": 0, "cambios": 0}),
            estado="pendiente",
        )
        sesion.add(trabajo)
        sesion.commit()
        trabajo_id = trabajo.id

    barrera = Barrier(4)

    def reclamar():
        with Session(motor) as sesion:
            barrera.wait(timeout=15)
            trabajo = RepositorioImportacion(sesion).tomar_trabajo_pendiente()
            sesion.commit()
            return trabajo.id if trabajo is not None else None

    with ThreadPoolExecutor(max_workers=4) as pool:
        reclamos = list(pool.map(lambda _: reclamar(), range(4)))

    assert reclamos.count(trabajo_id) == 1
    assert reclamos.count(None) == 3


def test_recuperacion_no_reclama_trabajo_bloqueado_vivo(base_concurrente):
    motor, _, _, actor = base_concurrente
    with Session(motor) as sesion:
        trabajo = TrabajoImportacion(
            huella=uuid4().hex,
            cuenta_solicitante_id=actor,
            entrada_json="{}",
            resumen_json="{}",
            estado="ejecutando",
            iniciado_en=datetime.now(timezone.utc) - timedelta(hours=1),
        )
        sesion.add(trabajo)
        sesion.commit()
        identificador = trabajo.id
    with Session(motor) as activo, Session(motor) as recuperador:
        RepositorioImportacion(activo).trabajo_para_entrega(identificador)
        RepositorioImportacion(recuperador).recuperar_trabajos_interrumpidos(
            datetime.now(timezone.utc)
        )
        recuperador.commit()
        assert recuperador.get(TrabajoImportacion, identificador).estado == "ejecutando"
    with Session(motor) as sesion:
        RepositorioImportacion(sesion).recuperar_trabajos_interrumpidos(datetime.now(timezone.utc))
        sesion.commit()
        assert sesion.get(TrabajoImportacion, identificador).estado == "pendiente"
