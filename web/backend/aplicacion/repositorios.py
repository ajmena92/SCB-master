"""Consultas compuestas que no pertenecen a los adaptadores HTTP."""

from datetime import date

from sqlalchemy import and_, func, or_, select, update
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AnioLectivo, AsignacionRuta, Matricula, Persona, Ruta
from aplicacion.modelos.operacion import IndicadorAnaliticoComedor, IngresoComedor, MarcaTransporte, VentaTiquete


def desactivar_anios(sesion: Session) -> None:
    sesion.execute(update(AnioLectivo).values(vigente=False))
    sesion.flush()


def filas_reporte_comedor(sesion: Session, desde: date, hasta: date):
    return sesion.execute(
        select(
            IngresoComedor.fecha,
            Persona.cedula,
            Persona.nombres,
            IngresoComedor.modalidad,
            IngresoComedor.consumio_tiquete,
        )
        .join(Persona, Persona.id == IngresoComedor.persona_id)
        .where(IngresoComedor.fecha.between(desde, hasta))
        .order_by(IngresoComedor.fecha)
    ).all()


def filas_reporte_transporte(sesion: Session, desde: date, hasta: date):
    return sesion.execute(
        select(MarcaTransporte.fecha, MarcaTransporte.matricula_id, Ruta.nombre)
        .join(Ruta, Ruta.id == MarcaTransporte.ruta_id)
        .where(MarcaTransporte.fecha.between(desde, hasta))
        .order_by(MarcaTransporte.fecha)
    ).all()


def filas_reporte_ventas(sesion: Session, desde: date, hasta: date):
    return sesion.execute(
        select(
            VentaTiquete.creado_en,
            Persona.cedula,
            VentaTiquete.cantidad,
            VentaTiquete.tarifa_aplicada,
            VentaTiquete.total,
            VentaTiquete.medio_pago,
        )
        .join(Persona, Persona.id == VentaTiquete.persona_id)
        .where(func.date(VentaTiquete.creado_en).between(desde, hasta))
        .order_by(VentaTiquete.creado_en)
    ).all()


class RepositorioReportes:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def comedor(self, desde: date, hasta: date):
        return filas_reporte_comedor(self.sesion, desde, hasta)

    def transporte(self, desde: date, hasta: date):
        return filas_reporte_transporte(self.sesion, desde, hasta)

    def ventas(self, desde: date, hasta: date):
        return filas_reporte_ventas(self.sesion, desde, hasta)

    def personas_dashboard(self, fecha: date, tipo_persona: str):
        if tipo_persona == "profesor":
            return self.sesion.execute(
                select(Persona, Matricula, Ruta)
                .outerjoin(Matricula, Matricula.id == -1)
                .outerjoin(Ruta, Ruta.id == -1)
                .where(Persona.tipo == "profesor", Persona.activo.is_(True))
                .order_by(Persona.nombres)
            ).all()
        # El tablero representa personas, no asignaciones. Si un dato histórico
        # contiene rutas con vigencias solapadas, se elige sólo la más reciente.
        asignacion_vigente_id = (
            select(AsignacionRuta.id)
            .where(
                AsignacionRuta.matricula_id == Matricula.id,
                AsignacionRuta.fecha_inicio <= fecha,
                or_(AsignacionRuta.fecha_fin.is_(None), AsignacionRuta.fecha_fin >= fecha),
            )
            .order_by(AsignacionRuta.fecha_inicio.desc(), AsignacionRuta.id.desc())
            .limit(1)
            .correlate(Matricula)
            .scalar_subquery()
        )
        return self.sesion.execute(
            select(Persona, Matricula, Ruta)
            .join(Matricula, Matricula.persona_id == Persona.id)
            .join(AnioLectivo, AnioLectivo.id == Matricula.anio_lectivo_id)
            .outerjoin(
                AsignacionRuta,
                AsignacionRuta.id == asignacion_vigente_id,
            )
            .outerjoin(Ruta, Ruta.id == AsignacionRuta.ruta_id)
            .where(
                Persona.tipo == "estudiante",
                Persona.activo.is_(True),
                AnioLectivo.anio == fecha.year,
                Matricula.estado == "activo",
            )
            .order_by(Persona.nombres)
        ).all()

    def ingresos_en_fechas(self, fechas: list[date]):
        if not fechas:
            return []
        return self.sesion.execute(
            select(IngresoComedor.persona_id, IngresoComedor.fecha).where(
                IngresoComedor.fecha.in_(fechas)
            )
        ).all()

    def alertas_analiticas(self, fecha: date):
        ultima = self.sesion.scalar(select(func.max(IndicadorAnaliticoComedor.fecha_corte)).where(IndicadorAnaliticoComedor.fecha_corte <= fecha))
        if ultima is None:
            return []
        return self.sesion.execute(
            select(IndicadorAnaliticoComedor.senal, func.count())
            .where(IndicadorAnaliticoComedor.fecha_corte == ultima, IndicadorAnaliticoComedor.senal != "sin datos suficientes")
            .group_by(IndicadorAnaliticoComedor.senal)
        ).all()

    def casos_analiticos(self, fecha: date, limite: int = 50):
        ultima = self.sesion.scalar(
            select(func.max(IndicadorAnaliticoComedor.fecha_corte)).where(
                IndicadorAnaliticoComedor.fecha_corte <= fecha
            )
        )
        if ultima is None:
            return []
        return self.sesion.execute(
            select(IndicadorAnaliticoComedor, Persona, Matricula)
            .join(Persona, Persona.id == IndicadorAnaliticoComedor.persona_id)
            .join(Matricula, Matricula.persona_id == Persona.id)
            .join(AnioLectivo, AnioLectivo.id == Matricula.anio_lectivo_id)
            .where(
                IndicadorAnaliticoComedor.fecha_corte == ultima,
                IndicadorAnaliticoComedor.senal != "sin datos suficientes",
                Persona.activo.is_(True),
                Matricula.estado == "activo",
                AnioLectivo.anio == ultima.year,
            )
            .order_by(IndicadorAnaliticoComedor.porcentaje_asistencia, Persona.nombres)
            .limit(limite)
        ).all()
