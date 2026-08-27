import inspect
from typing import Any, cast

from fastapi.routing import APIRoute as RutaAPI

from aplicacion.modulos.estudiantes.operaciones import (
    GeneracionPinesSeccion,
    crear_enrutador_operaciones,
)
from aplicacion.modulos.estudiantes.repositorio import RepositorioSqlEstudiantes
from aplicacion.nucleo.identidad.servicio import ServicioIdentidad


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
    enrutador = crear_enrutador_operaciones(
        lambda: iter((repo,)),
        lambda permiso: lambda: None,
        lambda: None,
        obtener_identidad=dependencia_identidad_nula,
        obtener_identidad_estudiante=dependencia_identidad_nula,
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
    enrutador = crear_enrutador_operaciones(
        lambda: iter((repo,)),
        lambda permiso: lambda: None,
        lambda: None,
        obtener_identidad=dependencia_identidad_nula,
        obtener_identidad_estudiante=dependencia_identidad_nula,
    )
    rutas = getattr(enrutador, "routes")
    ruta = next(r for r in rutas if isinstance(r, RutaAPI) and r.path.endswith("/pines/seccion"))
    salida = ruta.endpoint(GeneracionPinesSeccion(seccion=""), None, None, repo)
    assert salida["seccion"] == "Sin sección"
    assert len(repo.hashes) == 1 and 3 in repo.hashes


def test_reinicio_individual_devuelve_pin_nuevo() -> None:
    repo = RepositorioFalso()
    enrutador = crear_enrutador_operaciones(
        lambda: iter((repo,)),
        lambda permiso: lambda: None,
        lambda: None,
        obtener_identidad=dependencia_identidad_nula,
        obtener_identidad_estudiante=dependencia_identidad_nula,
    )
    rutas = getattr(enrutador, "routes")
    ruta = next(r for r in rutas if isinstance(r, RutaAPI) and r.path.endswith("/reset-pin"))
    salida = ruta.endpoint(4, None, None, repo)
    assert salida.id_estudiante == 4 and len(salida.pin) == 6
    assert repo.hashes[4].startswith("$argon2id$")


def test_persistencia_pin_aplica_vencimiento_y_lo_limpia_al_cambiar() -> None:
    fuente = inspect.getsource(RepositorioSqlEstudiantes)
    assert "fecha_expiracion_pin=DATEADD(day, 1" in fuente
    assert "fecha_expiracion_pin IS NULL OR fecha_expiracion_pin > SYSUTCDATETIME()" in fuente
    assert "fecha_expiracion_pin=NULL" in fuente


def test_rutas_literales_preceden_a_parametros_dinamicos() -> None:
    enrutador = crear_enrutador_operaciones(
        lambda: iter(()),
        lambda permiso: lambda: None,
        lambda: None,
        obtener_identidad=dependencia_identidad_nula,
        obtener_identidad_estudiante=dependencia_identidad_nula,
    )
    rutas = [r.path for r in getattr(enrutador, "routes") if isinstance(r, RutaAPI)]
    assert rutas.index("/estudiantes/secciones") < rutas.index(
        "/estudiantes/{id_estudiante}/perfil"
    )


def test_carnet_expone_contrato_canonico_y_descargas_sin_rutas_historicas() -> None:
    fuente = inspect.getsource(crear_enrutador_operaciones)
    assert '"idEstudiante"' in fuente
    assert '"primerApellido"' in fuente
    assert '"rutaDescripcion"' in fuente
    assert '"/carnet.pdf"' in fuente
    assert '"/carnet/foto"' in fuente
    assert "/api/student" not in fuente
