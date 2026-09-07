"""Altas con Argon2 real y carga ASGI sostenida en base sintética local."""

import argparse
import asyncio
import base64
import json
import math
import resource
import secrets
import time
from collections import Counter

import httpx
from sqlalchemy import delete, event, select
from sqlalchemy.orm import Session

import aplicacion.casos_importacion as casos_importacion
from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.entrada import crear_aplicacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.modelos.maestros import CredencialPortal, CuentaAdministrativa, Persona
from aplicacion.modelos.operacion import CuentaTiquete, LoteImportacion
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.seguridad import hash_secreto
from config import Settings


def resumen(tiempos):
    valores = sorted(tiempos)
    return {
        "n": len(valores),
        "p95_ms": round(valores[math.ceil(len(valores) * 0.95) - 1], 2),
        "p99_ms": round(valores[math.ceil(len(valores) * 0.99) - 1], 2),
    }


class MedicionSql:
    """Acumula la latencia de SQL de una fase, sin registrar sentencias ni datos."""

    def __init__(self, motor):
        self.activa = False
        self.milisegundos = 0.0
        self.consultas = 0

        @event.listens_for(motor, "before_cursor_execute")
        def antes(_conexion, _cursor, _sentencia, _parametros, contexto, _muchos):
            if self.activa:
                contexto._inicio_medicion_fase4 = time.perf_counter()

        @event.listens_for(motor, "after_cursor_execute")
        def despues(_conexion, _cursor, _sentencia, _parametros, contexto, _muchos):
            inicio = getattr(contexto, "_inicio_medicion_fase4", None)
            if self.activa and inicio is not None:
                self.milisegundos += (time.perf_counter() - inicio) * 1000
                self.consultas += 1

    def iniciar(self):
        self.milisegundos, self.consultas, self.activa = 0.0, 0, True

    def detener(self):
        self.activa = False
        return round(self.milisegundos, 2), self.consultas


def limpiar_lote_sintetico(motor, cedulas, huella):
    """Retira únicamente filas creadas por este medidor tras un commit real."""
    with Session(motor) as sesion:
        ids = select(Persona.id).where(Persona.cedula.in_(cedulas))
        sesion.execute(delete(CredencialPortal).where(CredencialPortal.persona_id.in_(ids)))
        sesion.execute(delete(CuentaTiquete).where(CuentaTiquete.persona_id.in_(ids)))
        sesion.execute(delete(Persona).where(Persona.cedula.in_(cedulas)))
        sesion.execute(delete(LoteImportacion).where(LoteImportacion.huella == huella))
        sesion.commit()


async def medir(puerto, filas, repeticiones, segundos):
    url = f"postgresql+psycopg://postgres@127.0.0.1:{int(puerto)}/scb_medicion"
    motor = crear_motor(url)
    sql = MedicionSql(motor)
    tiempos = {"previsualizacion": [], "confirmacion": [], "argon2": [], "sql": [], "commit": []}
    cpu = time.process_time()
    for indice in range(repeticiones):
        datos = ImportacionEntrada.model_validate(
            {
                "anio": 2026,
                "filas": [
                    {
                        "cedula": f"ALTA-{secrets.token_hex(8)}",
                        "nombres": "Alta sintética",
                        "tipo": "profesor",
                    }
                    for _ in range(filas)
                ],
            }
        )
        cedulas = [fila.cedula for fila in datos.filas]
        with Session(motor) as sesion:
            servicio = ServicioImportacion(RepositorioImportacion(sesion))
            sql.iniciar()
            inicio = time.perf_counter()
            previa = servicio.previsualizar(datos)
            previsualizacion_ms = (time.perf_counter() - inicio) * 1000
            previsualizacion_sql_ms, previsualizacion_consultas = sql.detener()
            argon2_ms = 0.0
            original_hash = casos_importacion.hash_secreto

            def medir_hash(secreto):
                nonlocal argon2_ms
                inicio_hash = time.perf_counter()
                resultado_hash = original_hash(secreto)
                argon2_ms += (time.perf_counter() - inicio_hash) * 1000
                return resultado_hash

            casos_importacion.hash_secreto = medir_hash
            sql.iniciar()
            inicio = time.perf_counter()
            try:
                resultado = servicio.confirmar(datos, previa["huella"])
                assert len(resultado["credenciales"]) == filas
                assert not resultado["repetida"]
                confirmacion_ms = (time.perf_counter() - inicio) * 1000
                confirmacion_sql_ms, confirmacion_consultas = sql.detener()
            finally:
                casos_importacion.hash_secreto = original_hash
            sql.iniciar()
            inicio = time.perf_counter()
            sesion.commit()
            commit_ms = (time.perf_counter() - inicio) * 1000
            commit_sql_ms, commit_consultas = sql.detener()
        limpiar_lote_sintetico(motor, cedulas, previa["huella"])
        tiempos["previsualizacion"].append(previsualizacion_ms)
        tiempos["confirmacion"].append(confirmacion_ms)
        tiempos["argon2"].append(argon2_ms)
        tiempos["sql"].append(confirmacion_sql_ms)
        tiempos["commit"].append(commit_ms)
        print(
            json.dumps(
                {
                    "alta_lote": indice + 1,
                    "previsualizacion_ms": round(previsualizacion_ms, 2),
                    "previsualizacion_sql_ms": previsualizacion_sql_ms,
                    "previsualizacion_consultas": previsualizacion_consultas,
                    "confirmacion_ms": round(confirmacion_ms, 2),
                    "argon2_ms": round(argon2_ms, 2),
                    "confirmacion_sql_ms": confirmacion_sql_ms,
                    "confirmacion_consultas": confirmacion_consultas,
                    "commit_ms": round(commit_ms, 2),
                    "commit_sql_ms": commit_sql_ms,
                    "commit_consultas": commit_consultas,
                }
            ),
            flush=True,
        )
    print(
        json.dumps(
            {
                "altas_nuevas": {fase: resumen(valores) for fase, valores in tiempos.items()},
                "filas_por_lote": filas,
                "cpu_s": round(time.process_time() - cpu, 3),
            }
        ),
        flush=True,
    )
    usuario, clave = "carga-" + secrets.token_hex(5), secrets.token_urlsafe(32)
    with Session(motor) as sesion:
        persona = Persona(cedula=usuario, nombres="Carga sintética", tipo="profesor")
        sesion.add(persona)
        sesion.flush()
        sesion.add(
            CuentaAdministrativa(
                persona_id=persona.id,
                usuario=usuario,
                contrasena_hash=hash_secreto(clave),
                rol="administrador",
                activo=True,
                vinculacion_pendiente=False,
                cambio_contrasena_obligatorio=False,
            )
        )
        sesion.commit()
    app = crear_aplicacion(
        motor=motor,
        configuracion=Settings(
            database_url=url,
            cookie_secure=False,
            cors_origin="http://localhost:5173",
            csrf_secret=secrets.token_urlsafe(32),
            carnet_qr_clave=base64.b64encode(secrets.token_bytes(32)).decode(),
        ),
    )
    async with httpx.AsyncClient(
        transport=httpx.ASGITransport(app), base_url="http://pruebas"
    ) as cliente:
        await cliente.get("/api/v1/autenticacion/csrf")
        respuesta = await cliente.post(
            "/api/v1/autenticacion/administracion",
            json={"usuario": usuario, "contrasena": clave},
            headers={
                "Origin": "http://localhost:5173",
                "X-CSRF-Token": cliente.cookies["csrf_token"],
            },
        )
        assert respuesta.status_code == 200
        rutas = [
            "/personas?tamano=25",
            "/personas/resumen",
            "/reportes/comedor?desde=2026-01-01&hasta=2026-12-31",
            "/comedor/operacion/estado?fecha=2026-09-05",
        ]
        muestras = {ruta: [] for ruta in rutas}
        estados = Counter()
        inicio, cpu = time.perf_counter(), time.process_time()

        async def trabajador():
            indice = 0
            while time.perf_counter() - inicio < segundos:
                ruta = rutas[indice % len(rutas)]
                marca = time.perf_counter()
                respuesta = await cliente.get("/api/v1" + ruta)
                muestras[ruta].append((time.perf_counter() - marca) * 1000)
                estados[respuesta.status_code] += 1
                indice += 1

        await asyncio.gather(*(trabajador() for _ in range(4)))
        print(
            json.dumps(
                {
                    "duracion_s": round(time.perf_counter() - inicio, 2),
                    "cpu_s": round(time.process_time() - cpu, 3),
                    "concurrencia": 4,
                    "rss_max_mib": round(
                        resource.getrusage(resource.RUSAGE_SELF).ru_maxrss / 1024, 2
                    ),
                    "estados": dict(estados),
                    "rutas": {ruta: resumen(v) for ruta, v in muestras.items()},
                },
                indent=2,
            )
        )
        assert set(estados) == {200}
    motor.dispose()


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--puerto", type=int, required=True)
    parser.add_argument("--filas", type=int, default=25)
    parser.add_argument("--repeticiones", type=int, default=10)
    parser.add_argument("--segundos", type=int, default=60)
    args = parser.parse_args()
    asyncio.run(medir(args.puerto, args.filas, args.repeticiones, args.segundos))
