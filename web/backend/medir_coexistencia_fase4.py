"""Mide login y comedor mientras una importación usa Argon2 en PostgreSQL sintético."""

import argparse
import asyncio
import base64
import json
import os
import secrets
import threading
import time

import httpx
from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.entrada import crear_aplicacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.modelos.maestros import AnioLectivo, CuentaAdministrativa, Persona
from aplicacion.modelos.operacion import CuentaTiquete
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.seguridad import hash_secreto
from config import Settings


def percentil_95(valores: list[float]) -> float:
    return round(sorted(valores)[max(0, int(len(valores) * 0.95) - 1)], 2)


def crear_datos(motor) -> tuple[str, str, int]:
    sufijo = secrets.token_hex(6)
    usuario, contrasena = f"fase4-{sufijo}", secrets.token_urlsafe(24)
    with Session(motor) as sesion:
        anio_vigente = sesion.scalar(select(AnioLectivo).where(AnioLectivo.vigente.is_(True)))
        if anio_vigente is None:
            anio_vigente = AnioLectivo(anio=2032, vigente=True)
            sesion.add(anio_vigente)
        anio = anio_vigente.anio
        administrador = Persona(cedula=f"F4-A-{sufijo}", nombres="Administrador Fase 4", tipo="profesor")
        comedor = Persona(cedula=f"F4-C-{sufijo}", nombres="Comedor Fase 4", tipo="profesor")
        sesion.add_all([administrador, comedor])
        sesion.flush()
        sesion.add_all(
            [
                CuentaAdministrativa(
                    persona_id=administrador.id,
                    usuario=usuario,
                    contrasena_hash=hash_secreto(contrasena),
                    rol="administrador",
                    activo=True,
                    vinculacion_pendiente=False,
                ),
                CuentaTiquete(persona_id=comedor.id, saldo=10, reservados=0),
            ]
        )
        sesion.commit()
    return usuario, contrasena, anio


def importar(motor, anio: int, filas: int, resultado: dict[str, float]) -> None:
    datos = ImportacionEntrada.model_validate(
        {
            "anio": anio,
            "filas": [
                {
                    "cedula": f"F4-I-{secrets.token_hex(10)}",
                    "nombres": "Alta sintética",
                    "tipo": "profesor",
                }
                for _ in range(filas)
            ],
        }
    )
    with Session(motor) as sesion:
        servicio = ServicioImportacion(RepositorioImportacion(sesion))
        previa = servicio.previsualizar(datos)
        inicio = time.perf_counter()
        servicio.confirmar(datos, previa["huella"])
        sesion.commit()
        resultado["importacion_ms"] = round((time.perf_counter() - inicio) * 1000, 2)


async def medir(url: str, filas: int, muestras: int) -> None:
    motor = crear_motor(url)
    usuario, contrasena, anio = crear_datos(motor)
    aplicacion = crear_aplicacion(
        motor=motor,
        configuracion=Settings(
            database_url=url,
            cookie_secure=False,
            cors_origin="http://localhost:5173",
            csrf_secret=secrets.token_urlsafe(32),
            carnet_qr_clave=base64.b64encode(secrets.token_bytes(32)).decode(),
        ),
    )
    resultado_importacion: dict[str, float] = {}
    hilo = threading.Thread(target=importar, args=(motor, anio, filas, resultado_importacion))
    transporte = httpx.ASGITransport(app=aplicacion)

    async def login() -> tuple[int, float]:
        async with httpx.AsyncClient(transport=transporte, base_url="http://pruebas") as cliente:
            csrf = await cliente.get("/api/v1/autenticacion/csrf")
            inicio = time.perf_counter()
            respuesta = await cliente.post(
                "/api/v1/autenticacion/administracion",
                json={"usuario": usuario, "contrasena": contrasena},
                headers={"Origin": "http://localhost:5173", "X-CSRF-Token": csrf.cookies["csrf_token"]},
            )
            return respuesta.status_code, (time.perf_counter() - inicio) * 1000

    async with httpx.AsyncClient(transport=transporte, base_url="http://pruebas") as cliente:
        csrf = await cliente.get("/api/v1/autenticacion/csrf")
        sesion = await cliente.post(
            "/api/v1/autenticacion/administracion",
            json={"usuario": usuario, "contrasena": contrasena},
            headers={"Origin": "http://localhost:5173", "X-CSRF-Token": csrf.cookies["csrf_token"]},
        )
        assert sesion.status_code == 200
        hilo.start()
        login_resultados = await asyncio.gather(*(login() for _ in range(muestras)))
        comedor_tiempos: list[float] = []
        estados: list[int] = []
        for _ in range(muestras):
            inicio = time.perf_counter()
            respuesta = await cliente.get(
                "/api/v1/comedor/operacion/estado", params={"fecha": f"{anio}-01-01"}
            )
            comedor_tiempos.append((time.perf_counter() - inicio) * 1000)
            estados.append(respuesta.status_code)
    hilo.join(timeout=120)
    assert not hilo.is_alive() and "importacion_ms" in resultado_importacion
    assert all(estado == 200 for estado, _ in login_resultados)
    assert estados == [200] * muestras
    print(
        json.dumps(
            {
                "filas_importacion": filas,
                "muestras": muestras,
                "importacion_ms": resultado_importacion["importacion_ms"],
                "login_p95_ms": percentil_95([tiempo for _, tiempo in login_resultados]),
                "comedor_p95_ms": percentil_95(comedor_tiempos),
            },
            indent=2,
        )
    )


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--filas", type=int, default=25)
    parser.add_argument("--muestras", type=int, default=10)
    argumentos = parser.parse_args()
    base = os.environ.get("SCB_DATABASE_URL")
    if not base:
        raise SystemExit("Defina SCB_DATABASE_URL para una base PostgreSQL exclusivamente sintética")
    asyncio.run(medir(base, argumentos.filas, argumentos.muestras))
