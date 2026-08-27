from datetime import datetime, timedelta, timezone

import pytest

from aplicacion.modulos.identidad.esquemas import CredencialesUsuario, SesionPersistida
from aplicacion.modulos.identidad.seguridad import (
    comparar_secreto_sesion,
    hash_contrasena,
    requiere_rehash,
    verificar_contrasena,
)
from aplicacion.modulos.identidad.servicio import (
    AutenticacionBloqueada,
    AutenticacionFallida,
    ControlIntentosAutenticacion,
    PoliticaBloqueo,
    ServicioIdentidad,
    ServicioPermisos,
    preparar_hash_contrasena,
)


class UsuariosMemoria:
    def __init__(self, usuario: CredencialesUsuario | None) -> None:
        self.usuario = usuario

    def buscar_por_nombre(self, nombre_usuario: str) -> CredencialesUsuario | None:
        return (
            self.usuario if self.usuario and self.usuario.nombre_usuario == nombre_usuario else None
        )

    def buscar_por_id(self, id_usuario: int) -> CredencialesUsuario | None:
        return self.usuario if self.usuario and self.usuario.id_usuario == id_usuario else None


class SesionesMemoria:
    def __init__(self) -> None:
        self.sesiones: dict[str, SesionPersistida] = {}

    def guardar(self, sesion: SesionPersistida) -> None:
        self.sesiones[sesion.id_sesion] = sesion

    def buscar_vigente(self, id_sesion: str, ahora: datetime) -> SesionPersistida | None:
        sesion = self.sesiones.get(id_sesion)
        return sesion if sesion and sesion.expira_en > ahora and not sesion.revocada else None

    def revocar(self, id_sesion: str, ahora: datetime) -> None:
        del ahora
        if id_sesion in self.sesiones:
            self.sesiones[id_sesion] = self.sesiones[id_sesion].model_copy(
                update={"revocada": True}
            )

    def actualizar_csrf(self, id_sesion: str, csrf_hash: str) -> None:
        self.sesiones[id_sesion] = self.sesiones[id_sesion].model_copy(
            update={"csrf_hash": csrf_hash}
        )


def usuario_prueba(activo: bool = True) -> CredencialesUsuario:
    return CredencialesUsuario(
        idUsuario=7,
        nombreUsuario="operador",
        hashContrasena=hash_contrasena("Clave segura 2026"),
        activo=activo,
        permisos=frozenset({"rutas.administrar"}),
    )


def test_hash_solo_verifica_contrasena_correcta_y_es_argon2id() -> None:
    hash_generado = preparar_hash_contrasena("Clave segura 2026")
    assert hash_generado.startswith("$argon2id$")
    assert verificar_contrasena("Clave segura 2026", hash_generado)
    assert not verificar_contrasena("incorrecta", hash_generado)


def test_no_acepta_hash_legacy() -> None:
    assert not verificar_contrasena("clave", "LEGACY_SHA2_512:abc")


def test_autentica_emite_secreto_y_solo_persiste_su_digest() -> None:
    sesiones = SesionesMemoria()
    ahora = datetime(2026, 1, 1, tzinfo=timezone.utc)
    servicio = ServicioIdentidad(UsuariosMemoria(usuario_prueba()), sesiones, reloj=lambda: ahora)
    resultado = servicio.autenticar("operador", "Clave segura 2026")
    sesion = next(iter(sesiones.sesiones.values()))
    assert resultado.secreto_sesion != sesion.secreto_hash
    assert comparar_secreto_sesion(resultado.secreto_sesion, sesion.secreto_hash)
    assert servicio.validar_sesion(sesion.id_sesion, resultado.secreto_sesion).id_usuario == 7


def test_duracion_estudiantil_de_un_ano_y_expiracion_rechazada() -> None:
    sesiones = SesionesMemoria()
    ahora = datetime(2026, 1, 1, tzinfo=timezone.utc)
    servicio = ServicioIdentidad(
        UsuariosMemoria(usuario_prueba()), sesiones, timedelta(days=365), reloj=lambda: ahora
    )
    resultado = servicio.autenticar("operador", "Clave segura 2026")
    assert resultado.expira_en == ahora + timedelta(days=365)
    servicio_vencido = ServicioIdentidad(
        UsuariosMemoria(usuario_prueba()), sesiones, reloj=lambda: ahora + timedelta(days=366)
    )
    with pytest.raises(AutenticacionFallida):
        servicio_vencido.validar_sesion(resultado.id_sesion, resultado.secreto_sesion)


def test_rechaza_usuario_inactivo_clave_incorrecta_y_sesion_invalida() -> None:
    ahora = datetime(2026, 1, 1, tzinfo=timezone.utc)
    sesiones = SesionesMemoria()
    inactivo = ServicioIdentidad(
        UsuariosMemoria(usuario_prueba(False)), sesiones, reloj=lambda: ahora
    )
    with pytest.raises(AutenticacionFallida):
        inactivo.autenticar("operador", "Clave segura 2026")
    activo = ServicioIdentidad(UsuariosMemoria(usuario_prueba()), sesiones, reloj=lambda: ahora)
    with pytest.raises(AutenticacionFallida):
        activo.autenticar("operador", "otra")
    with pytest.raises(AutenticacionFallida):
        activo.validar_sesion("desconocida", "secreto")


def test_bloquea_autenticacion_despues_del_limite_y_limpia_un_exito() -> None:
    ahora = datetime(2026, 1, 1, tzinfo=timezone.utc)
    sesiones = SesionesMemoria()
    servicio = ServicioIdentidad(
        UsuariosMemoria(usuario_prueba()),
        sesiones,
        reloj=lambda: ahora,
        politica_bloqueo=PoliticaBloqueo(max_intentos=3, minutos_bloqueo=5),
        control_intentos=ControlIntentosAutenticacion(),
    )

    for _ in range(2):
        with pytest.raises(AutenticacionFallida):
            servicio.autenticar("operador", "incorrecta")
    servicio.autenticar("operador", "Clave segura 2026")

    for _ in range(3):
        with pytest.raises(AutenticacionFallida):
            servicio.autenticar("operador", "incorrecta")
    with pytest.raises(AutenticacionBloqueada):
        servicio.autenticar("operador", "Clave segura 2026")


def test_permisos_son_canonicos_y_extensibles() -> None:
    assert ServicioPermisos.tiene(frozenset({"rutas.administrar"}), "rutas.administrar")
    assert not ServicioPermisos.tiene(frozenset(), "rutas.administrar")


def test_hash_contrasena_vacio_es_invalido() -> None:
    with pytest.raises(ValueError):
        hash_contrasena("")


def test_requiere_rehash_rechaza_hash_invalido_y_acepta_hash_actual() -> None:
    assert requiere_rehash("no-es-un-hash")
    assert not requiere_rehash(hash_contrasena("Clave segura 2026"))
