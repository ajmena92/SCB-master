"""Persistencia de importaciones anuales."""

from sqlalchemy import delete, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    CredencialPortal,
    Matricula,
    Persona,
    SesionAcceso,
)
from aplicacion.modelos.operacion import CuentaTiquete, LoteImportacion


class RepositorioImportacion:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def lote(self, huella):
        return self.sesion.scalar(select(LoteImportacion).where(LoteImportacion.huella == huella))

    def anio(self, valor):
        return self.sesion.scalar(select(AnioLectivo).where(AnioLectivo.anio == valor))

    def persona_cedula(self, cedula):
        return self.sesion.scalar(select(Persona).where(Persona.cedula == cedula))

    def activas_ausentes_del_padron(self, tipos, cedulas_presentes):
        consulta = select(Persona).where(Persona.activo.is_(True), Persona.tipo.in_(tipos))
        if cedulas_presentes:
            consulta = consulta.where(Persona.cedula.not_in(cedulas_presentes))
        return self.sesion.scalars(consulta).all()

    def desactivar_personas(self, personas):
        ids = [persona.id for persona in personas]
        if not ids:
            return
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.persona_id.in_(ids)))
        for persona in personas:
            persona.activo = False
        self.sesion.flush()

    def matricula(self, persona_id, anio_id):
        return self.sesion.scalar(
            select(Matricula).where(
                Matricula.persona_id == persona_id, Matricula.anio_lectivo_id == anio_id
            )
        )

    def guardar(self, *registros):
        self.sesion.add_all(registros)
        self.sesion.flush()
        return registros[0]

    def guardar_persona_nueva(self, persona, pin_hash):
        self.guardar(persona)
        self.guardar(
            CredencialPortal(persona_id=persona.id, pin_hash=pin_hash),
            CuentaTiquete(persona_id=persona.id, saldo=0, reservados=0),
        )
