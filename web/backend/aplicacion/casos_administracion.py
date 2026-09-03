"""Reglas de mantenimiento de cuentas administrativas."""

from __future__ import annotations

import secrets
import string

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.esquemas import (
    CambioContrasenaAdministrativaEntrada,
    CuentaAdministrativaActualizacion,
    CuentaAdministrativaEntrada,
    ProfesorNuevoAdministrativo,
    VinculacionCuentaEntrada,
)
from aplicacion.modelos.maestros import CredencialPortal, CuentaAdministrativa, Persona
from aplicacion.modelos.operacion import CuentaTiquete
from aplicacion.permisos import CLAVES_PERMISOS
from aplicacion.repositorios_administracion import RepositorioAdministracion
from aplicacion.seguridad import hash_secreto, verificar_secreto


def _secreto_temporal(longitud: int = 18) -> str:
    alfabeto = string.ascii_letters + string.digits + "-_.!"
    while True:
        valor = "".join(secrets.choice(alfabeto) for _ in range(longitud))
        if (
            any(c.islower() for c in valor)
            and any(c.isupper() for c in valor)
            and any(c.isdigit() for c in valor)
        ):
            return valor


def _pin_temporal() -> str:
    return f"{secrets.randbelow(1_000_000):06d}"


class ServicioAdministracion:
    def __init__(self, repositorio: RepositorioAdministracion):
        self.repo = repositorio

    @staticmethod
    def _normalizar_usuario(usuario: str) -> str:
        return usuario.strip().lower()

    @staticmethod
    def _validar_permisos(rol: str, permisos: list[str]) -> list[str]:
        desconocidos = sorted(set(permisos) - CLAVES_PERMISOS)
        if desconocidos:
            raise HTTPException(422, f"Permisos desconocidos: {', '.join(desconocidos)}")
        return [] if rol == "administrador" else sorted(set(permisos))

    def _salida(self, cuenta: CuentaAdministrativa, persona: Persona | None = None) -> dict:
        persona = persona or (self.repo.persona(cuenta.persona_id) if cuenta.persona_id else None)
        return {
            "id": cuenta.id,
            "persona": (
                {
                    "id": persona.id,
                    "cedula": persona.cedula,
                    "nombres": persona.nombres,
                }
                if persona
                else None
            ),
            "usuario": cuenta.usuario,
            "rol": cuenta.rol,
            "activo": cuenta.activo,
            "permisos": self.repo.permisos_cuenta(cuenta.id),
            "cambioContrasenaObligatorio": cuenta.cambio_contrasena_obligatorio,
            "vinculacionPendiente": cuenta.vinculacion_pendiente,
        }

    def listar(self) -> list[dict]:
        return [self._salida(cuenta, persona) for cuenta, persona in self.repo.listar_cuentas()]

    def listar_permisos(self) -> list[dict]:
        return [
            {
                "clave": permiso.clave,
                "nombre": permiso.nombre,
                "descripcion": permiso.descripcion,
                "modulo": permiso.modulo,
            }
            for permiso in self.repo.listar_permisos()
        ]

    def profesores_disponibles(self) -> list[dict]:
        return [
            {"id": p.id, "cedula": p.cedula, "nombres": p.nombres}
            for p in self.repo.profesores_disponibles()
        ]

    def _profesor_existente(self, persona_id: int) -> Persona:
        persona = self.repo.persona(persona_id)
        if persona is None or persona.tipo != "profesor" or not persona.activo:
            raise HTTPException(422, "La persona debe ser un profesor activo")
        if self.repo.cuenta_por_persona(persona.id):
            raise HTTPException(409, "El profesor ya tiene una cuenta administrativa")
        return persona

    def _crear_profesor(self, datos: ProfesorNuevoAdministrativo) -> tuple[Persona, str]:
        cedula = datos.cedula.strip()
        if self.repo.persona_por_cedula(cedula):
            raise HTTPException(409, "La cedula ya esta registrada")
        pin = _pin_temporal()
        persona = Persona(
            cedula=cedula,
            nombres=datos.nombres.strip(),
            tipo="profesor",
            activo=True,
        )
        self.repo.crear_profesor(
            persona,
            CredencialPortal(persona_id=0, pin_hash=hash_secreto(pin), cambio_obligatorio=True),
            CuentaTiquete(persona_id=0, saldo=0, reservados=0),
        )
        return persona, pin

    def _resolver_profesor(self, persona_id, profesor_nuevo) -> tuple[Persona, str | None]:
        if persona_id is not None:
            return self._profesor_existente(persona_id), None
        if profesor_nuevo is None:  # protegido tambien por Pydantic
            raise HTTPException(422, "Debe indicar un profesor")
        return self._crear_profesor(profesor_nuevo)

    def crear(self, datos: CuentaAdministrativaEntrada) -> dict:
        usuario = self._normalizar_usuario(datos.usuario)
        if self.repo.cuenta_por_usuario(usuario):
            raise HTTPException(409, "El usuario ya existe")
        try:
            with self.repo.sesion.begin_nested():
                persona, pin = self._resolver_profesor(datos.persona_id, datos.profesor_nuevo)
                permisos = self._validar_permisos(datos.rol, datos.permisos)
                contrasena = _secreto_temporal()
                cuenta = CuentaAdministrativa(
                    persona_id=persona.id,
                    usuario=usuario,
                    contrasena_hash=hash_secreto(contrasena),
                    rol=datos.rol,
                    activo=True,
                    cambio_contrasena_obligatorio=True,
                    vinculacion_pendiente=False,
                )
                self.repo.guardar(cuenta)
                self.repo.asignar_permisos(cuenta.id, permisos)
        except IntegrityError as exc:
            raise HTTPException(409, "El usuario o profesor ya esta asignado") from exc
        credenciales = {"contrasena": contrasena}
        if pin is not None:
            credenciales["pin"] = pin
        return {"cuenta": self._salida(cuenta, persona), "credencialesTemporales": credenciales}

    def actualizar(
        self, cuenta_id: int, datos: CuentaAdministrativaActualizacion, actor_id: int
    ) -> dict:
        self.repo.bloquear_administradores()
        cuenta = self.repo.cuenta(cuenta_id, bloquear=True)
        if cuenta is None:
            raise HTTPException(404, "Cuenta no encontrada")
        nuevo_rol = datos.rol or cuenta.rol
        nuevo_activo = cuenta.activo if datos.activo is None else datos.activo
        if cuenta.id == actor_id and (not nuevo_activo or nuevo_rol != "administrador"):
            raise HTTPException(409, "No puede desactivar ni degradar su propia cuenta")
        deja_de_ser_admin = (
            cuenta.rol == "administrador"
            and cuenta.activo
            and (nuevo_rol != "administrador" or not nuevo_activo)
        )
        if deja_de_ser_admin and self.repo.contar_administradores_activos() <= 1:
            raise HTTPException(409, "Debe existir al menos un administrador activo")
        if cuenta.rol == "administrador" and nuevo_rol == "operador" and datos.permisos is None:
            raise HTTPException(422, "Debe indicar los permisos al cambiar a operador")
        if datos.persona_id is not None and datos.persona_id != cuenta.persona_id:
            # La cuenta conserva su usuario, rol y permisos; solo se reasigna a
            # otro profesor activo que aún no tenga una cuenta administrativa.
            # Las sesiones previas se revocan al final de la actualización.
            cuenta.persona_id = self._profesor_existente(datos.persona_id).id
            cuenta.vinculacion_pendiente = False
        if datos.usuario is not None:
            usuario = self._normalizar_usuario(datos.usuario)
            existente = self.repo.cuenta_por_usuario(usuario)
            if existente and existente.id != cuenta.id:
                raise HTTPException(409, "El usuario ya existe")
            cuenta.usuario = usuario
        cuenta.rol = nuevo_rol
        cuenta.activo = nuevo_activo
        permisos = self._validar_permisos(
            nuevo_rol,
            datos.permisos if datos.permisos is not None else self.repo.permisos_cuenta(cuenta.id),
        )
        self.repo.asignar_permisos(cuenta.id, permisos)
        self.repo.revocar_sesiones(cuenta.id)
        self.repo.guardar(cuenta)
        return self._salida(cuenta)

    def restablecer(self, cuenta_id: int, actor_id: int) -> dict:
        cuenta = self.repo.cuenta(cuenta_id, bloquear=True)
        if cuenta is None:
            raise HTTPException(404, "Cuenta no encontrada")
        contrasena = _secreto_temporal()
        cuenta.contrasena_hash = hash_secreto(contrasena)
        cuenta.cambio_contrasena_obligatorio = True
        self.repo.revocar_sesiones(cuenta.id)
        self.repo.guardar(cuenta)
        return {
            "contrasenaTemporal": contrasena,
            "cambioContrasenaObligatorio": True,
            "sesionesRevocadas": True,
        }

    def vincular_inicial(self, identidad: dict, datos: VinculacionCuentaEntrada) -> dict:
        if identidad.get("tipo") != "administracion":
            raise HTTPException(403, "Se requiere una cuenta administrativa")
        cuenta = self.repo.cuenta(identidad["cuenta"].id, bloquear=True)
        if cuenta is None or cuenta.rol != "administrador":
            raise HTTPException(403, "Solo el administrador legado puede vincularse")
        if not cuenta.vinculacion_pendiente or cuenta.persona_id is not None:
            raise HTTPException(409, "La cuenta ya fue vinculada")
        try:
            with self.repo.sesion.begin_nested():
                persona, pin = self._resolver_profesor(datos.persona_id, datos.profesor_nuevo)
                cuenta.persona_id = persona.id
                cuenta.vinculacion_pendiente = False
                self.repo.guardar(cuenta)
        except IntegrityError as exc:
            raise HTTPException(409, "El profesor ya esta registrado o asignado") from exc
        salida: dict[str, object] = {"cuenta": self._salida(cuenta, persona)}
        if pin is not None:
            salida["pinTemporal"] = pin
        return salida

    def cambiar_contrasena(
        self, identidad: dict, datos: CambioContrasenaAdministrativaEntrada
    ) -> dict:
        cuenta = self.repo.cuenta(identidad["cuenta"].id, bloquear=True)
        if cuenta is None or not verificar_secreto(cuenta.contrasena_hash, datos.contrasena_actual):
            raise HTTPException(401, "La contrasena actual es incorrecta")
        if datos.contrasena_actual == datos.contrasena_nueva:
            raise HTTPException(422, "La contrasena nueva debe ser diferente")
        cuenta.contrasena_hash = hash_secreto(datos.contrasena_nueva)
        cuenta.cambio_contrasena_obligatorio = False
        self.repo.revocar_sesiones(cuenta.id)
        self.repo.guardar(cuenta)
        return {"cambioContrasenaObligatorio": False, "sesionesRevocadas": True}
