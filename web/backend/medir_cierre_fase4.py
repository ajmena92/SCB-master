"""Ensayo durable con commit y entrega única en PostgreSQL exclusivamente efímero.

Preparar scb_medicion con Alembic. No usar sobre un padrón existente:
la importación anual desactiva ausentes. Solo acepta localhost:55439/scb_medicion.
"""

import asyncio
import json
import multiprocessing as mp
import resource
import secrets
import time

import httpx
from cryptography.fernet import Fernet
from sqlalchemy import func, select
from sqlalchemy.orm import Session

from aplicacion.entrada import crear_aplicacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.modelos.maestros import CuentaAdministrativa, Matricula, Persona
from aplicacion.nucleo.postgresql import crear_fabrica_sesiones, crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.seguridad import hash_secreto
from aplicacion.trabajos_importacion import procesar_un_trabajo
from config import Settings

URL = "postgresql+psycopg://postgres@127.0.0.1:55439/scb_medicion"


def trabajador(clave, canal):
    motor = crear_motor(URL)
    inicio = time.perf_counter()
    cpu = time.process_time()
    trabajo_id = procesar_un_trabajo(crear_fabrica_sesiones(motor), clave)
    canal.send(
        {
            "trabajo_id": trabajo_id,
            "worker_segundos": round(time.perf_counter() - inicio, 2),
            "cpu_segundos": round(time.process_time() - cpu, 2),
            "rss_worker_mib": round(resource.getrusage(resource.RUSAGE_SELF).ru_maxrss / 1024, 2),
        }
    )
    motor.dispose()


async def main():
    motor = crear_motor(URL)
    clave = Fernet.generate_key().decode()
    contrasena = secrets.token_urlsafe(24)
    with Session(motor) as sesion:
        # Exige base sin estudiantes para impedir desactivaciones accidentales.
        assert (
            sesion.scalar(
                select(func.count()).select_from(Persona).where(Persona.tipo == "estudiante")
            )
            == 0
        ), "El ensayo exige una base sin estudiantes"
        persona = Persona(cedula="CIERRE-ADMIN", nombres="Medición", tipo="profesor")
        sesion.add(persona)
        sesion.flush()
        cuenta = CuentaAdministrativa(
            persona_id=persona.id,
            usuario="cierre-f4",
            contrasena_hash=hash_secreto(contrasena),
            rol="administrador",
            activo=True,
            vinculacion_pendiente=False,
        )
        sesion.add(cuenta)
        sesion.commit()
    app = crear_aplicacion(
        motor=motor,
        configuracion=Settings(
            database_url=URL,
            cors_origin="http://pruebas",
            cookie_secure=False,
            csrf_secret=secrets.token_urlsafe(32),
            carnet_qr_clave=clave,
            importacion_resultados_key=clave,
        ),
    )
    datos = ImportacionEntrada(
        anio=2026,
        filas=[
            {
                "cedula": f"CIERRE-{i:05d}",
                "nombres": f"Estudiante sintético {i}",
                "tipo": "estudiante",
                "seccion": "7-1",
            }
            for i in range(2500)
        ],
    )
    async with httpx.AsyncClient(
        transport=httpx.ASGITransport(app=app), base_url="http://pruebas", timeout=60
    ) as cliente:

        async def post(ruta, datos=None):
            await cliente.get("/api/v1/autenticacion/csrf")
            return await cliente.post(
                "/api/v1/" + ruta,
                json=datos,
                headers={"Origin": "http://pruebas", "X-CSRF-Token": cliente.cookies["csrf_token"]},
            )

        assert (
            await post(
                "autenticacion/administracion", {"usuario": "cierre-f4", "contrasena": contrasena}
            )
        ).status_code == 200
        inicio = time.perf_counter()
        previa = await post("importaciones/previsualizar", datos.model_dump())
        assert previa.status_code == 200, previa.text
        previa_ms = (time.perf_counter() - inicio) * 1000
        cuerpo = {**datos.model_dump(), "huella": previa.json()["huella"]}
        inicio = time.perf_counter()
        respuesta = await post("importaciones/confirmar", cuerpo)
        assert respuesta.status_code == 202, respuesta.text
        cola_ms = (time.perf_counter() - inicio) * 1000
        trabajo_id = respuesta.json()["trabajoId"]
        assert (await post("importaciones/confirmar", cuerpo)).json()["trabajoId"] == trabajo_id
        contexto = mp.get_context("spawn")
        receptor, emisor = contexto.Pipe(duplex=False)
        proceso = contexto.Process(target=trabajador, args=(clave, emisor))
        proceso.start()
        estados = set()
        limite = time.monotonic() + 900
        while proceso.is_alive() and time.monotonic() < limite:
            actual = await cliente.get(f"/api/v1/importaciones/trabajos/{trabajo_id}")
            assert actual.status_code == 200
            estados.add(actual.json()["estado"])
            await asyncio.sleep(1)
        if proceso.is_alive():
            proceso.terminate()
            proceso.join()
            raise AssertionError("El trabajador superó 15 minutos")
        proceso.join()
        assert proceso.exitcode == 0
        metricas = receptor.recv()
        actual = await cliente.get(f"/api/v1/importaciones/trabajos/{trabajo_id}")
        assert actual.json()["estado"] == "completado"
        with Session(motor) as sesion:
            assert sesion.scalar(select(func.count()).select_from(Matricula)) == 2500
            trabajo = RepositorioImportacion(sesion).trabajo(trabajo_id)
            assert trabajo.resultado_cifrado and "pinTemporal" not in trabajo.resultado_cifrado
        entrega = await post(f"importaciones/trabajos/{trabajo_id}/credenciales")
        assert entrega.status_code == 200
        assert len(entrega.json()["credenciales"]) == 2500
        assert (await post(f"importaciones/trabajos/{trabajo_id}/credenciales")).status_code == 410
        tiempos = []
        for _ in range(10):
            inicio = time.perf_counter()
            assert (
                await post(
                    "autenticacion/administracion",
                    {"usuario": "cierre-f4", "contrasena": contrasena},
                )
            ).status_code == 200
            tiempos.append((time.perf_counter() - inicio) * 1000)
        comedor = await cliente.get("/api/v1/comedor/operacion/estado?fecha=2026-09-07")
        assert comedor.status_code == 200, comedor.text
        print(
            json.dumps(
                {
                    **metricas,
                    "filas_persistidas": 2500,
                    "previsualizacion_ms": round(previa_ms, 2),
                    "encolado_ms": round(cola_ms, 2),
                    "login_p95_posterior_ms": round(sorted(tiempos)[-1], 2),
                    "estados_observados": sorted(estados),
                    "entrega_unica": True,
                    "transporte": "HTTP ASGI; worker en proceso separado",
                    "comedor_posterior": 200,
                },
                indent=2,
            )
        )
    motor.dispose()


if __name__ == "__main__":
    asyncio.run(main())
