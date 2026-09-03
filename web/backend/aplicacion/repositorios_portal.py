"""Consultas de solo lectura para la experiencia web del portal."""

from datetime import date

from sqlalchemy import or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    FotografiaPersona,
    HorarioReserva,
    Matricula,
    Ruta,
)
from aplicacion.modelos.menu import (
    CalendarioMenu,
    ComponenteMenu,
    ComponenteSustitucionMenu,
    PlantillaMenu,
    SustitucionMenu,
)
from aplicacion.modelos.operacion import ReservaComedor


class RepositorioPortal:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def matricula_fecha(self, persona_id: int, fecha: date):
        return self.sesion.scalar(
            select(Matricula)
            .join(AnioLectivo)
            .where(
                Matricula.persona_id == persona_id,
                AnioLectivo.anio == fecha.year,
                Matricula.estado == "activo",
            )
        )

    def foto_persona(self, persona_id: int):
        return self.sesion.scalar(
            select(FotografiaPersona).where(FotografiaPersona.persona_id == persona_id)
        )

    def ruta_fecha(self, matricula_id: int, fecha: date):
        return self.sesion.execute(
            select(Ruta)
            .join(AsignacionRuta, AsignacionRuta.ruta_id == Ruta.id)
            .where(
                AsignacionRuta.matricula_id == matricula_id,
                AsignacionRuta.fecha_inicio <= fecha,
                or_(AsignacionRuta.fecha_fin.is_(None), AsignacionRuta.fecha_fin >= fecha),
            )
        ).scalar_one_or_none()

    def menu_fecha(self, fecha: date):
        if fecha.isoweekday() > 5:
            return None, [], "sin_menu"
        calendario = self.sesion.get(CalendarioMenu, fecha)
        if calendario and not calendario.habilitado:
            return None, [], "cerrado"
        sustitucion = self.sesion.scalar(
            select(SustitucionMenu).where(SustitucionMenu.fecha == fecha)
        )
        if sustitucion:
            componentes = self.sesion.scalars(
                select(ComponenteSustitucionMenu)
                .where(ComponenteSustitucionMenu.sustitucion_id == sustitucion.id)
                .order_by(ComponenteSustitucionMenu.orden)
            ).all()
            return sustitucion, componentes, "sustitucion"
        semana = (fecha.day - 1) // 7 + 1
        plantilla = self.sesion.scalar(
            select(PlantillaMenu).where(
                PlantillaMenu.semana == semana,
                PlantillaMenu.dia == fecha.isoweekday(),
                PlantillaMenu.activo.is_(True),
            )
        )
        if not plantilla:
            return None, [], "sin_menu"
        componentes = self.sesion.scalars(
            select(ComponenteMenu)
            .where(ComponenteMenu.plantilla_id == plantilla.id)
            .order_by(ComponenteMenu.orden)
        ).all()
        return plantilla, componentes, "plantilla"

    def reserva_fecha(self, persona_id: int, fecha: date):
        return self.sesion.scalar(
            select(ReservaComedor).where(
                ReservaComedor.persona_id == persona_id,
                ReservaComedor.fecha == fecha,
            )
        )

    def horario_reserva_general(self):
        return self.sesion.get(HorarioReserva, "general")
