"""Casos de uso operativos; toda persistencia se delega al repositorio."""

from datetime import date, datetime, time
from hashlib import sha256
from time import perf_counter

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.codigo_qr_carnet import CodigoQrCarnet, ErrorCodigoQrCarnet, PREFIJO_QR
from aplicacion.modelos.operacion import (
    AutorizacionComedor,
    IngresoComedor,
    MarcaTransporte,
    ReservaComedor,
    Tarifa,
    VentaTiquete,
)


class ServicioOperacion:
    def __init__(self, repo, clave_qr_carnet: str = ""):
        self.repo = repo
        self.clave_qr_carnet = clave_qr_carnet

    def listar_tarifas(self):
        return self.repo.listar_tarifas()

    def crear_tarifa(self, datos):
        if self.repo.tarifa_solapada(datos):
            raise HTTPException(409, "La vigencia de tarifa se superpone")
        return self.repo.guardar(Tarifa(**datos.model_dump()))

    def actualizar_tarifa(self, tarifa_id, datos):
        tarifa = self.repo.tarifa(tarifa_id)
        if not tarifa:
            raise HTTPException(404, "Tarifa no encontrada")
        if self.repo.tarifa_solapada(datos, excluir_id=tarifa_id):
            raise HTTPException(409, "La vigencia de tarifa se superpone")
        tarifa.tipo_persona, tarifa.monto, tarifa.fecha_inicio, tarifa.fecha_fin = datos.tipo_persona, datos.monto, datos.fecha_inicio, datos.fecha_fin
        self.repo.sesion.flush()
        return tarifa

    def buscar_personas_venta(self, termino):
        personas = self.repo.buscar_personas_venta(termino)
        hoy = date.today()
        return [
            {
                "id": persona.id,
                "cedula": persona.cedula,
                "nombres": persona.nombres,
                "tipo": persona.tipo,
                "becado": self._becado(persona, hoy),
                "saldoTiquetes": (self.repo.cuenta(persona.id).saldo if self.repo.cuenta(persona.id) else 0),
            }
            for persona in personas
        ]

    def foto_persona_venta(self, persona_id):
        return self.repo.foto_persona(persona_id)

    def foto_persona_comedor(self, persona_id):
        """Entrega únicamente la fotografía para validar una lectura de comedor."""
        return self.repo.foto_persona(persona_id)

    def listar_horarios_reserva(self):
        return self.repo.listar_horarios_reserva()

    def actualizar_horario_reserva(self, datos):
        horario = self.repo.actualizar_horario_reserva(datos.turno, datos.hora_limite)
        if not horario:
            raise HTTPException(404, "Horario de reserva no encontrado")
        return horario

    def configuracion_institucional(self):
        configuracion = self.repo.configuracion_institucional()
        if configuracion:
            return configuracion
        return {"nombre_colegio": "Colegio Técnico Profesional de Platanares", "subtitulo_reportes": "Comedor estudiantil"}

    def actualizar_configuracion_institucional(self, datos):
        return self.repo.guardar_configuracion_institucional(datos)

    def _persona(self, persona_id=None, cedula=None):
        persona = self.repo.persona_cedula(cedula) if cedula else self.repo.persona(persona_id)
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        return persona

    def _persona_por_carnet(self, codigo: str):
        if not codigo.startswith(PREFIJO_QR):
            return self.repo.persona_cedula(codigo)
        if not self.clave_qr_carnet:
            return None
        try:
            persona_id = CodigoQrCarnet(self.clave_qr_carnet).resolver(codigo, hoy=date.today())
        except ErrorCodigoQrCarnet:
            return None
        persona = self.repo.persona(persona_id)
        return persona if persona and persona.activo else None

    @staticmethod
    def _codigo_auditoria(codigo: str) -> str:
        if codigo.startswith(PREFIJO_QR):
            return f"{PREFIJO_QR}{sha256(codigo.encode('utf-8')).hexdigest()[:32]}"
        return codigo

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
        persona = self._persona(cedula=datos.cedula)
        if self._becado(persona, date.today()):
            raise HTTPException(409, "Las personas beneficiarias de comedor no compran tiquetes")
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
        if (
            horario
            and datos.fecha == datetime.now().date()
            and datetime.now().time() > time.fromisoformat(horario.hora_limite)
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

    def capturar_ingreso(self, datos, operador_id):
        inicio = perf_counter()
        codigo_capturado = datos.cedula
        codigo_auditoria = self._codigo_auditoria(codigo_capturado)
        persona = self._persona_por_carnet(codigo_capturado)
        datos_ingreso = datos.model_copy(update={"cedula": persona.cedula}) if persona else datos
        try:
            ingreso = self.ingresar(datos_ingreso, operador_id)
        except HTTPException as exc:
            detalle = str(exc.detail)
            resultado = "rechazado"
            if "no encontrada" in detalle.lower():
                resultado = "no_encontrado"
            elif "duplicado" in detalle.lower():
                resultado = "duplicado"
            elif "saldo" in detalle.lower() or "tiquete" in detalle.lower():
                resultado = "sin_tiquete"
            elif "reserva" in detalle.lower():
                resultado = "sin_reserva"
            self.repo.registrar_evento(
                fecha=datos.fecha,
                codigo=codigo_auditoria,
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
            codigo=codigo_auditoria,
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
        matricula = self._matricula(self._persona(cedula=datos.cedula), datos.fecha)
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
