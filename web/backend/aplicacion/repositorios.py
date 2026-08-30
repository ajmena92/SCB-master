"""Consultas compuestas que no pertenecen a los adaptadores HTTP."""

from datetime import date

from sqlalchemy import func, select, update
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AnioLectivo, Persona, Ruta
from aplicacion.modelos.operacion import IngresoComedor, MarcaTransporte, VentaTiquete


def desactivar_anios(sesion: Session) -> None:
    sesion.execute(update(AnioLectivo).values(vigente=False))
    sesion.flush()


def filas_reporte_comedor(sesion: Session, desde: date, hasta: date):
    return sesion.execute(
        select(
            IngresoComedor.fecha,
            Persona.codigo,
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
            Persona.codigo,
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
