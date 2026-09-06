"""Casos de uso de personas, beneficios, credenciales y años."""

import secrets
from typing import Any

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.esquemas import MatriculaBeneficiosEntrada
from aplicacion.modelos.maestros import AnioLectivo
from aplicacion.seguridad import hash_secreto


class CasosCatalogosPersonas:
    repo: Any

    def listar_personas(
        self,
        buscar="",
        estado="activos",
        tipo=None,
        pagina=1,
        tamano=50,
        ordenar_por="nombres",
        direccion="asc",
    ):
        return self.repo.listar_personas(
            buscar, estado, tipo, pagina, tamano, ordenar_por, direccion
        )

    def resumen_personas(self):
        return self.repo.resumen_personas()

    def obtener_persona(self, persona_id: int):
        persona = self.repo.obtener_persona_resumen(persona_id)
        if persona is None:
            raise HTTPException(404, "Persona no encontrada")
        return persona

    def obtener_persona_referencia_publica(self, referencia_publica: str):
        persona = self.repo.persona_referencia_publica(referencia_publica)
        if persona is None:
            raise HTTPException(404, "Persona no encontrada")
        return self.repo.obtener_persona_resumen(persona.id)

    def _persona_para_foto(self, persona_id):
        persona = self.repo.persona(persona_id)
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        return persona

    def obtener_foto_persona(self, persona_id):
        self._persona_para_foto(persona_id)
        return self.repo.foto_persona(persona_id)

    def guardar_foto_persona(self, persona_id, contenido, tipo_contenido):
        self._persona_para_foto(persona_id)
        self.repo.guardar_foto_persona(persona_id, contenido, tipo_contenido)

    def eliminar_foto_persona(self, persona_id):
        self._persona_para_foto(persona_id)
        self.repo.eliminar_foto_persona(persona_id)

    def crear_persona(self, datos):
        raise HTTPException(
            409, "Las personas solo se crean mediante la importación anual del padrón"
        )

    def actualizar_persona(self, persona_id, datos):
        persona = self.repo.persona(persona_id)
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        if persona.tipo == "estudiante":
            raise HTTPException(
                409, "Los datos del estudiante solo se actualizan mediante el padrón anual"
            )
        try:
            return self.repo.actualizar_persona(persona, datos)
        except IntegrityError as exc:
            raise HTTPException(409, "La cedula ya esta registrada") from exc

    def desactivar_persona(self, persona_id, cuenta_id):
        persona = self.repo.persona(persona_id)
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        if not persona.activo:
            raise HTTPException(409, "La persona ya esta inactiva")
        if self.repo.tiene_cuenta_administrativa(persona.id):
            raise HTTPException(409, "No se puede desactivar una persona con cuenta administrativa")
        self.repo.desactivar_persona(persona)
        return {"id": persona.id, "activo": False, "sesionesRevocadas": True}

    def _validar_matricula_beneficios(self, matricula_id):
        matricula = self.repo.matricula(matricula_id)
        if not matricula:
            raise HTTPException(404, "Matrícula no encontrada")
        persona = self.repo.persona(matricula.persona_id)
        if not persona or persona.tipo != "estudiante" or not persona.activo:
            raise HTTPException(409, "Los beneficios requieren un estudiante activo")
        if matricula.estado != "activo":
            raise HTTPException(409, "Los beneficios requieren una matrícula activa")
        anio_vigente = self.repo.anio_vigente()
        if not anio_vigente or matricula.anio_lectivo_id != anio_vigente.id:
            raise HTTPException(409, "Los beneficios solo se administran en el año lectivo vigente")
        return matricula

    def actualizar_beneficio_comedor(self, matricula_id, datos):
        matricula = self._validar_matricula_beneficios(matricula_id)
        return self.actualizar_beneficios_matricula(
            matricula_id,
            MatriculaBeneficiosEntrada(
                becado=datos.becado, ruta_id=self.repo.ruta_activa_matricula(matricula.id)
            ),
        )

    def actualizar_beneficios_matricula(self, matricula_id, datos):
        matricula = self._validar_matricula_beneficios(matricula_id)
        if datos.ruta_id is not None:
            ruta = self.repo.ruta(datos.ruta_id)
            if not ruta or not ruta.activo or ruta.codigo == "0000":
                raise HTTPException(409, "La ruta no es válida para asignación operativa")
        matricula.becado = datos.becado
        self.repo.cambiar_ruta_matricula(matricula, datos.ruta_id)
        self.repo.guardar(matricula)
        return {"matriculaId": matricula.id, "becado": matricula.becado, "rutaId": datos.ruta_id}

    def cambiar_ruta_matricula(self, matricula_id, ruta_id):
        matricula = self._validar_matricula_beneficios(matricula_id)
        return self.actualizar_beneficios_matricula(
            matricula_id, MatriculaBeneficiosEntrada(becado=matricula.becado, ruta_id=ruta_id)
        )

    def reiniciar_pin(self, persona_id, cuenta_id, tipo="reinicio_individual"):
        persona = self.repo.persona(persona_id)
        if not persona or not persona.activo:
            raise HTTPException(404, "Persona activa no encontrada")
        pin = f"{secrets.randbelow(1_000_000):06d}"
        self.repo.reiniciar_pin(persona, hash_secreto(pin), cuenta_id, tipo)
        return {
            "personaId": persona.id,
            "cedula": persona.cedula,
            "nombre": persona.nombres,
            "pinTemporal": pin,
        }

    def reiniciar_pines_seccion(self, datos, cuenta_id):
        estudiantes = self.repo.estudiantes_seccion(datos.anio_lectivo_id, datos.seccion.strip())
        if not estudiantes:
            raise HTTPException(404, "No hay estudiantes activos para la seccion indicada")
        return [
            self.reiniciar_pin(estudiante.id, cuenta_id, "reinicio_masivo")
            for estudiante in estudiantes
        ]

    def listar_anios(self):
        return self.repo.listar_anios()

    def listar_secciones_anio(self, anio_id: int):
        if not self.repo.anio(anio_id):
            raise HTTPException(404, "Año lectivo no encontrado")
        return {"elementos": self.repo.secciones_anio(anio_id)}

    def resumen_pines_seccion(self, anio_id: int, seccion: str):
        if not self.repo.anio(anio_id):
            raise HTTPException(404, "Año lectivo no encontrado")
        return {"estudiantesActivos": len(self.repo.estudiantes_seccion(anio_id, seccion.strip()))}

    def listar_matriculas(self, anio_id=None):
        return self.repo.listar_matriculas(anio_id)

    def crear_anio(self, datos):
        return self.repo.guardar_anio(AnioLectivo(**datos.model_dump()))

    def activar_anio(self, anio_id):
        registro = self.repo.activar_anio(anio_id)
        if not registro:
            raise HTTPException(404, "Año lectivo no encontrado")
        return registro

    def crear_matricula(self, datos):
        raise HTTPException(
            409, "Las matrículas solo se crean mediante la importación anual del padrón"
        )
