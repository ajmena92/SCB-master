"""Casos de uso operativos; toda persistencia se delega al repositorio."""

from datetime import date, datetime, time
from time import perf_counter

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.modelos.operacion import (
    AutorizacionComedor,
    IngresoComedor,
    MarcaTransporte,
    ReservaComedor,
    Tarifa,
    VentaTiquete,
)


class ServicioOperacion:
    def __init__(self, repo):
        self.repo = repo

    def listar_tarifas(self):
        return self.repo.listar_tarifas()

    def crear_tarifa(self, datos):
        if self.repo.tarifa_solapada(datos):
            raise HTTPException(409, "La vigencia de tarifa se superpone")
        return self.repo.guardar(Tarifa(**datos.model_dump()))

    def _persona(self, persona_id=None, codigo=None):
        persona = self.repo.persona_codigo(codigo) if codigo else self.repo.persona(persona_id)
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        return persona

    def _matricula(self, persona, fecha):
        return self.repo.matricula_fecha(persona.id, fecha)

    def _becado(self, persona, fecha):
        matricula = self._matricula(persona, fecha)
        return persona.tipo == "estudiante" and bool(matricula and matricula.becado)

    def _mover(self, persona_id, tipo, cantidad, referencia=None):
        cuenta = self.repo.obtener_o_crear_cuenta(persona_id)
        if cuenta.saldo + cantidad < 0:
            raise HTTPException(409, "Saldo de tiquetes insuficiente")
        cuenta.saldo += cantidad
        self.repo.movimiento(persona_id, tipo, cantidad, cuenta.saldo, referencia)
        return cuenta

    def vender(self, datos, operador_id):
        persona = self._persona(codigo=datos.codigo)
        tarifa = self.repo.tarifa_vigente(persona.tipo, date.today())
        if not tarifa:
            raise HTTPException(409, "No existe tarifa vigente")
        venta = VentaTiquete(
            persona_id=persona.id,
            tarifa_id=tarifa.id,
            cantidad=datos.cantidad,
            tarifa_aplicada=tarifa.monto,
            total=tarifa.monto * datos.cantidad,
            medio_pago=datos.medio_pago,
            operador_id=operador_id,
        )
        self.repo.guardar(venta)
        self._mover(persona.id, "venta", datos.cantidad, str(venta.id))
        return venta

    def reservar(self, datos, identidad):
        persona = (
            identidad["persona"]
            if identidad["tipo"] == "portal" and not datos.codigo
            else self._persona(codigo=datos.codigo)
        )
        if identidad["tipo"] == "portal" and identidad["persona"].codigo != persona.codigo:
            raise HTTPException(403, "No puede reservar para otra persona")
        if self.repo.reserva_fecha(persona.id, datos.fecha):
            raise HTTPException(409, "Ya existe una reserva")
        matricula = self._matricula(persona, datos.fecha)
        if persona.tipo == "estudiante" and not matricula:
            raise HTTPException(409, "Sin matricula activa")
        horario = self.repo.horario(matricula.turno) if matricula else None
        if (
            horario
            and datos.fecha == datetime.now().date()
            and datetime.now().time() > time.fromisoformat(horario.hora_limite)
        ):
            raise HTTPException(409, "La hora limite de reserva ya paso")
        inmoviliza = not self._becado(persona, datos.fecha)
        if inmoviliza:
            cuenta = self._mover(persona.id, "reserva", -1, datos.fecha.isoformat())
            cuenta.reservados += 1
        return self.repo.guardar(
            ReservaComedor(
                persona_id=persona.id,
                fecha=datos.fecha,
                estado="reservada",
                tiquete_inmovilizado=inmoviliza,
            )
        )

    def cancelar(self, datos, identidad):
        persona = (
            identidad["persona"]
            if identidad["tipo"] == "portal" and not datos.codigo
            else self._persona(codigo=datos.codigo)
        )
        if identidad["tipo"] == "portal" and identidad["persona"].codigo != persona.codigo:
            raise HTTPException(403, "No puede cancelar una reserva ajena")
        reserva = self.repo.reserva_fecha(persona.id, datos.fecha, True)
        if not reserva:
            raise HTTPException(404, "Reserva no encontrada")
        if reserva.tiquete_inmovilizado:
            cuenta = self._mover(reserva.persona_id, "liberacion", 1, str(reserva.id))
            cuenta.reservados -= 1
        reserva.estado = "cancelada"

    def autorizar(self, datos, operador_id):
        persona = self._persona(codigo=datos.codigo)
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
        persona = self._persona(codigo=datos.codigo)
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

    def capturar_ingreso(self, datos, operador_id):
        inicio = perf_counter()
        persona = self.repo.persona_codigo(datos.codigo)
        try:
            ingreso = self.ingresar(datos, operador_id)
        except HTTPException as exc:
            detalle = str(exc.detail)
            resultado = "rechazado"
            if "no encontrada" in detalle.lower():
                resultado = "no_encontrado"
            elif "duplicado" in detalle.lower():
                resultado = "duplicado"
            elif "reserva" in detalle.lower():
                resultado = "sin_reserva"
            elif "saldo" in detalle.lower() or "tiquete" in detalle.lower():
                resultado = "sin_tiquete"
            self.repo.registrar_evento(
                fecha=datos.fecha,
                codigo=datos.codigo,
                resultado=resultado,
                operador_id=operador_id,
                persona_id=persona.id if persona else None,
                motivo=detalle,
                duracion_ms=round((perf_counter() - inicio) * 1000),
            )
            return {
                "estado": "rechazada",
                "resultado": resultado,
                "mensaje": detalle,
                "persona": self._persona_salida(persona),
            }, exc.status_code

        cuenta = self.repo.cuenta(persona.id)
        advertencia = ingreso.advertencia
        self.repo.registrar_evento(
            fecha=datos.fecha,
            codigo=datos.codigo,
            resultado="aceptado",
            operador_id=operador_id,
            persona_id=persona.id,
            motivo=advertencia,
            advertencia=bool(advertencia),
            duracion_ms=round((perf_counter() - inicio) * 1000),
        )
        return {
            "id": ingreso.id,
            "estado": "aceptada",
            "resultado": "aceptado",
            "mensaje": advertencia or "Ingreso registrado correctamente",
            "modalidad": ingreso.modalidad,
            "consumioTiquete": ingreso.consumio_tiquete,
            "marcaTransporteExistente": ingreso.marca_transporte_existente,
            "advertencia": advertencia,
            "saldo": cuenta.saldo if cuenta else None,
            "persona": self._persona_salida(persona),
        }, 201

    @staticmethod
    def _persona_salida(persona):
        if not persona:
            return None
        return {
            "id": persona.id,
            "codigo": persona.codigo,
            "cedula": persona.cedula,
            "nombres": persona.nombres,
            "tipo": persona.tipo,
            "activo": persona.activo,
        }

    def estado_captura(self, fecha):
        total, meta, duplicados, errores, eventos = self.repo.estado_captura(fecha)
        return {
            "fecha": fecha,
            "ingresos": total,
            "meta": meta,
            "porcentaje": round(total * 100 / meta, 1) if meta else 0,
            "duplicados": duplicados,
            "errores": errores,
            "recientes": [
                {
                    "id": evento.id,
                    "hora": evento.fecha_evento,
                    "codigo": evento.codigo_capturado,
                    "nombre": persona.nombres if persona else "Código no reconocido",
                    "resultado": evento.resultado,
                    "motivo": evento.motivo,
                }
                for evento, persona in eventos
            ],
        }

    def marcar_transporte(self, datos, operador_id):
        matricula = self._matricula(self._persona(codigo=datos.codigo), datos.fecha)
        if not matricula:
            raise HTTPException(404, "Matricula no encontrada")
        asignacion = self.repo.ruta_vigente(matricula.id, datos.fecha)
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
