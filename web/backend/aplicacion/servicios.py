"""Fachada pública de casos de uso operativos."""

from datetime import date
from hashlib import sha256

from fastapi import HTTPException

from aplicacion.codigo_qr_carnet import PREFIJO_QR, CodigoQrCarnet, ErrorCodigoQrCarnet
from aplicacion.servicios_operacion_captura import ServicioOperacionCaptura
from aplicacion.servicios_operacion_comedor import ServicioOperacionComedor
from aplicacion.servicios_operacion_ventas import ServicioOperacionVentas


class ServicioOperacion:
    def __init__(self, repo, clave_qr_carnet: str = ""):
        self.repo = repo
        self.clave_qr_carnet = clave_qr_carnet
        self.ventas = ServicioOperacionVentas(repo, repo)
        self.comedor = ServicioOperacionComedor(repo, repo, repo)
        self.captura = ServicioOperacionCaptura(repo, self.comedor, clave_qr_carnet)

    def __getattr__(self, nombre):
        """Expone temporalmente el contrato público mediante delegación explícita."""
        for componente in (self.ventas, self.comedor, self.captura):
            try:
                return getattr(componente, nombre)
            except AttributeError:
                continue
        raise AttributeError(nombre)

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
