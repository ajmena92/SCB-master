"""Casos de uso de autenticacion y autorizacion."""

import secrets
from datetime import datetime, timezone

from fastapi import HTTPException

from aplicacion.esquemas import (
    AdministracionEntrada,
    CambioContrasenaAdministrativaEntrada,
    PortalEntrada,
    SesionSalida,
)
from aplicacion.repositorios_identidad import RepositorioIdentidad
from aplicacion.seguridad import hash_secreto, nueva_sesion, token_hash, verificar_secreto


class ServicioIdentidad:
    def __init__(
        self,
        repositorio: RepositorioIdentidad,
        *,
        student_max_login_attempts: int = 8,
        student_lock_minutes: int = 5,
        admin_max_login_attempts: int = 5,
        admin_lock_minutes: int = 15,
        student_session_days: int,
        admin_session_minutes: int,
    ):
        self.repo = repositorio
        self.student_max_login_attempts = student_max_login_attempts
        self.student_lock_minutes = student_lock_minutes
        self.admin_max_login_attempts = admin_max_login_attempts
        self.admin_lock_minutes = admin_lock_minutes
        self.student_session_days = student_session_days
        self.admin_session_minutes = admin_session_minutes

    def autenticar_portal(self, datos: PortalEntrada) -> tuple[str, SesionSalida]:
        identificador = datos.cedula.strip()
        self.repo.verificar_bloqueo("portal", identificador)
        persona = self.repo.persona_por_cedula(datos.cedula.strip())
        if persona is None:
            self.repo.registrar_fallo(
                "portal", identificador, self.student_max_login_attempts, self.student_lock_minutes
            )
            raise HTTPException(401, "Cedula o PIN incorrecto")
        credencial = self.repo.credencial(persona.id)
        if credencial is None or not verificar_secreto(credencial.pin_hash, datos.pin):
            self.repo.registrar_fallo(
                "portal", identificador, self.student_max_login_attempts, self.student_lock_minutes
            )
            raise HTTPException(401, "Cedula o PIN incorrecto")
        self.repo.registrar_exito("portal", identificador)
        token, acceso = nueva_sesion(
            tipo="portal",
            persona_id=persona.id,
            cambio_obligatorio=credencial.cambio_obligatorio,
            student_session_days=self.student_session_days,
            admin_session_minutes=self.admin_session_minutes,
        )
        self.repo.guardar_sesion(acceso)
        return token, SesionSalida(
            tipo="portal",
            persona_id=persona.id,
            cambio_obligatorio=credencial.cambio_obligatorio,
            expira_en=acceso.expira_en,
        )

    def autenticar_administracion(self, datos: AdministracionEntrada) -> tuple[str, SesionSalida]:
        identificador = datos.usuario.strip().lower()
        self.repo.verificar_bloqueo("administracion", identificador)
        cuenta = self.repo.cuenta_por_usuario(identificador)
        if cuenta is None or not verificar_secreto(cuenta.contrasena_hash, datos.contrasena):
            self.repo.registrar_fallo(
                "administracion",
                identificador,
                self.admin_max_login_attempts,
                self.admin_lock_minutes,
            )
            raise HTTPException(401, "Usuario o contrasena incorrectos")
        self.repo.registrar_exito("administracion", identificador)
        if cuenta.rol == "operador" and cuenta.vinculacion_pendiente:
            raise HTTPException(403, "La cuenta requiere vinculacion por un administrador")
        persona = self.repo.persona(cuenta.persona_id) if cuenta.persona_id else None
        if cuenta.persona_id is not None and (
            persona is None or not persona.activo or persona.tipo != "profesor"
        ):
            raise HTTPException(401, "Profesor inactivo o invalido")
        if not cuenta.vinculacion_pendiente and persona is None:
            raise HTTPException(401, "Cuenta sin profesor")
        token, acceso = nueva_sesion(
            tipo="administracion",
            cuenta_id=cuenta.id,
            student_session_days=self.student_session_days,
            admin_session_minutes=self.admin_session_minutes,
        )
        self.repo.guardar_sesion(acceso)
        return token, SesionSalida(
            tipo="administracion",
            rol=cuenta.rol,
            persona_id=cuenta.persona_id,
            cuenta_id=cuenta.id,
            usuario=cuenta.usuario,
            permisos=[] if cuenta.rol == "administrador" else self.repo.permisos(cuenta.id),
            vinculacion_pendiente=cuenta.vinculacion_pendiente,
            cambio_contrasena_obligatorio=cuenta.cambio_contrasena_obligatorio,
            expira_en=acceso.expira_en,
        )

    def identidad_por_token(self, token: str) -> dict:
        acceso = self.repo.sesion_acceso(token_hash(token))
        if acceso is None or acceso.expira_en.replace(tzinfo=timezone.utc) <= datetime.now(
            timezone.utc
        ):
            raise HTTPException(401, "Sesion invalida o vencida")
        if acceso.tipo == "portal":
            if acceso.persona_id is None:
                raise HTTPException(401, "Sesion invalida")
            persona = self.repo.persona(acceso.persona_id)
            if persona is None or not persona.activo:
                raise HTTPException(401, "Persona inactiva")
            return {
                "tipo": "portal",
                "persona": persona,
                "cambioObligatorio": acceso.cambio_obligatorio,
                "_token": token,
            }
        if acceso.cuenta_id is None:
            raise HTTPException(401, "Sesion invalida")
        cuenta = self.repo.cuenta(acceso.cuenta_id)
        if cuenta is None or not cuenta.activo:
            raise HTTPException(401, "Cuenta inactiva")
        persona = self.repo.persona(cuenta.persona_id) if cuenta.persona_id else None
        if persona is not None and (not persona.activo or persona.tipo != "profesor"):
            raise HTTPException(401, "Profesor inactivo o invalido")
        if not cuenta.vinculacion_pendiente and persona is None:
            raise HTTPException(401, "Cuenta sin profesor")
        return {
            "tipo": "administracion",
            "cuenta": cuenta,
            "persona": persona,
            "rol": cuenta.rol,
            "permisos": [] if cuenta.rol == "administrador" else self.repo.permisos(cuenta.id),
            "_token": token,
        }

    def cambiar_pin(self, identidad: dict, datos) -> dict:
        if identidad["tipo"] != "portal":
            raise HTTPException(403, "Se requiere sesion de portal")
        credencial = self.repo.credencial(identidad["persona"].id)
        if credencial is None or not verificar_secreto(credencial.pin_hash, datos.pin_actual):
            raise HTTPException(401, "PIN actual incorrecto")
        if datos.pin_actual == datos.pin_nuevo:
            raise HTTPException(422, "El PIN nuevo debe ser diferente")
        self.repo.cambiar_pin(credencial, hash_secreto(datos.pin_nuevo))
        return {"cambioObligatorio": False, "sesionesRevocadas": True}

    def cambiar_contrasena_administrativa(
        self, identidad: dict, datos: CambioContrasenaAdministrativaEntrada
    ) -> dict:
        if identidad["tipo"] != "administracion":
            raise HTTPException(403, "Se requiere una cuenta administrativa")
        cuenta = identidad["cuenta"]
        if not verificar_secreto(cuenta.contrasena_hash, datos.contrasena_actual):
            raise HTTPException(401, "La contrasena actual es incorrecta")
        if datos.contrasena_actual == datos.contrasena_nueva:
            raise HTTPException(422, "La contrasena nueva debe ser diferente")
        cuenta.contrasena_hash = hash_secreto(datos.contrasena_nueva)
        cuenta.cambio_contrasena_obligatorio = False
        self.repo.revocar_sesiones_cuenta(cuenta.id)
        return {"cambioContrasenaObligatorio": False, "sesionesRevocadas": True}

    def cerrar_sesion(self, token: str) -> None:
        self.repo.revocar_sesion(token_hash(token))

    def renovar_sesion(self, token: str) -> tuple[str, datetime]:
        """Rota el identificador sin ampliar el vencimiento absoluto vigente."""
        acceso = self.repo.sesion_acceso(token_hash(token))
        if acceso is None or acceso.expira_en.replace(tzinfo=timezone.utc) <= datetime.now(
            timezone.utc
        ):
            raise HTTPException(401, "Sesion invalida o vencida")
        nuevo_token = secrets.token_urlsafe(32)
        acceso_nuevo = type(acceso)(
            token_hash=token_hash(nuevo_token),
            tipo=acceso.tipo,
            persona_id=acceso.persona_id,
            cuenta_id=acceso.cuenta_id,
            cambio_obligatorio=acceso.cambio_obligatorio,
            expira_en=acceso.expira_en,
        )
        self.repo.revocar_sesion(token_hash(token))
        self.repo.guardar_sesion(acceso_nuevo)
        return nuevo_token, acceso.expira_en
