from datetime import datetime, timezone
from typing import Any, cast

import pytest

from aplicacion.modulos.identidad.esquemas import SesionPersistida
from aplicacion.modulos.identidad.servicio import ServicioSesiones


class IdentidadFalsa:
    def __init__(self, id_usuario: int, tipo: str) -> None:
        self.sesion = SesionPersistida(
            idSesion=f"{tipo}-sesion",
            idUsuario=id_usuario,
            secretoHash="digest",
            expiraEn=datetime.now(timezone.utc),
            csrfHash=f"{tipo}-csrf",
        )
        self.revocada = False

    def validar_sesion(self, id_sesion: str, secreto: str) -> SesionPersistida:
        from aplicacion.modulos.identidad.servicio import AutenticacionFallida

        if id_sesion != self.sesion.id_sesion or secreto != "secreto" or self.revocada:
            raise AutenticacionFallida("La sesión no es válida")
        return self.sesion

    def permisos_de_sesion(self, _sesion: SesionPersistida) -> frozenset[str]:
        return frozenset({"administracion.usuarios.editar"})

    def validar_csrf(self, _sesion: SesionPersistida, token: str) -> bool:
        return token == self.sesion.csrf_hash

    def cerrar_sesion(self, _id_sesion: str) -> None:
        self.revocada = True


def crear_servicio() -> tuple[ServicioSesiones, IdentidadFalsa, IdentidadFalsa]:
    admin = IdentidadFalsa(10, "admin")
    estudiante = IdentidadFalsa(20, "estudiante")
    servicio = ServicioSesiones(
        cast(Any, admin),
        cast(Any, estudiante),
        lambda id_estudiante: {"idEstudiante": id_estudiante, "tieneFoto": True},
    )
    return servicio, admin, estudiante


def test_validar_resuelve_sesion_administrativa() -> None:
    servicio, _, _ = crear_servicio()

    resultado = servicio.validar("admin-sesion", "secreto")

    assert resultado.tipo == "admin"
    assert resultado.usuario["roles"] == ["Administrador"]


def test_validar_resuelve_sesion_estudiantil_y_su_perfil() -> None:
    servicio, _, _ = crear_servicio()

    resultado = servicio.validar("estudiante-sesion", "secreto")

    assert resultado.tipo == "estudiante"
    assert resultado.usuario == {"idEstudiante": 20, "tieneFoto": True}


@pytest.mark.parametrize("tipo", ["administrativa", "estudiantil"])
def test_cerrar_exige_csrf_y_revoca_ambos_tipos(tipo: str) -> None:
    servicio, admin, estudiante = crear_servicio()
    identidad = admin if tipo == "administrativa" else estudiante
    id_sesion = identidad.sesion.id_sesion

    with pytest.raises(ValueError, match="CSRF"):
        servicio.cerrar(id_sesion, "secreto", "incorrecto", identidad.sesion.csrf_hash or "")
    assert not identidad.revocada

    servicio.cerrar(
        id_sesion, "secreto", identidad.sesion.csrf_hash or "", identidad.sesion.csrf_hash or ""
    )

    assert identidad.revocada


def test_validar_falla_si_no_hay_identidad_estudiantil() -> None:
    admin = IdentidadFalsa(10, "admin")
    servicio = ServicioSesiones(cast(Any, admin))

    with pytest.raises(ValueError, match="sesión"):
        servicio.validar("estudiante-sesion", "secreto")
