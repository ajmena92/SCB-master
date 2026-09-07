"""Ejecutor único de trabajos de importación."""

import time
from datetime import datetime, timedelta, timezone

from sqlalchemy import text

from aplicacion.nucleo.postgresql import crear_fabrica_sesiones, crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from aplicacion.trabajos_importacion import procesar_un_trabajo
from config import Settings


def ejecutar() -> None:
    configuracion = Settings.from_environment()
    motor = crear_motor(configuracion.database_url)
    fabrica = crear_fabrica_sesiones(motor)
    # Bloqueo por conexión: PostgreSQL lo libera al morir el proceso.
    # Impide que dos réplicas apliquen padrones completos simultáneamente.
    with motor.connect() as conexion:
        if not conexion.scalar(text("SELECT pg_try_advisory_lock(7340219)")):
            raise RuntimeError("Ya existe un trabajador de importación activo")
        with fabrica() as sesion:
            RepositorioImportacion(sesion).recuperar_trabajos_interrumpidos(
                datetime.now(timezone.utc)
            )
            sesion.commit()
        consumir(fabrica, configuracion.importacion_resultados_key)


def consumir(fabrica, clave_resultados: str) -> None:
    while True:
        with fabrica() as sesion:
            RepositorioImportacion(sesion).recuperar_trabajos_interrumpidos(
                datetime.now(timezone.utc) - timedelta(minutes=30)
            )
            sesion.commit()
        try:
            trabajo_id = procesar_un_trabajo(fabrica, clave_resultados)
        except Exception:
            # El trabajo ya quedó fallido y con rollback; no publicar payloads
            # ni credenciales en logs ni bloquear el resto de la cola.
            print("Trabajo de importación fallido; consulte su estado", flush=True)
            time.sleep(1)
            continue
        if trabajo_id is None:
            time.sleep(1)


if __name__ == "__main__":
    ejecutar()
