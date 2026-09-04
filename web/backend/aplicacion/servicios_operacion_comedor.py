"""Casos de uso de reservas, ingresos y transporte."""

from datetime import datetime, time
from zoneinfo import ZoneInfo

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.modelos.operacion import (
    AutorizacionComedor, IngresoComedor, MarcaTransporte, ReservaComedor,
)
from aplicacion.servicios_operacion_base import ServicioOperacionBase

ZONA_INSTITUCIONAL = ZoneInfo("America/Costa_Rica")


class ServicioOperacionComedor(ServicioOperacionBase):
    def __init__(self, repositorio_operacion, repositorio_personas, repositorio_rutas):
        super().__init__(repositorio_operacion, repositorio_personas)
        self.rutas = repositorio_rutas

    def reservar(self, datos, identidad):
        persona = (
            identidad["persona"]
            if identidad["tipo"] == "portal" and not datos.cedula
            else self._persona(cedula=datos.cedula)
        )
        if identidad["tipo"] == "portal" and identidad["persona"].cedula != persona.cedula:
            raise HTTPException(403, "No puede reservar para otra persona")
        if self.repo.reserva_fecha(persona.id, datos.fecha):
            raise HTTPException(409, "Ya existe una reserva")
        matricula = self._matricula(persona, datos.fecha)
        if persona.tipo == "estudiante" and not matricula:
            raise HTTPException(409, "Sin matricula activa")
        # El comedor opera con una única hora límite institucional; no depende
        # de comparar horarios de estudiantes ni de la hora de transporte.
        horario = self.repo.horario("general")
        ahora = datetime.now(ZONA_INSTITUCIONAL)
        if (
            horario
            and datos.fecha == ahora.date()
            and ahora.time() > time.fromisoformat(horario.hora_limite)
        ):
            raise HTTPException(409, "La hora limite de reserva ya paso")
        inmoviliza = not self._becado(persona, datos.fecha)
        sin_tiquete = False
        if inmoviliza:
            cuenta = self.repo.obtener_o_crear_cuenta(persona.id)
            if cuenta.saldo > 0:
                cuenta = self._mover(persona.id, "reserva", -1, datos.fecha.isoformat())
                cuenta.reservados += 1
            else:
                # Confirmar asistencia no depende de poder pagarla. El operador
                # verá el estado antes de decidir el ingreso físico.
                inmoviliza = False
                sin_tiquete = True
        return self.repo.guardar(
            ReservaComedor(
                persona_id=persona.id,
                fecha=datos.fecha,
                estado="reservada",
                tiquete_inmovilizado=inmoviliza,
                sin_tiquete=sin_tiquete,
            )
        )

    def cancelar(self, datos, identidad):
        persona = (
            identidad["persona"]
            if identidad["tipo"] == "portal" and not datos.cedula
            else self._persona(cedula=datos.cedula)
        )
        if identidad["tipo"] == "portal" and identidad["persona"].cedula != persona.cedula:
            raise HTTPException(403, "No puede cancelar una reserva ajena")
        reserva = self.repo.reserva_fecha(persona.id, datos.fecha, True)
        if not reserva:
            raise HTTPException(404, "Reserva no encontrada")
        if reserva.tiquete_inmovilizado:
            cuenta = self._mover(reserva.persona_id, "liberacion", 1, str(reserva.id))
            cuenta.reservados -= 1
        reserva.estado = "cancelada"

    def autorizar(self, datos, operador_id):
        persona = self._persona(cedula=datos.cedula)
        if persona.tipo != "estudiante":
            raise HTTPException(409, "Solo estudiantes sin reserva")
        try:
            return self.repo.guardar(
                AutorizacionComedor(
                    persona_id=persona.id,
                    fecha=datos.fecha,
                    decision=datos.decision,
                    motivo=datos.motivo,
                    operador_id=operador_id,
                )
            )
        except IntegrityError as exc:
            raise HTTPException(409, "Ya existe decision") from exc

    def ingresar(self, datos, operador_id):
        persona = self._persona(cedula=datos.cedula)
        if self.repo.ingreso_fecha(persona.id, datos.fecha):
            raise HTTPException(409, "Ingreso duplicado")
        reserva = self.repo.reserva_fecha(persona.id, datos.fecha, True)
        autorizacion = None
        modalidad = "reserva" if reserva else "directo_profesor"
        if not reserva and persona.tipo == "estudiante":
            autorizacion = self.repo.autorizacion_aprobada(persona.id, datos.fecha)
            if not autorizacion:
                raise HTTPException(409, "Estudiante sin reserva requiere autorizacion")
            modalidad = "autorizacion"
        if reserva and reserva.sin_tiquete:
            raise HTTPException(409, "Reserva confirmada, pero no tiene tiquetes disponibles")
        consume = not self._becado(persona, datos.fecha)
        matricula = self._matricula(persona, datos.fecha)
        marca_transporte = bool(
            matricula and self.repo.tiene_marca_transporte(matricula.id, datos.fecha)
        )
        advertencia = (
            "Sin marca de transporte"
            if persona.tipo == "estudiante" and not marca_transporte
            else None
        )
        if reserva and reserva.tiquete_inmovilizado:
            cuenta = self.repo.obtener_o_crear_cuenta(persona.id)
            cuenta.reservados -= 1
            self.repo.movimiento(persona.id, "consumo", 0, cuenta.saldo, str(reserva.id))
            reserva.estado = "consumida"
        elif consume:
            self._mover(persona.id, "consumo", -1, datos.fecha.isoformat())
        return self.repo.guardar(
            IngresoComedor(
                persona_id=persona.id,
                fecha=datos.fecha,
                reserva_id=reserva.id if reserva else None,
                autorizacion_id=autorizacion.id if autorizacion else None,
                modalidad=modalidad,
                consumio_tiquete=consume,
                marca_transporte_existente=marca_transporte,
                advertencia=advertencia,
                operador_id=operador_id,
            )
        )
    def marcar_transporte(self, datos, operador_id):
        matricula = self._matricula(self._persona(cedula=datos.cedula), datos.fecha)
        if not matricula:
            raise HTTPException(404, "Matricula no encontrada")
        asignacion = self.rutas.ruta_vigente(matricula.id, datos.fecha)
        if not asignacion:
            raise HTTPException(409, "No existe ruta vigente")
        try:
            return self.repo.guardar(
                MarcaTransporte(
                    matricula_id=matricula.id,
                    ruta_id=asignacion.ruta_id,
                    fecha=datos.fecha,
                    operador_id=operador_id,
                )
            )
        except IntegrityError as exc:
            raise HTTPException(409, "Marca de transporte duplicada") from exc
