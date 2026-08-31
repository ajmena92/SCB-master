"""Consultas de solo lectura para la experiencia web del portal."""

from datetime import date

from sqlalchemy import or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AnioLectivo, AsignacionRuta, Matricula, Ruta
from aplicacion.modelos.menu import ComponentePublicado, PublicacionMenu
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
        publicacion = self.sesion.scalar(
            select(PublicacionMenu).where(PublicacionMenu.fecha == fecha)
        )
        if not publicacion:
            return None, []
        componentes = self.sesion.scalars(
            select(ComponentePublicado)
            .where(ComponentePublicado.publicacion_id == publicacion.id)
            .order_by(ComponentePublicado.orden)
        ).all()
        return publicacion, componentes

    def reserva_fecha(self, persona_id: int, fecha: date):
        return self.sesion.scalar(
            select(ReservaComedor).where(
                ReservaComedor.persona_id == persona_id,
                ReservaComedor.fecha == fecha,
            )
        )
