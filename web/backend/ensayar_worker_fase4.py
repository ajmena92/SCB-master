"""Reinicio real del worker Docker sobre la base efímera del medidor de cierre.

Ejecutar después de medir_cierre_fase4.py. No admite destinos externos.
"""

import json
import secrets
import subprocess
import time

from cryptography.fernet import Fernet
from sqlalchemy import func, select
from sqlalchemy.orm import Session

from aplicacion.casos_importacion import ServicioImportacion
from aplicacion.esquemas import ImportacionEntrada
from aplicacion.modelos.maestros import CuentaAdministrativa, Persona
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.repositorios_importacion import RepositorioImportacion
from medir_cierre_fase4 import URL


def docker(*args):
    return subprocess.run(
        ["docker", *args], check=True, capture_output=True, text=True
    ).stdout.strip()


def main():
    motor = crear_motor(URL)
    clave = Fernet.generate_key().decode()
    entorno = {
        "DATABASE_URL": URL,
        "CORS_ORIGIN": "http://127.0.0.1:8081",
        "APP_ENV": "development",
        "COOKIE_SECURE": "false",
        "CSRF_SECRET": secrets.token_urlsafe(32),
        "CARNET_QR_CLAVE": clave,
        "IMPORTACION_RESULTADOS_KEY": clave,
        "STUDENT_MAX_LOGIN_ATTEMPTS": "8",
        "STUDENT_LOCK_MINUTES": "5",
        "ADMIN_MAX_LOGIN_ATTEMPTS": "5",
        "ADMIN_LOCK_MINUTES": "15",
        "STUDENT_SESSION_DAYS": "365",
        "ADMIN_SESSION_MINUTES": "60",
        "CSRF_ANONYMOUS_TTL_SECONDS": "600",
    }
    nombres = []

    def iniciar(nombre):
        argumentos = [
            "run",
            "-d",
            "--name",
            nombre,
            "--network",
            "host",
            "--memory",
            "256m",
            "--read-only",
            "--tmpfs",
            "/tmp",
            "--cap-drop",
            "ALL",
            "--security-opt",
            "no-new-privileges:true",
            "--user",
            "10001",
            "--entrypoint",
            "python",
        ]
        for campo, valor in entorno.items():
            argumentos.extend(["-e", f"{campo}={valor}"])
        argumentos.extend(["scb-f4-worker-cierre", "-m", "aplicacion.worker_importacion"])
        docker(*argumentos)
        nombres.append(nombre)

    try:
        with Session(motor) as sesion:
            cuenta = sesion.scalar(
                select(CuentaAdministrativa).where(CuentaAdministrativa.usuario == "cierre-f4")
            )
            assert cuenta is not None
            actor = cuenta.id
            datos = ImportacionEntrada(
                anio=2026,
                filas=[
                    {
                        "cedula": f"CIERRE-{i:05d}",
                        "nombres": f"Estudiante sintético {i}",
                        "tipo": "estudiante",
                        "seccion": "7-1",
                    }
                    for i in range(2600)
                ],
            )
            servicio = ServicioImportacion(RepositorioImportacion(sesion))
            identificador = servicio.encolar(datos, servicio.previsualizar(datos)["huella"], actor)[
                "trabajoId"
            ]
            sesion.commit()
        iniciar("scb-f4-worker-reinicio")
        for _ in range(60):
            with Session(motor) as sesion:
                estado = RepositorioImportacion(sesion).trabajo(identificador).estado
            if estado == "ejecutando":
                break
            time.sleep(0.2)
        assert estado == "ejecutando"
        docker("kill", "scb-f4-worker-reinicio")
        with Session(motor) as sesion:
            assert (
                sesion.scalar(
                    select(func.count()).select_from(Persona).where(Persona.tipo == "estudiante")
                )
                == 2500
            ), "La caída debe revertir las altas parciales"
        inicio = time.monotonic()
        docker("start", "scb-f4-worker-reinicio")
        while time.monotonic() - inicio < 120:
            with Session(motor) as sesion:
                estado = RepositorioImportacion(sesion).trabajo(identificador).estado
            if estado == "completado":
                break
            time.sleep(0.5)
        assert estado == "completado", estado
        iniciar("scb-f4-worker-duplicado")
        codigo = docker("wait", "scb-f4-worker-duplicado")
        assert codigo == "1", codigo
        with Session(motor) as sesion:
            assert (
                sesion.scalar(
                    select(func.count()).select_from(Persona).where(Persona.tipo == "estudiante")
                )
                == 2600
            )
            servicio = ServicioImportacion(RepositorioImportacion(sesion))
            resultado = servicio.entregar_resultado(identificador, actor, clave)
            assert len(resultado["credenciales"]) == 100
            sesion.commit()
        print(
            json.dumps(
                {
                    "reinicio_real": True,
                    "rollback_parcial": True,
                    "recuperacion_segundos": round(time.monotonic() - inicio, 2),
                    "filas_finales": 2600,
                    "segunda_replica_rechazada": True,
                },
                indent=2,
            )
        )
    finally:
        for nombre in nombres:
            docker("rm", "-f", nombre)
        motor.dispose()


if __name__ == "__main__":
    main()
