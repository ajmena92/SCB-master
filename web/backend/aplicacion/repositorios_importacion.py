"""Persistencia de importaciones anuales."""

from sqlalchemy import delete, func, or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    CredencialPortal,
    CuentaAdministrativa,
    Matricula,
    Persona,
    SesionAcceso,
)
from aplicacion.modelos.operacion import CuentaTiquete, LoteImportacion, TrabajoImportacion


class RepositorioImportacion:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def lote(self, huella):
        return self.sesion.scalar(select(LoteImportacion).where(LoteImportacion.huella == huella))

    def trabajo_por_huella(self, huella: str) -> TrabajoImportacion | None:
        return self.sesion.scalar(
            select(TrabajoImportacion).where(TrabajoImportacion.huella == huella)
        )

    def trabajo(self, trabajo_id: int) -> TrabajoImportacion | None:
        return self.sesion.get(TrabajoImportacion, trabajo_id)

    def trabajo_para_entrega(self, trabajo_id: int) -> TrabajoImportacion | None:
        return self.sesion.scalar(
            select(TrabajoImportacion).where(TrabajoImportacion.id == trabajo_id).with_for_update()
        )

    def recuperar_trabajos_interrumpidos(self, antes_de) -> int:
        trabajos = self.sesion.scalars(
            select(TrabajoImportacion)
            .where(
                TrabajoImportacion.estado == "ejecutando", TrabajoImportacion.iniciado_en < antes_de
            )
            .with_for_update(skip_locked=True)
        ).all()
        for trabajo in trabajos:
            trabajo.estado, trabajo.iniciado_en = "pendiente", None
        self.sesion.flush()
        return len(trabajos)

    def tomar_trabajo_pendiente(self) -> TrabajoImportacion | None:
        trabajo = self.sesion.scalar(
            select(TrabajoImportacion)
            .where(TrabajoImportacion.estado == "pendiente")
            .order_by(TrabajoImportacion.creado_en, TrabajoImportacion.id)
            .with_for_update(skip_locked=True)
            .limit(1)
        )
        if trabajo is not None:
            from datetime import datetime, timezone

            trabajo.estado = "ejecutando"
            trabajo.iniciado_en = datetime.now(timezone.utc)
            self.sesion.flush()
        return trabajo

    def anio(self, valor):
        return self.sesion.scalar(select(AnioLectivo).where(AnioLectivo.anio == valor))

    def persona_cedula(self, cedula):
        return self.sesion.scalar(select(Persona).where(Persona.cedula == cedula))

    def personas_por_cedulas(self, cedulas: set[str]) -> dict[str, Persona]:
        valores = list(cedulas)
        personas: dict[str, Persona] = {}
        for inicio in range(0, len(valores), 500):
            consulta = select(Persona).where(Persona.cedula.in_(valores[inicio : inicio + 500]))
            personas.update(
                (p.cedula, p) for p in self.sesion.scalars(consulta) if p.cedula is not None
            )
        return personas

    def contar_activas_ausentes(self, tipos: set[str], cedulas_presentes: set[str]) -> int:
        consulta = (
            select(func.count())
            .select_from(Persona)
            .outerjoin(CuentaAdministrativa, CuentaAdministrativa.persona_id == Persona.id)
            .where(
                Persona.activo.is_(True),
                Persona.tipo.in_(tipos),
                or_(CuentaAdministrativa.id.is_(None), CuentaAdministrativa.activo.is_(False)),
            )
        )
        if cedulas_presentes:
            consulta = consulta.where(Persona.cedula.not_in(cedulas_presentes))
        return self.sesion.scalar(consulta) or 0

    def matriculas_por_personas(
        self, personas_ids: list[int], anio_id: int
    ) -> dict[int, Matricula]:
        valores = list(personas_ids)
        matriculas: dict[int, Matricula] = {}
        for inicio in range(0, len(valores), 500):
            consulta = select(Matricula).where(
                Matricula.anio_lectivo_id == anio_id,
                Matricula.persona_id.in_(valores[inicio : inicio + 500]),
            )
            matriculas.update((m.persona_id, m) for m in self.sesion.scalars(consulta))
        return matriculas

    def activas_ausentes_del_padron(self, tipos, cedulas_presentes):
        consulta = (
            select(Persona)
            .outerjoin(CuentaAdministrativa, CuentaAdministrativa.persona_id == Persona.id)
            .where(
                Persona.activo.is_(True),
                Persona.tipo.in_(tipos),
                or_(CuentaAdministrativa.id.is_(None), CuentaAdministrativa.activo.is_(False)),
            )
        )
        if cedulas_presentes:
            consulta = consulta.where(Persona.cedula.not_in(cedulas_presentes))
        return self.sesion.scalars(consulta).all()

    def desactivar_personas(self, personas):
        ids = [persona.id for persona in personas]
        if not ids:
            return
        self.sesion.execute(delete(SesionAcceso).where(SesionAcceso.persona_id.in_(ids)))
        for persona in personas:
            persona.activo = False
        self.sesion.flush()

    def matricula(self, persona_id, anio_id):
        return self.sesion.scalar(
            select(Matricula).where(
                Matricula.persona_id == persona_id, Matricula.anio_lectivo_id == anio_id
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
