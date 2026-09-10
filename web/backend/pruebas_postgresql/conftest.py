import asyncio
from datetime import date
from decimal import Decimal

import httpx
import pytest
from sqlalchemy.orm import Session

import aplicacion.modelos  # noqa: F401
from aplicacion.entrada import crear_aplicacion
from aplicacion.modelos.maestros import (
    AnioLectivo,
    CredencialPortal,
    CuentaAdministrativa,
    CuentaPermiso,
    Matricula,
    PermisoAdministrativo,
    Persona,
)
from aplicacion.modelos.operacion import CuentaTiquete, Tarifa
from aplicacion.nucleo.modelos_base import BaseDeclarativa
from aplicacion.nucleo.postgresql import crear_motor
from aplicacion.permisos import PERMISOS_ADMINISTRATIVOS
from aplicacion.seguridad import hash_secreto
from config import Settings


class ClienteASGI:
    def __init__(self, app):
        self.app = app
        self.cookies: dict[str, str] = {}

    def request(self, metodo, ruta, **opciones):
        async def ejecutar():
            transporte = httpx.ASGITransport(app=self.app)
            async with httpx.AsyncClient(
                transport=transporte, base_url="http://pruebas"
            ) as cliente:
                cabeceras = dict(opciones.pop("headers", {}))
                if self.cookies:
                    cabeceras.setdefault(
                        "Cookie", "; ".join(f"{k}={v}" for k, v in self.cookies.items())
                    )
                respuesta = await cliente.request(metodo, ruta, headers=cabeceras, **opciones)
                self.cookies.update(respuesta.cookies)
                return respuesta

        return asyncio.run(ejecutar())

    def get(self, ruta, **opciones):
        return self.request("GET", ruta, **opciones)

    def post(self, ruta, **opciones):
        return self.request("POST", ruta, **opciones)

    def put(self, ruta, **opciones):
        return self.request("PUT", ruta, **opciones)

    def delete(self, ruta, **opciones):
        return self.request("DELETE", ruta, **opciones)

    def patch(self, ruta, **opciones):
        return self.request("PATCH", ruta, **opciones)

    def csrf(self) -> dict[str, str]:
        respuesta = self.get("/api/v1/autenticacion/csrf")
        return {
            "Origin": "http://localhost:5173",
            "X-CSRF-Token": respuesta.cookies.get("csrf_token") or self.cookies["csrf_token"],
        }

    def cabecera_autenticada(self) -> dict[str, str]:
        """Expone solo las cookies de esta identidad aislada para una solicitud."""
        cabecera = self.csrf()
        cabecera["Cookie"] = "; ".join(
            f"{nombre}={valor}" for nombre, valor in self.cookies.items()
        )
        return cabecera


def autenticar_administracion(app, usuario: str, contrasena: str) -> ClienteASGI:
    cliente = ClienteASGI(app)
    respuesta = cliente.post(
        "/api/v1/autenticacion/administracion",
        json={"usuario": usuario, "contrasena": contrasena},
        headers=cliente.csrf(),
    )
    assert respuesta.status_code == 200, respuesta.text
    return cliente


def autenticar_portal(app, cedula: str, pin: str = "123456") -> ClienteASGI:
    cliente = ClienteASGI(app)
    respuesta = cliente.post(
        "/api/v1/autenticacion/portal",
        json={"cedula": cedula, "pin": pin},
        headers=cliente.csrf(),
    )
    assert respuesta.status_code == 200, respuesta.text
    return cliente


@pytest.fixture
def entorno(request):
    vigencia_portal, vigencia_administracion = getattr(request, "param", (365, 60))
    motor = crear_motor("sqlite://")
    BaseDeclarativa.metadata.create_all(motor)
    with Session(motor) as sesion:
        admin_persona = Persona(
            cedula="900000001",
            nombres="Administrador Pruebas",
            tipo="profesor",
            activo=True,
        )
        operador_persona = Persona(
            cedula="900000002",
            nombres="Operador Pruebas",
            tipo="profesor",
            activo=True,
        )
        sesion.add_all([admin_persona, operador_persona])
        sesion.flush()
        admin_cuenta = CuentaAdministrativa(
            persona_id=admin_persona.id,
            usuario="admin",
            contrasena_hash=hash_secreto("Clave-segura-2026"),
            rol="administrador",
            activo=True,
            vinculacion_pendiente=False,
        )
        operador_cuenta = CuentaAdministrativa(
            persona_id=operador_persona.id,
            usuario="operador",
            contrasena_hash=hash_secreto("Clave-operador-2026"),
            rol="operador",
            activo=True,
            vinculacion_pendiente=False,
        )
        sesion.add_all(
            [
                admin_cuenta,
                operador_cuenta,
                Tarifa(
                    tipo_persona="estudiante", monto=Decimal("700"), fecha_inicio=date(2026, 1, 1)
                ),
                Tarifa(
                    tipo_persona="profesor", monto=Decimal("1000"), fecha_inicio=date(2026, 1, 1)
                ),
            ]
        )
        sesion.flush()
        for clave, nombre, descripcion, modulo in PERMISOS_ADMINISTRATIVOS:
            sesion.add(
                PermisoAdministrativo(
                    clave=clave,
                    nombre=nombre,
                    descripcion=descripcion,
                    modulo=modulo,
                )
            )
            if clave in {
                "comedor.operar",
                "transporte.operar",
                "tiquetes.operar",
                "reportes.leer",
            }:
                sesion.add(CuentaPermiso(cuenta_id=operador_cuenta.id, permiso_clave=clave))
        sesion.commit()
    app = crear_aplicacion(
        motor=motor,
        configuracion=Settings(
            database_url="postgresql+psycopg://no-usada",
            cors_origin="http://localhost:5173",
            cookie_secure=False,
            csrf_secret="csrf-pruebas",
            carnet_qr_clave="MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY=",
            importacion_resultados_key="eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHg=",
            student_session_days=vigencia_portal,
            admin_session_minutes=vigencia_administracion,
        ),
    )
    cliente = ClienteASGI(app)
    cliente.motor = motor
    admin = autenticar_administracion(app, "admin", "Clave-segura-2026")
    operador = autenticar_administracion(app, "operador", "Clave-operador-2026")
    yield (
        cliente,
        motor,
        {
            "admin": admin.cabecera_autenticada(),
            "operador": operador.cabecera_autenticada(),
            "admin_cliente": admin,
            "operador_cliente": operador,
        },
    )


def crear_persona(
    cliente,
    cabecera,
    *,
    tipo="estudiante",
    cedula="1",
    nombres="Ana Perez",
    cambio_pin_obligatorio=False,
):
    # El padrón es la única fuente de personas; la preparación de pruebas lo
    # representa directamente en persistencia, nunca por el endpoint retirado.
    with Session(cliente.motor) as sesion:
        persona = Persona(cedula=cedula, nombres=nombres, tipo=tipo, activo=True)
        sesion.add(persona)
        sesion.flush()
        sesion.add_all(
            [
                CredencialPortal(
                    persona_id=persona.id,
                    pin_hash=hash_secreto("123456"),
                    cambio_obligatorio=cambio_pin_obligatorio,
                ),
                CuentaTiquete(persona_id=persona.id, saldo=0, reservados=0),
            ]
        )
        sesion.commit()
        return {
            "id": persona.id,
            "codigo": persona.cedula,
            "cedula": persona.cedula,
            "nombres": persona.nombres,
            "tipo": persona.tipo,
            "activo": persona.activo,
            "pinTemporal": "123456",
        }


def preparar_estudiante(cliente, cabecera, cedula="1"):
    persona = crear_persona(cliente, cabecera, cedula=cedula)
    with Session(cliente.motor) as sesion:
        anio = AnioLectivo(anio=2026, vigente=True)
        sesion.add(anio)
        sesion.flush()
        matricula = Matricula(
            persona_id=persona["id"],
            anio_lectivo_id=anio.id,
            seccion="7-1",
            turno="diurno",
            becado=False,
            estado="activo",
        )
        sesion.add(matricula)
        sesion.commit()
        return (
            persona,
            {"id": anio.id, "anio": anio.anio, "vigente": anio.vigente},
            {
                "id": matricula.id,
                "persona_id": matricula.persona_id,
                "anio_lectivo_id": matricula.anio_lectivo_id,
                "seccion": matricula.seccion,
                "turno": matricula.turno,
                "becado": matricula.becado,
                "estado": matricula.estado,
            },
        )
