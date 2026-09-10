"""Consultas compuestas que no pertenecen a los adaptadores HTTP."""

import json
from datetime import date

from sqlalchemy import func, or_, select, update
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    ConfiguracionInstitucional,
    Matricula,
    Persona,
    Ruta,
)
from aplicacion.modelos.operacion import (
    EventoExportacionListaControl,
    IndicadorAnaliticoComedor,
    IngresoComedor,
    MarcaTransporte,
    ReservaComedor,
    VentaTiquete,
)


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

    def configuracion_institucional(self):
        return self.sesion.get(ConfiguracionInstitucional, 1)

    def personas_dashboard(self, fecha: date, tipo_persona: str):
        if tipo_persona == "profesor":
            profesores = self.sesion.scalars(
                select(Persona)
                .where(Persona.tipo == "profesor", Persona.activo.is_(True))
                .order_by(Persona.nombres)
            ).all()
            # La forma de salida del tablero siempre es (persona, matrícula,
            # ruta). Un profesor no posee matrícula ni ruta escolar: se
            # representa explícitamente con None, sin joins artificiales.
            return [(profesor, None, None) for profesor in profesores]
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

    def reservas_confirmadas_en_fecha(self, fecha: date) -> set[int]:
        return set(
            self.sesion.scalars(
                select(ReservaComedor.persona_id).where(
                    ReservaComedor.fecha == fecha,
                    ReservaComedor.estado.in_(("reservada", "consumida")),
                )
            )
        )

    def matriculas_con_marca_transporte_en_fecha(self, fecha: date) -> set[int]:
        return set(
            self.sesion.scalars(
                select(MarcaTransporte.matricula_id).where(MarcaTransporte.fecha == fecha)
            )
        )

    def registrar_exportacion_lista_control(
        self, cuenta_id: int, servicio: str, formato: str, fecha: date, filtros: dict, total: int
    ) -> None:
        # La búsqueda puede contener nombre o identificación: la trazabilidad no
        # debe replicar esa información personal fuera del padrón.
        filtros_auditoria = {
            clave: valor for clave, valor in filtros.items() if clave != "busqueda" and valor
        }
        self.sesion.add(
            EventoExportacionListaControl(
                cuenta_administrativa_id=cuenta_id,
                servicio=servicio,
                formato=formato,
                fecha_operativa=fecha,
                filtros=json.dumps(filtros_auditoria, ensure_ascii=False, sort_keys=True),
                total_registros=total,
            )
        )

    def alertas_analiticas(self, fecha: date):
        ultima = self.sesion.scalar(
            select(func.max(IndicadorAnaliticoComedor.fecha_corte)).where(
                IndicadorAnaliticoComedor.fecha_corte <= fecha
            )
        )
        if ultima is None:
            return []
        return self.sesion.execute(
            select(IndicadorAnaliticoComedor.senal, func.count())
            .where(
                IndicadorAnaliticoComedor.fecha_corte == ultima,
                IndicadorAnaliticoComedor.senal != "sin datos suficientes",
            )
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
