"""Casos de uso de tarifas y venta de tiquetes."""

from datetime import date

from fastapi import HTTPException

from aplicacion.modelos.operacion import Tarifa, VentaTiquete
from aplicacion.servicios_operacion_base import ServicioOperacionBase


class ServicioOperacionVentas(ServicioOperacionBase):
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
        tarifa.tipo_persona, tarifa.monto, tarifa.fecha_inicio, tarifa.fecha_fin = (
            datos.tipo_persona,
            datos.monto,
            datos.fecha_inicio,
            datos.fecha_fin,
        )
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
                "saldoTiquetes": (
                    self.repo.cuenta(persona.id).saldo if self.repo.cuenta(persona.id) else 0
                ),
            }
            for persona in personas
        ]

    def foto_persona_venta(self, persona_id):
        return self.personas.foto_persona(persona_id)

    def foto_persona_comedor(self, persona_id):
        """Entrega únicamente la fotografía para validar una lectura de comedor."""
        return self.personas.foto_persona(persona_id)

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
        return {
            "nombre_colegio": "Colegio Técnico Profesional de Platanares",
            "subtitulo_reportes": "Comedor estudiantil",
        }

    def actualizar_configuracion_institucional(self, datos):
        return self.repo.guardar_configuracion_institucional(datos)

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
