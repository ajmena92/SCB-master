"""Persistencia de rutas y sus asignaciones."""

from datetime import date

from sqlalchemy import func, or_, select

from aplicacion.modelos.maestros import AnioLectivo, AsignacionRuta, Ruta


class RepositorioCatalogosRutas:
    def __init__(self, sesion):
        self.sesion = sesion

    def listar_rutas(self):
        return self.sesion.execute(
            select(Ruta, func.count(AsignacionRuta.id))
            .outerjoin(AsignacionRuta, AsignacionRuta.ruta_id == Ruta.id)
            .group_by(Ruta.id)
            .order_by(Ruta.codigo)
        ).all()

    def listar_rutas_activas(self):
        return self.sesion.execute(
            select(Ruta, func.count(AsignacionRuta.id))
            .outerjoin(AsignacionRuta, AsignacionRuta.ruta_id == Ruta.id)
            .where(Ruta.activo.is_(True), Ruta.codigo != "0000")
            .group_by(Ruta.id)
            .order_by(Ruta.codigo)
        ).all()

    def anio_vigente(self):
        return self.sesion.scalar(select(AnioLectivo).where(AnioLectivo.vigente.is_(True)))

    def contar_asignados(self, ruta_id: int) -> int:
        return int(
            self.sesion.scalar(
                select(func.count(AsignacionRuta.id)).where(AsignacionRuta.ruta_id == ruta_id)
            )
            or 0
        )

    def ruta(self, ruta_id: int):
        return self.sesion.get(Ruta, ruta_id)

    def asignacion_solapada(self, entrada) -> bool:

        return (
            self.sesion.scalar(
                select(AsignacionRuta.id).where(
                    AsignacionRuta.matricula_id == entrada.matricula_id,
                    AsignacionRuta.fecha_inicio <= (entrada.fecha_fin or date.max),
                    or_(
                        AsignacionRuta.fecha_fin.is_(None),
                        AsignacionRuta.fecha_fin >= entrada.fecha_inicio,
                    ),
                )
            )
            is not None
        )
