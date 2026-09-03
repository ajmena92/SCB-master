"""Persistencia de identidad sin reglas de negocio."""

from datetime import datetime, timedelta, timezone
import hashlib

from fastapi import HTTPException
from sqlalchemy import delete, func, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    CredencialPortal,
    CuentaAdministrativa,
    CuentaPermiso,
    Persona,
    SesionAcceso,
    IntentoAutenticacion,
)


class RepositorioIdentidad:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def persona_por_cedula(self, cedula: str) -> Persona | None:
        return self.sesion.scalar(
            select(Persona).where(Persona.cedula == cedula, Persona.activo.is_(True))
        )

    def credencial(self, persona_id: int) -> CredencialPortal | None:
        return self.sesion.get(CredencialPortal, persona_id)

    def cuenta_por_usuario(self, usuario: str) -> CuentaAdministrativa | None:
        return self.sesion.scalar(
            select(CuentaAdministrativa).where(
                func.lower(CuentaAdministrativa.usuario) == usuario,
                CuentaAdministrativa.activo.is_(True),
            )
        )

    def permisos(self, cuenta_id: int) -> list[str]:
        return list(
            self.sesion.scalars(
                select(CuentaPermiso.permiso_clave).where(CuentaPermiso.cuenta_id == cuenta_id)
            ).all()
        )

    def sesion_acceso(self, hash_token: str) -> SesionAcceso | None:
        return self.sesion.get(SesionAcceso, hash_token)

    def persona(self, persona_id: int) -> Persona | None:
        return self.sesion.get(Persona, persona_id)

    def cuenta(self, cuenta_id: int) -> CuentaAdministrativa | None:
        return self.sesion.get(CuentaAdministrativa, cuenta_id)

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

    def revocar_sesiones_cuenta(self, cuenta_id: int) -> None:
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.cuenta_id == cuenta_id))

    @staticmethod
    def _hash_identificador(ambito: str, identificador: str) -> str:
        valor = f"{ambito}:{identificador.strip().lower()}"
        return hashlib.sha256(valor.encode("utf-8")).hexdigest()

    def verificar_bloqueo(self, ambito: str, identificador: str) -> None:
        registro = self.sesion.get(
            IntentoAutenticacion, self._hash_identificador(ambito, identificador)
        )
        if registro and registro.bloqueado_hasta and registro.bloqueado_hasta > datetime.now(timezone.utc):
            raise HTTPException(429, "Demasiados intentos. Intente nuevamente más tarde")

    def registrar_fallo(
        self, ambito: str, identificador: str, maximo: int, minutos_bloqueo: int
    ) -> None:
        clave = self._hash_identificador(ambito, identificador)
        registro = self.sesion.scalar(
            select(IntentoAutenticacion)
            .where(IntentoAutenticacion.identificador_hash == clave)
            .with_for_update()
        )
        ahora = datetime.now(timezone.utc)
        if registro is None:
            registro = IntentoAutenticacion(identificador_hash=clave)
            self.sesion.add(registro)
        registro.intentos_fallidos += 1
        registro.actualizado_en = ahora
        if registro.intentos_fallidos >= maximo:
            registro.bloqueado_hasta = ahora + timedelta(minutes=minutos_bloqueo)
        # El endpoint devuelve 401, por lo que la dependencia de sesión revierte
        # la unidad de trabajo normal. Este único registro debe sobrevivir al
        # rechazo para que el límite también funcione entre requests y workers.
        self.sesion.commit()

    def registrar_exito(self, ambito: str, identificador: str) -> None:
        self.sesion.execute(
            delete(IntentoAutenticacion).where(
                IntentoAutenticacion.identificador_hash
                == self._hash_identificador(ambito, identificador)
            )
        )
