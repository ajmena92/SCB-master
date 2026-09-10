"""Medición HTTPS/Nginx local sobre la base efímera 55439; no usa producción."""

import json
import os
import secrets
import ssl
import subprocess
import sys
import tempfile
import time
from pathlib import Path

import httpx
from cryptography.fernet import Fernet
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import CuentaAdministrativa, Persona
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.seguridad import hash_secreto
from ensayar_worker_fase4 import docker
from medir_cierre_fase4 import URL


def main():
    clave = Fernet.generate_key().decode()
    contrasena = secrets.token_urlsafe(24)
    usuario = "tls-" + secrets.token_hex(5)
    motor = crear_motor(URL)
    with Session(motor) as sesion:
        persona = Persona(cedula=usuario, nombres="HTTPS sintético", tipo="profesor")
        sesion.add(persona)
        sesion.flush()
        sesion.add(
            CuentaAdministrativa(
                persona_id=persona.id,
                usuario=usuario,
                contrasena_hash=hash_secreto(contrasena),
                rol="administrador",
                activo=True,
                vinculacion_pendiente=False,
            )
        )
        sesion.commit()
    entorno = {
        **os.environ,
        "DATABASE_URL": URL,
        "CORS_ORIGIN": "https://localhost:55441",
        "APP_ENV": "test",
        "COOKIE_SECURE": "true",
        "CSRF_SECRET": clave,
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
    with tempfile.TemporaryDirectory(prefix="scb-f4-tls-") as carpeta:
        cert, key = Path(carpeta) / "cert.pem", Path(carpeta) / "key.pem"
        subprocess.run(
            [
                "openssl",
                "req",
                "-x509",
                "-newkey",
                "rsa:2048",
                "-nodes",
                "-keyout",
                str(key),
                "-out",
                str(cert),
                "-days",
                "1",
                "-subj",
                "/CN=localhost",
                "-addext",
                "subjectAltName=DNS:localhost",
            ],
            check=True,
            capture_output=True,
        )
        api = subprocess.Popen(
            [
                sys.executable,
                "-m",
                "uvicorn",
                "aplicacion.entrada:crear_aplicacion",
                "--factory",
                "--host",
                "127.0.0.1",
                "--port",
                "55440",
                "--no-access-log",
            ],
            env=entorno,
            stdout=subprocess.DEVNULL,
        )
        nginx = False
        try:
            for _ in range(60):
                try:
                    if httpx.get("http://127.0.0.1:55440/api/v1/salud").status_code == 200:
                        break
                except httpx.TransportError:
                    time.sleep(0.5)
            config = Path(__file__).resolve().parents[1] / "ops/nginx/medicion_fase4.conf"
            docker(
                "run",
                "-d",
                "--name",
                "scb-f4-nginx-tls",
                "--network",
                "host",
                "--mount",
                f"type=bind,src={config},dst=/etc/nginx/nginx.conf,readonly",
                "--mount",
                f"type=bind,src={carpeta},dst=/certificados,readonly",
                "nginx:alpine",
            )
            nginx = True
            time.sleep(1)
            contexto = ssl.create_default_context(cafile=str(cert))
            with httpx.Client(
                base_url="https://localhost:55441/api/v1", verify=contexto
            ) as cliente:
                login = []
                comedor = []
                for _ in range(20):
                    assert cliente.get("/autenticacion/csrf").status_code == 204
                    inicio = time.perf_counter()
                    respuesta = cliente.post(
                        "/autenticacion/administracion",
                        json={"usuario": usuario, "contrasena": contrasena},
                        headers={
                            "Origin": "https://localhost:55441",
                            "X-CSRF-Token": cliente.cookies["csrf_token"],
                        },
                    )
                    assert respuesta.status_code == 200, respuesta.text
                    login.append((time.perf_counter() - inicio) * 1000)
                    inicio = time.perf_counter()
                    respuesta = cliente.get("/comedor/operacion/estado?fecha=2026-09-07")
                    assert respuesta.status_code == 200, respuesta.text
                    comedor.append((time.perf_counter() - inicio) * 1000)
                print(
                    json.dumps(
                        {
                            "transporte": "Nginx HTTPS, certificado verificado",
                            "muestras": 20,
                            "login_p95_ms": round(sorted(login)[18], 2),
                            "comedor_p95_ms": round(sorted(comedor)[18], 2),
                        },
                        indent=2,
                    )
                )
        finally:
            if nginx:
                docker("rm", "-f", "scb-f4-nginx-tls")
            api.terminate()
            api.wait(timeout=15)
    motor.dispose()


if __name__ == "__main__":
    main()
