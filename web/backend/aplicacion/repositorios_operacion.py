"""Persistencia de tiquetes, comedor y transporte."""

from datetime import date

from sqlalchemy import func, or_, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    HorarioReserva,
    Matricula,
    Persona,
    FotografiaPersona,
    ConfiguracionInstitucional,
)
from aplicacion.modelos.operacion import (
    AutorizacionComedor,
    CuentaTiquete,
    EventoOperacionComedor,
    IngresoComedor,
    MarcaTransporte,
    MovimientoTiquete,
    ReservaComedor,
    Tarifa,
)


class RepositorioOperacion:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def listar_tarifas(self):
        return self.sesion.scalars(select(Tarifa).order_by(Tarifa.fecha_inicio.desc())).all()

    def tarifa_solapada(self, datos, excluir_id=None):
        return (
            self.sesion.scalar(
                select(Tarifa.id).where(
                    Tarifa.tipo_persona == datos.tipo_persona,
                    Tarifa.fecha_inicio <= (datos.fecha_fin or date.max),
                    or_(Tarifa.fecha_fin.is_(None), Tarifa.fecha_fin >= datos.fecha_inicio),
                ).where(Tarifa.id != excluir_id) if excluir_id else select(Tarifa.id).where(
                    Tarifa.tipo_persona == datos.tipo_persona,
                    Tarifa.fecha_inicio <= (datos.fecha_fin or date.max),
                    or_(Tarifa.fecha_fin.is_(None), Tarifa.fecha_fin >= datos.fecha_inicio),
                )
            )
            is not None
        )

    def tarifa(self, tarifa_id):
        return self.sesion.get(Tarifa, tarifa_id)

    def tarifa_vigente(self, tipo, fecha):
        return self.sesion.scalar(
            select(Tarifa)
            .where(
                Tarifa.tipo_persona == tipo,
                Tarifa.fecha_inicio <= fecha,
                or_(Tarifa.fecha_fin.is_(None), Tarifa.fecha_fin >= fecha),
            )
            .order_by(Tarifa.fecha_inicio.desc())
        )

    def persona(self, persona_id):
        return self.sesion.get(Persona, persona_id)

    def persona_cedula(self, cedula):
        return self.sesion.scalar(
            select(Persona).where(
                Persona.cedula == cedula, Persona.activo.is_(True)
            )
        )

    def buscar_personas_venta(self, termino):
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

    def foto_persona(self, persona_id):
        return self.sesion.scalar(select(FotografiaPersona).where(FotografiaPersona.persona_id == persona_id))

    def listar_horarios_reserva(self):
        return self.sesion.scalars(select(HorarioReserva).order_by(HorarioReserva.turno)).all()

    def actualizar_horario_reserva(self, turno, hora_limite):
        horario = self.sesion.get(HorarioReserva, turno)
        if not horario:
            return None
        horario.hora_limite = hora_limite
        self.sesion.flush()
        return horario

    def configuracion_institucional(self):
        return self.sesion.get(ConfiguracionInstitucional, 1)

    def guardar_configuracion_institucional(self, datos):
        configuracion = self.configuracion_institucional()
        if not configuracion:
            configuracion = ConfiguracionInstitucional(id=1, **datos.model_dump())
            return self.guardar(configuracion)
        configuracion.nombre_colegio = datos.nombre_colegio.strip()
        configuracion.subtitulo_reportes = datos.subtitulo_reportes.strip()
        self.sesion.flush()
        return configuracion

    def matricula(self, matricula_id):
        return self.sesion.get(Matricula, matricula_id)

    def matricula_fecha(self, persona_id, fecha):
        return self.sesion.scalar(
            select(Matricula)
            .join(AnioLectivo)
            .where(
                Matricula.persona_id == persona_id,
                AnioLectivo.anio == fecha.year,
                Matricula.estado == "activo",
            )
        )

    def horario(self, turno):
        return self.sesion.get(HorarioReserva, turno)

    def cuenta(self, persona_id):
        return self.sesion.get(CuentaTiquete, persona_id)

    def obtener_o_crear_cuenta(self, persona_id):
        registro = self.cuenta(persona_id)
        if not registro:
            registro = self.guardar(CuentaTiquete(persona_id=persona_id, saldo=0, reservados=0))
        return registro

    def reserva_fecha(self, persona_id, fecha, activa=False):
        consulta = select(ReservaComedor).where(
            ReservaComedor.persona_id == persona_id, ReservaComedor.fecha == fecha
        )
        if activa:
            consulta = consulta.where(ReservaComedor.estado == "reservada")
        return self.sesion.scalar(consulta)

    def ingreso_fecha(self, persona_id, fecha):
        return self.sesion.scalar(
            select(IngresoComedor).where(
                IngresoComedor.persona_id == persona_id, IngresoComedor.fecha == fecha
            )
        )

    def tiene_marca_transporte(self, matricula_id, fecha):
        if not matricula_id:
            return False
        return (
            self.sesion.scalar(
                select(MarcaTransporte.id).where(
                    MarcaTransporte.matricula_id == matricula_id,
                    MarcaTransporte.fecha == fecha,
                )
            )
            is not None
        )

    def autorizacion_aprobada(self, persona_id, fecha):
        return self.sesion.scalar(
            select(AutorizacionComedor).where(
                AutorizacionComedor.persona_id == persona_id,
                AutorizacionComedor.fecha == fecha,
                AutorizacionComedor.decision == "aprobada",
            )
        )

    def ruta_vigente(self, matricula_id, fecha):
        return self.sesion.scalar(
            select(AsignacionRuta).where(
                AsignacionRuta.matricula_id == matricula_id,
                AsignacionRuta.fecha_inicio <= fecha,
                or_(AsignacionRuta.fecha_fin.is_(None), AsignacionRuta.fecha_fin >= fecha),
            )
        )

    def guardar(self, *registros):
        self.sesion.add_all(registros)
        self.sesion.flush()
        return registros[0]

    def registrar_evento(
        self,
        *,
        fecha,
        codigo,
        resultado,
        operador_id,
        persona_id=None,
        motivo=None,
        advertencia=False,
        duracion_ms=None,
    ):
        return self.guardar(
            EventoOperacionComedor(
                fecha_operativa=fecha,
                codigo_capturado=codigo,
                resultado=resultado,
                operador_id=operador_id,
                persona_id=persona_id,
                motivo=motivo,
                advertencia=advertencia,
                duracion_ms=duracion_ms,
            )
        )

    def estado_captura(self, fecha):
        total = (
            self.sesion.scalar(
                select(func.count(IngresoComedor.id)).where(IngresoComedor.fecha == fecha)
            )
            or 0
        )
        reservas = (
            self.sesion.scalar(
                select(func.count(ReservaComedor.id))
                .join(Persona, Persona.id == ReservaComedor.persona_id)
                .where(
                    ReservaComedor.fecha == fecha,
                    ReservaComedor.estado.in_(("reservada", "consumida")),
                    Persona.tipo == "estudiante",
                )
            )
            or 0
        )
        eventos = self.sesion.execute(
            select(EventoOperacionComedor, Persona)
            .outerjoin(Persona, Persona.id == EventoOperacionComedor.persona_id)
            .where(EventoOperacionComedor.fecha_operativa == fecha)
            .order_by(EventoOperacionComedor.fecha_evento.desc())
            .limit(12)
        ).all()
        duplicados = (
            self.sesion.scalar(
                select(func.count(EventoOperacionComedor.id)).where(
                    EventoOperacionComedor.fecha_operativa == fecha,
                    EventoOperacionComedor.resultado == "duplicado",
                )
            )
            or 0
        )
        errores = (
            self.sesion.scalar(
                select(func.count(EventoOperacionComedor.id)).where(
                    EventoOperacionComedor.fecha_operativa == fecha,
                    EventoOperacionComedor.resultado.not_in(("aceptado", "duplicado")),
                )
            )
            or 0
        )
        return int(total), int(reservas), int(duplicados), int(errores), eventos

    def movimiento(self, persona_id, tipo, cantidad, saldo, referencia=None):
        self.guardar(
            MovimientoTiquete(
                persona_id=persona_id,
                tipo=tipo,
                cantidad=cantidad,
                saldo_resultante=saldo,
                referencia=referencia,
            )
        )
