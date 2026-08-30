import asyncio
import inspect
from datetime import datetime, timezone
from typing import Any, cast

import httpx
import pytest
from fastapi import Depends, FastAPI, HTTPException, Request
from fastapi.routing import APIRoute as RutaAPI

from aplicacion.modulos.estudiantes.administracion import crear_enrutador_administracion
from aplicacion.modulos.estudiantes.esquemas import GeneracionPinesSeccion
from aplicacion.modulos.estudiantes.operaciones import crear_enrutador_operaciones
from aplicacion.modulos.estudiantes.portal import crear_enrutador_portal
from aplicacion.modulos.estudiantes.repositorio_credenciales import RepositorioSqlCredenciales
from aplicacion.modulos.estudiantes.repositorio_pines import RepositorioSqlPines
from aplicacion.modulos.identidad.esquemas import SesionPersistida
from aplicacion.modulos.identidad.servicio import ServicioIdentidad
from aplicacion.seguridad_dependencias import crear_dependencias_seguridad


def dependencia_identidad_nula() -> ServicioIdentidad:
    return cast(ServicioIdentidad, None)


class RepositorioFalso:
    def __init__(self) -> None:
        self.hashes: dict[int, str] = {}

    def listar_para_generacion_pines(self) -> list[dict[str, Any]]:
        return [
            {"id_estudiante": 1, "seccion": "10-1", "turno": "diurno"},
            {"id_estudiante": 2, "seccion": "10-1", "turno": "nocturno"},
            {"id_estudiante": 3, "seccion": None, "turno": "diurno"},
        ]

    def actualizar_pines_seccion(self, seccion: str | None, hashes: dict[int, str]) -> None:
        self.hashes = hashes

    def reiniciar_pin(self, id_estudiante: int, hash_pin: str) -> None:
        self.hashes[id_estudiante] = hash_pin


def test_generacion_de_pines_por_seccion_filtra_turno_y_devuelve_reporte() -> None:
    repo = RepositorioFalso()
    enrutador = crear_enrutador_administracion(
        lambda: iter((repo,)), lambda permiso: lambda: None, lambda: None
    )
    rutas = getattr(enrutador, "routes")
    ruta = next(r for r in rutas if isinstance(r, RutaAPI) and r.path.endswith("/pines/seccion"))
    salida = ruta.endpoint(GeneracionPinesSeccion(seccion="10-1", turno="diurno"), None, None, repo)
    assert salida["total"] == 1
    assert salida["turno"] == "diurno"
    assert salida["estudiantes"][0]["idEstudiante"] == 1
    assert salida["estudiantes"][0]["pin"].isdigit()
    assert repo.hashes[1].startswith("$argon2id$")


def test_generacion_de_pines_sin_seccion_persiste_con_none() -> None:
    repo = RepositorioFalso()
    enrutador = crear_enrutador_administracion(
        lambda: iter((repo,)), lambda permiso: lambda: None, lambda: None
    )
    rutas = getattr(enrutador, "routes")
    ruta = next(r for r in rutas if isinstance(r, RutaAPI) and r.path.endswith("/pines/seccion"))
    salida = ruta.endpoint(GeneracionPinesSeccion(seccion=""), None, None, repo)
    assert salida["seccion"] == "Sin sección"
    assert len(repo.hashes) == 1 and 3 in repo.hashes


def test_reinicio_individual_devuelve_pin_nuevo() -> None:
    repo = RepositorioFalso()
    enrutador = crear_enrutador_administracion(
        lambda: iter((repo,)), lambda permiso: lambda: None, lambda: None
    )
    rutas = getattr(enrutador, "routes")
    ruta = next(r for r in rutas if isinstance(r, RutaAPI) and r.path.endswith("/reset-pin"))
    salida = ruta.endpoint(4, None, None, repo)
    assert salida.id_estudiante == 4 and len(salida.pin) == 6
    assert repo.hashes[4].startswith("$argon2id$")


def test_persistencia_pin_aplica_vencimiento_y_lo_limpia_al_cambiar() -> None:
    fuente = inspect.getsource(RepositorioSqlPines)
    fuente_credenciales = inspect.getsource(RepositorioSqlCredenciales)
    assert "fecha_expiracion_pin=DATEADD(day, 1" in fuente
    assert (
        "fecha_expiracion_pin IS NULL OR fecha_expiracion_pin > SYSUTCDATETIME()"
        in fuente_credenciales
    )
    assert "fecha_expiracion_pin=NULL" in fuente_credenciales


def test_rutas_literales_preceden_a_parametros_dinamicos() -> None:
    enrutador = crear_enrutador_administracion(
        lambda: iter(()), lambda permiso: lambda: None, lambda: None
    )
    rutas = [r.path for r in getattr(enrutador, "routes") if isinstance(r, RutaAPI)]
    assert rutas.index("/secciones") < rutas.index("/{id_estudiante}/perfil")


def test_ensamblador_expone_rutas_de_portal_y_administracion() -> None:
    aplicacion = FastAPI()
    aplicacion.include_router(
        crear_enrutador_operaciones(
            lambda: iter(()),
            lambda permiso: lambda: None,
            lambda: None,
            obtener_identidad=dependencia_identidad_nula,
            obtener_identidad_estudiante=dependencia_identidad_nula,
        )
    )
    rutas = set(aplicacion.openapi()["paths"])
    assert {
        "/estudiantes/menu",
        "/estudiantes/carnet",
        "/estudiantes/secciones",
        "/estudiantes/pines/seccion",
    } <= rutas


def test_asistencia_estudiantil_exige_csrf() -> None:
    def exigir_csrf():
        return None

    enrutador = crear_enrutador_portal(
        lambda: iter(()),
        obtener_identidad=dependencia_identidad_nula,
        obtener_identidad_estudiante=dependencia_identidad_nula,
        obtener_asistencia=lambda: iter(()),
        exigir_csrf=exigir_csrf,
    )
    ruta = next(
        r
        for r in enrutador.routes
        if (
            isinstance(r, RutaAPI)
            and r.path == "/asistencia/{accion}"
            and r.methods
            and "POST" in r.methods
        )
    )
    assert any(dependencia.call == exigir_csrf for dependencia in ruta.dependant.dependencies)


class IdentidadHttpFalsa:
    def __init__(self) -> None:
        self.sesion = SesionPersistida(
            idSesion="sesion-estudiante",
            idUsuario=7,
            secretoHash="hash",
            expiraEn=datetime(2030, 1, 1, tzinfo=timezone.utc),
            csrfHash="hash-csrf",
        )

    def validar_sesion(self, id_sesion: str, secreto: str) -> SesionPersistida:
        if id_sesion != "sesion-estudiante" or secreto != "secreto":
            raise ValueError("sesión inválida")
        return self.sesion

    def validar_csrf(self, sesion: SesionPersistida, token: str) -> bool:
        return token == "token-valido"


class RepositorioPortalHttpFalso:
    def buscar_credencial_por_id(self, id_usuario: int) -> dict[str, object]:
        return {"id_estudiante": id_usuario, "carne": "2026-001", "nombre": "Ana", "activo": True}


class AsistenciaHttpFalsa:
    def __init__(self) -> None:
        self.registros: list[dict[str, object]] = []

    def registrar(
        self, datos: dict[str, object], id_estudiante: int, origen: str
    ) -> dict[str, object]:
        self.registros.append(datos)
        return {"estado": datos["estado"], "idEstudiante": id_estudiante, "origen": origen}


class ComedorPortalHttpFalso:
    def __init__(self, reservada: bool) -> None:
        self.reservada = reservada
        self.cancelaciones = 0

    def persona_por_estudiante(self, id_estudiante: int) -> int:
        assert id_estudiante == 7
        return 1

    def reserva_por_persona_fecha(self, id_persona: int, fecha) -> dict | None:
        if not self.reservada:
            return None
        return {
            "id_reserva": 1,
            "id_persona": id_persona,
            "fecha": fecha,
            "estado": "reservada",
            "requiere_tiquete": False,
            "modalidad": "beca",
        }

    def reservar(self, id_persona: int, fecha, usuario) -> dict:
        if not self.reservada:
            raise ValueError("No hay tiquetes disponibles para reservar el comedor")
        return {
            "id_reserva": 1,
            "id_persona": id_persona,
            "fecha": fecha,
            "estado": "reservada",
            "requiere_tiquete": False,
            "modalidad": "beca",
        }

    def cancelar(self, id_persona: int, fecha, usuario) -> dict:
        assert id_persona == 1
        self.cancelaciones += 1
        self.reservada = False
        return {
            "id_reserva": 1,
            "id_persona": id_persona,
            "fecha": fecha,
            "estado": "cancelada",
            "requiere_tiquete": False,
            "modalidad": "beca",
        }


def test_confirmar_asistencia_sin_reserva_es_rechazado() -> None:
    comedor = ComedorPortalHttpFalso(reservada=False)
    identidad = IdentidadHttpFalsa()
    asistencia = AsistenciaHttpFalsa()
    router = crear_enrutador_portal(
        obtener_repositorio=lambda: RepositorioPortalHttpFalso(),
        obtener_identidad_estudiante=lambda: identidad,
        obtener_asistencia=lambda: asistencia,
        obtener_comedor=lambda: comedor,
        exigir_csrf=lambda: None,
        obtener_fecha_local=lambda: datetime(2026, 8, 28).date(),
    )
    ruta = next(
        r for r in router.routes if isinstance(r, RutaAPI) and r.path == "/asistencia/{accion}"
    )

    with pytest.raises(HTTPException) as error:
        ruta.endpoint(
            "confirm",
            RepositorioPortalHttpFalso(),
            identidad,
            "sesion-estudiante",
            "secreto",
            asistencia,
            comedor,
            None,
        )

    assert error.value.status_code == 409
    assert asistencia.registros == []


def test_cancelar_asistencia_libera_reserva_y_registra_ausencia() -> None:
    comedor = ComedorPortalHttpFalso(reservada=True)
    identidad = IdentidadHttpFalsa()
    asistencia = AsistenciaHttpFalsa()
    router = crear_enrutador_portal(
        obtener_repositorio=lambda: RepositorioPortalHttpFalso(),
        obtener_identidad_estudiante=lambda: identidad,
        obtener_asistencia=lambda: asistencia,
        obtener_comedor=lambda: comedor,
        exigir_csrf=lambda: None,
        obtener_fecha_local=lambda: datetime(2026, 8, 28).date(),
    )
    ruta = next(
        r for r in router.routes if isinstance(r, RutaAPI) and r.path == "/asistencia/{accion}"
    )
    respuesta = ruta.endpoint(
        "decline",
        RepositorioPortalHttpFalso(),
        identidad,
        "sesion-estudiante",
        "secreto",
        asistencia,
        comedor,
        None,
    )

    assert respuesta["estado"] == "ausente"
    assert comedor.cancelaciones == 1


def _cliente_portal_http() -> httpx.AsyncClient:
    identidad = IdentidadHttpFalsa()
    dependencias = crear_dependencias_seguridad(lambda: cast(ServicioIdentidad, identidad))
    aplicacion = FastAPI()

    async def csrf_http(request: Request) -> dict[str, object]:
        token = request.headers.get("X-CSRF-Token")
        cookie = request.cookies.get("csrf_token")
        return dependencias["exigir_csrf"]((identidad, identidad.sesion), token, cookie)

    @aplicacion.post("/asistencia/confirm")
    async def asistencia_confirmar(_: dict[str, object] = Depends(csrf_http)):
        return {"estado": "presente"}

    transporte = httpx.ASGITransport(app=aplicacion)
    return httpx.AsyncClient(transport=transporte, base_url="http://pruebas")


def test_asistencia_http_sin_csrf_devuelve_403() -> None:
    async def ejecutar() -> httpx.Response:
        async with _cliente_portal_http() as cliente:
            cliente.cookies.set("id_sesion", "sesion-estudiante")
            cliente.cookies.set("secreto_sesion", "secreto")
            return await cliente.post("/asistencia/confirm")

    assert asyncio.run(ejecutar()).status_code == 403


def test_asistencia_http_con_csrf_invalido_devuelve_403() -> None:
    async def ejecutar() -> httpx.Response:
        async with _cliente_portal_http() as cliente:
            cliente.cookies.update(
                {
                    "id_sesion": "sesion-estudiante",
                    "secreto_sesion": "secreto",
                    "csrf_token": "incorrecto",
                }
            )
            return await cliente.post("/asistencia/confirm", headers={"X-CSRF-Token": "incorrecto"})

    assert asyncio.run(ejecutar()).status_code == 403


def test_asistencia_http_con_csrf_valido_permite_operacion() -> None:
    async def ejecutar() -> httpx.Response:
        async with _cliente_portal_http() as cliente:
            cliente.cookies.update(
                {
                    "id_sesion": "sesion-estudiante",
                    "secreto_sesion": "secreto",
                    "csrf_token": "token-valido",
                }
            )
            return await cliente.post(
                "/asistencia/confirm", headers={"X-CSRF-Token": "token-valido"}
            )

    respuesta = asyncio.run(ejecutar())
    assert respuesta.status_code == 200
    assert respuesta.json()["estado"] == "presente"


def test_carnet_expone_contrato_en_linea_sin_descargas() -> None:
    fuente = inspect.getsource(crear_enrutador_portal)
    assert '"idEstudiante"' in fuente
    assert '"primerApellido"' in fuente
    assert '"rutaDescripcion"' in fuente
    assert '"/carnet/foto"' in fuente
    assert "/api/student" not in fuente
    rutas = {
        ruta.path
        for ruta in crear_enrutador_portal(
            lambda: iter(()),
            obtener_identidad=dependencia_identidad_nula,
            obtener_identidad_estudiante=dependencia_identidad_nula,
            obtener_asistencia=lambda: iter(()),
            obtener_comedor=lambda: iter(()),
        ).routes
    }
    assert not any(ruta.endswith(".pdf") for ruta in rutas)
