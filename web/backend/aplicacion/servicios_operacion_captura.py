"""Casos de uso de captura y auditoría de ingresos."""

from datetime import date
from hashlib import sha256
from time import perf_counter

from fastapi import HTTPException

from aplicacion.codigo_qr_carnet import PREFIJO_QR, CodigoQrCarnet, ErrorCodigoQrCarnet


class ServicioOperacionCaptura:
    def __init__(self, repo, comedor, clave_qr_carnet: str = ""):
        self.repo = repo
        self.comedor = comedor
        self.personas = comedor.personas
        self.clave_qr_carnet = clave_qr_carnet

    def _persona_por_carnet(self, codigo: str):
        if not codigo.startswith(PREFIJO_QR):
            return self.personas.persona_cedula(codigo)
        if not self.clave_qr_carnet:
            return None
        try:
            persona_id = CodigoQrCarnet(self.clave_qr_carnet).resolver(codigo, hoy=date.today())
        except ErrorCodigoQrCarnet:
            return None
        persona = self.personas.persona(persona_id)
        return persona if persona and persona.activo else None

    @staticmethod
    def _codigo_auditoria(codigo: str) -> str:
        if codigo.startswith(PREFIJO_QR):
            return f"{PREFIJO_QR}{sha256(codigo.encode('utf-8')).hexdigest()[:32]}"
        return codigo

    def capturar_ingreso(self, datos, operador_id):
        inicio = perf_counter()
        codigo_capturado = datos.cedula
        codigo_auditoria = self._codigo_auditoria(codigo_capturado)
        persona = self._persona_por_carnet(codigo_capturado)
        datos_ingreso = datos.model_copy(update={"cedula": persona.cedula}) if persona else datos
        try:
            ingreso = self.comedor.ingresar(datos_ingreso, operador_id)
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
