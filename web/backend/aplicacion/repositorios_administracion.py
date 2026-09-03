"""Persistencia PostgreSQL de cuentas y permisos administrativos."""

from __future__ import annotations

from sqlalchemy import delete, func, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    CredencialPortal,
    CuentaAdministrativa,
    CuentaPermiso,
    PermisoAdministrativo,
    Persona,
    SesionAcceso,
)
from aplicacion.modelos.operacion import CuentaTiquete


class RepositorioAdministracion:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def listar_cuentas(self):
        return self.sesion.execute(
            select(CuentaAdministrativa, Persona)
            .outerjoin(Persona, Persona.id == CuentaAdministrativa.persona_id)
            .order_by(CuentaAdministrativa.usuario)
        ).all()

    def cuenta(self, cuenta_id: int, *, bloquear: bool = False) -> CuentaAdministrativa | None:
        consulta = select(CuentaAdministrativa).where(CuentaAdministrativa.id == cuenta_id)
        if bloquear and self.sesion.bind is not None and self.sesion.bind.dialect.name != "sqlite":
            consulta = consulta.with_for_update()
        return self.sesion.scalar(consulta)

    def cuenta_por_usuario(self, usuario: str) -> CuentaAdministrativa | None:
        return self.sesion.scalar(
            select(CuentaAdministrativa).where(func.lower(CuentaAdministrativa.usuario) == usuario)
        )

    def persona(self, persona_id: int) -> Persona | None:
        return self.sesion.get(Persona, persona_id)

    def persona_por_cedula(self, cedula: str) -> Persona | None:
        return self.sesion.scalar(select(Persona).where(Persona.cedula == cedula))

    def cuenta_por_persona(self, persona_id: int) -> CuentaAdministrativa | None:
        return self.sesion.scalar(
            select(CuentaAdministrativa).where(CuentaAdministrativa.persona_id == persona_id)
        )

    def profesores_disponibles(self):
        cuenta_persona = select(CuentaAdministrativa.persona_id).where(
            CuentaAdministrativa.persona_id.is_not(None)
        )
        return self.sesion.scalars(
            select(Persona)
            .where(
                Persona.tipo == "profesor",
                Persona.activo.is_(True),
                Persona.id.not_in(cuenta_persona),
            )
            .order_by(Persona.nombres)
        ).all()

    def listar_permisos(self):
        return self.sesion.scalars(
            select(PermisoAdministrativo).order_by(
                PermisoAdministrativo.modulo, PermisoAdministrativo.clave
            )
        ).all()

    def permisos_cuenta(self, cuenta_id: int) -> list[str]:
        return list(
            self.sesion.scalars(
                select(CuentaPermiso.permiso_clave)
                .where(CuentaPermiso.cuenta_id == cuenta_id)
                .order_by(CuentaPermiso.permiso_clave)
            ).all()
        )

    def guardar(self, registro):
        self.sesion.add(registro)
        self.sesion.flush()
        return registro

    def crear_profesor(
        self, persona: Persona, credencial: CredencialPortal, cuenta_tiquete: CuentaTiquete
    ) -> Persona:
        self.sesion.add(persona)
        self.sesion.flush()
        credencial.persona_id = persona.id
        cuenta_tiquete.persona_id = persona.id
        self.sesion.add_all([credencial, cuenta_tiquete])
        self.sesion.flush()
        return persona

    def credencial_portal(self, persona_id: int) -> CredencialPortal | None:
        return self.sesion.get(CredencialPortal, persona_id)

    def asignar_permisos(self, cuenta_id: int, permisos: list[str]) -> None:
        self.sesion.execute(delete(CuentaPermiso).where(CuentaPermiso.cuenta_id == cuenta_id))
        self.sesion.add_all(
            CuentaPermiso(cuenta_id=cuenta_id, permiso_clave=clave)
            for clave in sorted(set(permisos))
        )
        self.sesion.flush()

    def revocar_sesiones(self, cuenta_id: int) -> None:
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.cuenta_id == cuenta_id))

    def contar_administradores_activos(self) -> int:
        return int(
            self.sesion.scalar(
                select(func.count(CuentaAdministrativa.id)).where(
                    CuentaAdministrativa.rol == "administrador",
                    CuentaAdministrativa.activo.is_(True),
                    CuentaAdministrativa.vinculacion_pendiente.is_(False),
                )
            )
            or 0
        )

    def bloquear_administradores(self) -> None:
        if self.sesion.bind is not None and self.sesion.bind.dialect.name != "sqlite":
            self.sesion.execute(
                select(CuentaAdministrativa.id)
                .where(CuentaAdministrativa.rol == "administrador")
                .with_for_update()
            )
