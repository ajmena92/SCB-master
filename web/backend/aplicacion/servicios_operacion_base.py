"""Reglas compartidas de operación; no expone una fachada de casos de uso."""

from fastapi import HTTPException


class ServicioOperacionBase:
    def __init__(self, repositorio_operacion, repositorio_personas):
        self.repo = repositorio_operacion
        self.personas = repositorio_personas

    def _persona(self, persona_id=None, cedula=None):
        persona = (
            self.personas.persona_cedula(cedula) if cedula else self.personas.persona(persona_id)
        )
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        return persona

    def _matricula(self, persona, fecha):
        return self.personas.matricula_fecha(persona.id, fecha)

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
