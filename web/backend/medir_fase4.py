"""Medición reproducible, exclusivamente sobre PostgreSQL sintético vacío."""

import argparse
import asyncio
import io
import json
import math
import resource
import secrets
import time
from base64 import b64encode
from collections import Counter

import httpx
from openpyxl import Workbook
from sqlalchemy import event, inspect
from sqlalchemy.orm import Session

import aplicacion.modelos  # noqa: F401
from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.entrada import crear_aplicacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.modelos.maestros import CuentaAdministrativa, Persona
from aplicacion.nucleo.modelos_base import BaseDeclarativa
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.seguridad import hash_secreto
from config import Settings
from datos_medicion_fase4 import excel_casi_limite, preparar_operacion


async def ejecutar(url: str, filas: int, muestras: int):
    motor = crear_motor(url)
    if motor.url.host not in {"127.0.0.1", "localhost"} or motor.url.database != "scb_medicion":
        raise ValueError("Solo se admite la base local scb_medicion")
    if inspect(motor).get_table_names():
        raise ValueError("Se requiere una base vacía; no se borran datos existentes")
    BaseDeclarativa.metadata.create_all(motor)
    clave = secrets.token_urlsafe(32)
    with Session(motor) as sesion:
        persona = Persona(cedula="METRICA-ADMIN", nombres="Métrica sintética", tipo="profesor")
        sesion.add(persona)
        sesion.flush()
        sesion.add(
            CuentaAdministrativa(
                persona_id=persona.id,
                usuario="metrica",
                contrasena_hash=hash_secreto(clave),
                rol="administrador",
                activo=True,
                vinculacion_pendiente=False,
                cambio_contrasena_obligatorio=False,
            )
        )
        sesion.add_all(
            [
                Persona(
                    cedula=f"SINTETICA-{i}", nombres=f"Persona sintética {i}", tipo="estudiante"
                )
                for i in range(filas)
            ]
        )
        sesion.commit()
    datos = ImportacionEntrada.model_validate(
        {
            "anio": 2026,
            "filas": [
                {
                    "cedula": f"SINTETICA-{i}",
                    "nombres": f"Persona sintética {i}",
                    "tipo": "estudiante",
                    "seccion": "9-1",
                }
                for i in range(filas)
            ],
        }
    )
    consultas = 0

    @event.listens_for(motor, "before_cursor_execute")
    def contar(*args):
        nonlocal consultas
        consultas += 1

    resultados = []

    def registrar(nombre, tiempos, inicio_cpu, inicio_sql, estados=None):
        ordenados = sorted(tiempos)
        resultados.append(
            {
                "escenario": nombre,
                "muestras": len(tiempos),
                "max_ms": round(ordenados[-1], 2),
                "p95_ms": round(ordenados[math.ceil(len(tiempos) * 0.95) - 1], 2)
                if len(tiempos) > 1
                else None,
                "p99_ms": round(ordenados[math.ceil(len(tiempos) * 0.99) - 1], 2)
                if len(tiempos) > 1
                else None,
                "sql_total": consultas - inicio_sql,
                "cpu_s": round(time.process_time() - inicio_cpu, 3),
                "rss_max_mib": round(resource.getrusage(resource.RUSAGE_SELF).ru_maxrss / 1024, 2),
                "estados": dict(estados or {}),
            }
        )
        print(json.dumps(resultados[-1]), flush=True)

    with Session(motor) as sesion:
        servicio = ServicioImportacion(RepositorioImportacion(sesion))
        for nombre in ("previsualizar", "confirmar", "repetir"):
            tiempos, cpu, sql = [], time.process_time(), consultas
            for _ in range(muestras if nombre != "confirmar" else 1):
                inicio = time.perf_counter()
                if nombre == "previsualizar":
                    resumen = servicio.previsualizar(datos)
                else:
                    respuesta = servicio.confirmar(datos, resumen["huella"])
                    sesion.commit()
                    assert respuesta["repetida"] == (nombre == "repetir")
                tiempos.append((time.perf_counter() - inicio) * 1000)
            registrar(nombre, tiempos, cpu, sql)
        libro = Workbook(write_only=True)
        hoja = libro.create_sheet()
        hoja.append(["cedula", "nombres", "tipo", "seccion"])
        for fila in datos.filas:
            hoja.append([fila.cedula, fila.nombres, fila.tipo, fila.seccion])
        salida = io.BytesIO()
        libro.save(salida)
        contenido = salida.getvalue()
        tiempos, cpu, sql = [], time.process_time(), consultas
        for _ in range(3):
            inicio = time.perf_counter()
            assert servicio.desde_excel(contenido, 2026) == datos
            tiempos.append((time.perf_counter() - inicio) * 1000)
        registrar("xlsx", tiempos, cpu, sql)
        grande = excel_casi_limite(datos)
    preparar_operacion(motor)
    app = crear_aplicacion(
        motor=motor,
        configuracion=Settings(
            database_url=url,
            cookie_secure=False,
            csrf_secret=secrets.token_urlsafe(32),
            cors_origin="http://localhost:5173",
            carnet_qr_clave=b64encode(secrets.token_bytes(32)).decode(),
        ),
    )
    async with httpx.AsyncClient(
        transport=httpx.ASGITransport(app), base_url="http://pruebas"
    ) as cliente:
        await cliente.get("/api/v1/autenticacion/csrf")
        cabeceras = {
            "Origin": "http://localhost:5173",
            "X-CSRF-Token": cliente.cookies["csrf_token"],
        }
        login = await cliente.post(
            "/api/v1/autenticacion/administracion",
            json={"usuario": "metrica", "contrasena": clave},
            headers=cabeceras,
        )
        assert login.status_code == 200
        await cliente.get("/api/v1/autenticacion/csrf")
        cabeceras["X-CSRF-Token"] = cliente.cookies["csrf_token"]
        for ruta in (
            "/personas?pagina=1&tamano=25",
            "/personas/resumen",
            "/reportes/comedor?desde=2026-01-01&hasta=2026-12-31",
            "/reportes/transporte?desde=2026-01-01&hasta=2026-12-31",
            "/reportes/ventas?desde=2026-01-01&hasta=2026-12-31",
            "/comedor/operacion/estado?fecha=2026-09-05",
        ):
            tiempos, cpu, sql, estados = [], time.process_time(), consultas, Counter()
            limite = asyncio.Semaphore(4)

            async def solicitar():
                async with limite:
                    inicio = time.perf_counter()
                    respuesta = await cliente.get("/api/v1" + ruta)
                    tiempos.append((time.perf_counter() - inicio) * 1000)
                    estados[respuesta.status_code] += 1

            await asyncio.gather(*(solicitar() for _ in range(muestras)))
            registrar(ruta, tiempos, cpu, sql, estados)
            assert estados == {200: muestras}
        tiempos, cpu, sql, estados = [], time.process_time(), consultas, Counter()
        for _ in range(3):
            inicio = time.perf_counter()
            previa = await cliente.post(
                "/api/v1/importaciones/previsualizar",
                headers=cabeceras,
                data={"anio": "2026"},
                files={"archivo": ("padron.xlsx", grande)},
            )
            tiempos.append((time.perf_counter() - inicio) * 1000)
            estados[previa.status_code] += 1
        registrar("xlsx_cerca_limite", tiempos, cpu, sql, estados)
        assert estados == {200: 3}
        tiempos, cpu, sql, estados = [], time.process_time(), consultas, Counter()
        limite = asyncio.Semaphore(4)

        async def ingresar(indice):
            async with limite:
                inicio = time.perf_counter()
                respuesta = await cliente.post(
                    "/api/v1/comedor/operacion",
                    headers=cabeceras,
                    json={"cedula": f"SINTETICA-{indice}", "fecha": "2026-09-05"},
                )
                tiempos.append((time.perf_counter() - inicio) * 1000)
                estados[respuesta.status_code] += 1

        await asyncio.gather(*(ingresar(i) for i in range(muestras)))
        registrar("ingreso_concurrente", tiempos, cpu, sql, estados)
        assert estados == {201: muestras}
        exceso = await cliente.post(
            "/api/v1/importaciones/previsualizar",
            headers=cabeceras,
            data={"anio": "2026"},
            files={"archivo": ("padron.xlsx", b"x" * (12 * 1024 * 1024 + 1))},
        )
        assert exceso.status_code == 413
    motor.dispose()
    print(
        json.dumps(
            {
                "filas": filas,
                "xlsx_bytes": len(contenido),
                "xlsx_grande_bytes": len(grande),
                "concurrencia": 4,
                "exceso_http": exceso.status_code,
                "resultados": resultados,
            },
            indent=2,
        )
    )


if __name__ == "__main__":
    argumentos = argparse.ArgumentParser(description=__doc__)
    argumentos.add_argument("--puerto", type=int, required=True)
    argumentos.add_argument("--filas", type=int, default=1000)
    argumentos.add_argument("--muestras", type=int, default=30)
    opciones = argumentos.parse_args()
    asyncio.run(
        ejecutar(
            f"postgresql+psycopg://postgres@127.0.0.1:{opciones.puerto}/scb_medicion",
            opciones.filas,
            opciones.muestras,
        )
    )
