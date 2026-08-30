"""Persistencia de importaciones anuales."""

from sqlalchemy import select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    CredencialPortal,
    Matricula,
    Persona,
    Ruta,
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

    def codigo_existe(self, codigo):
        return self.sesion.scalar(select(Persona.id).where(Persona.codigo == codigo)) is not None

    def matricula(self, persona_id, anio_id):
        return self.sesion.scalar(
            select(Matricula).where(
                Matricula.persona_id == persona_id, Matricula.anio_lectivo_id == anio_id
            )
        )

    def ruta_nombre(self, nombre):
        return self.sesion.scalar(select(Ruta).where(Ruta.nombre == nombre))

    def asignacion(self, matricula_id, inicio):
        return self.sesion.scalar(
            select(AsignacionRuta.id).where(
                AsignacionRuta.matricula_id == matricula_id, AsignacionRuta.fecha_inicio == inicio
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
