"""Consultas de personas requeridas por la operación, como contrato explícito."""

from sqlalchemy import or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AnioLectivo, FotografiaPersona, Matricula, Persona


class RepositorioOperacionPersonas:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def persona(self, persona_id: int):
        return self.sesion.get(Persona, persona_id)

    def persona_cedula(self, cedula: str):
        return self.sesion.scalar(
            select(Persona).where(Persona.cedula == cedula, Persona.activo.is_(True))
        )

    def buscar_personas_venta(self, termino: str):
        patron = f"%{' '.join(termino.split())}%"
        return self.sesion.scalars(
            select(Persona)
            .where(
                Persona.activo.is_(True),
                or_(Persona.cedula.ilike(patron), Persona.nombres.ilike(patron)),
            )
            .order_by(Persona.nombres, Persona.id)
            .limit(8)
        ).all()

    def foto_persona(self, persona_id: int):
        return self.sesion.scalar(
            select(FotografiaPersona).where(FotografiaPersona.persona_id == persona_id)
        )

    def matricula_fecha(self, persona_id: int, fecha):
        return self.sesion.scalar(
            select(Matricula)
            .join(AnioLectivo)
            .where(
                Matricula.persona_id == persona_id,
                AnioLectivo.anio == fecha.year,
                Matricula.estado == "activo",
            )
        )
