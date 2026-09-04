"""Consulta pública de asignaciones de ruta requeridas por la operación."""

from sqlalchemy import or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AsignacionRuta


class RepositorioOperacionRutas:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def ruta_vigente(self, matricula_id: int, fecha):
        return self.sesion.scalar(
            select(AsignacionRuta).where(
                AsignacionRuta.matricula_id == matricula_id,
                AsignacionRuta.fecha_inicio <= fecha,
                or_(AsignacionRuta.fecha_fin.is_(None), AsignacionRuta.fecha_fin >= fecha),
            )
        )
