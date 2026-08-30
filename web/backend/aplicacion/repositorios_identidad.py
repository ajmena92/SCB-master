"""Persistencia de identidad sin reglas de negocio."""

from sqlalchemy import delete, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    CredencialPortal,
    CuentaAdministrativa,
    Persona,
    SesionAcceso,
)


class RepositorioIdentidad:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def persona_por_codigo(self, codigo: str) -> Persona | None:
        return self.sesion.scalar(
            select(Persona).where(Persona.codigo == codigo, Persona.activo.is_(True))
        )

    def credencial(self, persona_id: int) -> CredencialPortal | None:
        return self.sesion.get(CredencialPortal, persona_id)

    def cuenta_por_usuario(self, usuario: str) -> CuentaAdministrativa | None:
        return self.sesion.scalar(
            select(CuentaAdministrativa).where(
                CuentaAdministrativa.usuario == usuario, CuentaAdministrativa.activo.is_(True)
            )
        )

    def sesion_acceso(self, hash_token: str) -> SesionAcceso | None:
        return self.sesion.get(SesionAcceso, hash_token)

    def persona(self, persona_id: int) -> Persona | None:
        return self.sesion.get(Persona, persona_id)

    def cuenta(self, cuenta_id: int) -> CuentaAdministrativa | None:
        return self.sesion.get(CuentaAdministrativa, cuenta_id)

    def codigo_existe(self, codigo: str) -> bool:
        return self.sesion.scalar(select(Persona.id).where(Persona.codigo == codigo)) is not None

    def guardar_sesion(self, acceso: SesionAcceso) -> None:
        self.sesion.add(acceso)
        self.sesion.flush()

    def cambiar_pin(self, credencial: CredencialPortal, pin_hash: str) -> None:
        credencial.pin_hash = pin_hash
        credencial.cambio_obligatorio = False
        self.sesion.execute(
            delete(SesionAcceso).where(
                SesionAcceso.persona_id == credencial.persona_id, SesionAcceso.tipo == "portal"
            )
        )

    def revocar_sesion(self, hash_token: str) -> None:
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.token_hash == hash_token))
