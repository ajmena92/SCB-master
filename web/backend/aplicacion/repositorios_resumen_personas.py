"""Carga por página de las relaciones del resumen de personas."""

from collections.abc import Sequence
from datetime import date

from sqlalchemy import or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import AnioLectivo, AsignacionRuta, Matricula, Persona, Ruta
from aplicacion.modelos.operacion import CuentaTiquete


def cargar_relaciones_personas(
    sesion: Session,
    personas: Sequence[Persona],
    anio: AnioLectivo | None,
) -> dict[int, tuple[Matricula | None, Ruta | None, CuentaTiquete | None]]:
    ids = [p.id for p in personas]
    if not ids:
        return {}
    cuentas = {
        c.persona_id: c
        for c in sesion.scalars(select(CuentaTiquete).where(CuentaTiquete.persona_id.in_(ids)))
    }
    matriculas = (
        {
            m.persona_id: m
            for m in sesion.scalars(
                select(Matricula).where(
                    Matricula.persona_id.in_([p.id for p in personas if p.tipo == "estudiante"]),
                    Matricula.anio_lectivo_id == anio.id,
                )
            )
        }
        if anio
        else {}
    )
    rutas: dict[int, Ruta] = {}
    if matriculas:
        consulta = (
            select(AsignacionRuta, Ruta)
            .join(Ruta, Ruta.id == AsignacionRuta.ruta_id)
            .where(
                AsignacionRuta.matricula_id.in_([m.id for m in matriculas.values()]),
                AsignacionRuta.fecha_inicio <= date.today(),
                or_(AsignacionRuta.fecha_fin.is_(None), AsignacionRuta.fecha_fin >= date.today()),
            )
            .order_by(AsignacionRuta.fecha_inicio.desc(), AsignacionRuta.id.desc())
        )
        for asignacion, ruta in sesion.execute(consulta):
            rutas.setdefault(asignacion.matricula_id, ruta)
    return {
        p.id: (
            matriculas.get(p.id),
            rutas.get(matriculas[p.id].id) if p.id in matriculas else None,
            cuentas.get(p.id),
        )
        for p in personas
    }
