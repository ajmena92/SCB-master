"""Casos de uso de datos maestros y menu."""

import secrets
from datetime import date

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.esquemas import MatriculaBeneficiosEntrada
from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    Ruta,
)
from aplicacion.modelos.menu import CalendarioMenu, ComponenteMenu, ComponenteSustitucionMenu
from aplicacion.seguridad import hash_secreto


class ServicioCatalogos:
    def __init__(self, repo):
        self.repo = repo

    def listar_personas(
        self, buscar="", estado="activos", tipo=None, pagina=1, tamano=50,
        ordenar_por="nombres", direccion="asc",
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

    def listar_rutas(self):
        return [
            {
                "idRuta": ruta.id,
                "codigo": ruta.codigo,
                "descripcion": ruta.descripcion,
                "colorCarnetHex": ruta.color_hex,
                "activo": ruta.activo,
                "estudiantesAsignados": asignados,
            }
            for ruta, asignados in self.repo.listar_rutas()
        ]

    def listar_rutas_activas(self):
        return [
            self._ruta_salida(ruta, asignados)
            for ruta, asignados in self.repo.listar_rutas_activas()
        ]

    def crear_persona(self, datos):
        raise HTTPException(409, "Las personas solo se crean mediante la importación anual del padrón")

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

    def actualizar_persona(self, persona_id, datos):
        persona = self.repo.persona(persona_id)
        if not persona:
            raise HTTPException(404, "Persona no encontrada")
        if persona.tipo == "estudiante":
            raise HTTPException(409, "Los datos del estudiante solo se actualizan mediante el padrón anual")
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

    def actualizar_beneficio_comedor(self, matricula_id, datos):
        matricula = self._validar_matricula_beneficios(matricula_id)
        return self.actualizar_beneficios_matricula(
            matricula_id,
            MatriculaBeneficiosEntrada(
                becado=datos.becado,
                ruta_id=self.repo.ruta_activa_matricula(matricula.id),
            ),
        )

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
            matricula_id,
            MatriculaBeneficiosEntrada(becado=matricula.becado, ruta_id=ruta_id),
        )

    def reiniciar_pin(self, persona_id, cuenta_id, tipo="reinicio_individual"):
        persona = self.repo.persona(persona_id)
        if not persona or not persona.activo:
            raise HTTPException(404, "Persona activa no encontrada")
        pin = f"{secrets.randbelow(1_000_000):06d}"
        self.repo.reiniciar_pin(persona, hash_secreto(pin), cuenta_id, tipo)
        return {"personaId": persona.id, "cedula": persona.cedula, "nombre": persona.nombres, "pinTemporal": pin}

    def reiniciar_pines_seccion(self, datos, cuenta_id):
        estudiantes = self.repo.estudiantes_seccion(datos.anio_lectivo_id, datos.seccion.strip())
        if not estudiantes:
            raise HTTPException(404, "No hay estudiantes activos para la seccion indicada")
        return [self.reiniciar_pin(estudiante.id, cuenta_id, "reinicio_masivo") for estudiante in estudiantes]

    def crear_anio(self, datos):
        return self.repo.guardar_anio(AnioLectivo(**datos.model_dump()))

    def activar_anio(self, anio_id):
        registro = self.repo.activar_anio(anio_id)
        if not registro:
            raise HTTPException(404, "Año lectivo no encontrado")
        return registro

    def configuracion_ciclo_menu(self):
        return self.repo.configuracion_ciclo_menu()

    def configurar_ciclo_menu(self, datos):
        return self.repo.guardar_configuracion_ciclo_menu(datos.inicio_ciclo_menu)

    def crear_matricula(self, datos):
        raise HTTPException(409, "Las matrículas solo se crean mediante la importación anual del padrón")

    def crear_ruta(self, datos):
        codigo = datos.codigo.strip()
        descripcion = " ".join(datos.descripcion.split())
        ruta = self.repo.guardar(
            Ruta(
                nombre=f"{codigo}-{descripcion}",
                codigo=codigo,
                descripcion=descripcion,
                color_hex=datos.color_hex.upper(),
                activo=datos.activa,
            )
        )
        return self._ruta_salida(ruta, 0)

    def actualizar_ruta(self, ruta_id, datos):
        ruta = self.repo.ruta(ruta_id)
        if not ruta:
            raise HTTPException(404, "Ruta no encontrada")
        if ruta.codigo == "0":
            raise HTTPException(409, "La ruta 0 esta protegida")
        ruta.codigo = datos.codigo.strip()
        ruta.descripcion = " ".join(datos.descripcion.split())
        ruta.nombre = f"{ruta.codigo}-{ruta.descripcion}"
        ruta.color_hex = datos.color_hex.upper()
        ruta.activo = datos.activa
        self.repo.guardar(ruta)
        return self._ruta_salida(ruta, self.repo.contar_asignados(ruta.id))

    @staticmethod
    def _ruta_salida(ruta, asignados):
        return {
            "idRuta": ruta.id,
            "codigo": ruta.codigo,
            "descripcion": ruta.descripcion,
            "colorCarnetHex": ruta.color_hex,
            "activo": ruta.activo,
            "estudiantesAsignados": asignados,
        }

    def asignar_ruta(self, ruta_id, datos):
        if not self.repo.ruta(ruta_id) or not self.repo.matricula(datos.matricula_id):
            raise HTTPException(404, "Ruta o matricula no encontrada")
        if self.repo.asignacion_solapada(datos):
            raise HTTPException(409, "La vigencia de ruta se superpone")
        return self.repo.guardar(AsignacionRuta(ruta_id=ruta_id, **datos.model_dump()))

    def listar_plantillas(self):
        return [
            {
                "id": p.id,
                "semana": p.semana,
                "dia": p.dia,
                "titulo": p.titulo,
                "observaciones": p.observaciones,
                "activo": p.activo,
                "componentes": [
                    {"nombre": c.nombre, "tipo": c.tipo, "orden": c.orden} for c in cs
                ],
            }
            for p, cs in self.repo.listar_plantillas()
        ]

    def crear_plantilla(self, datos):
        return self.guardar_plantilla(datos)

    def actualizar_plantilla(self, semana, dia, datos):
        if semana != datos.semana or dia != datos.dia:
            raise HTTPException(422, "La posición de la plantilla no coincide con el cuerpo")
        return self.guardar_plantilla(datos)

    def guardar_plantilla(self, datos):
        ordenes = [c.orden for c in datos.componentes]
        if len(ordenes) != len(set(ordenes)):
            raise HTTPException(422, "Los componentes no pueden repetir orden")
        cs = [ComponenteMenu(**c.model_dump()) for c in datos.componentes]
        p = self.repo.reemplazar_plantilla(datos, cs)
        return {"id": p.id, **datos.model_dump()}

    def listar_calendario(self, desde, hasta):
        from datetime import timedelta

        excepciones = {x.fecha: x for x in self.repo.listar_calendario(desde, hasta)}
        sustituciones = {x.fecha: x for x in self.repo.sustituciones_rango(desde, hasta)}
        componentes_plantilla = {}
        componentes_sustitucion = {}
        dias = []
        fecha = desde
        while fecha <= hasta:
            es_lectivo = fecha.isoweekday() <= 5
            excepcion = excepciones.get(fecha) if es_lectivo else None
            habilitado = (excepcion.habilitado if excepcion else True) if es_lectivo else False
            origen = "cerrado" if es_lectivo and not habilitado else "no_lectivo"
            titulo = None
            componentes = []
            posicion = None
            if es_lectivo:
                semana = (fecha.day - 1) // 7 + 1
                posicion = (semana, fecha.isoweekday())
            if habilitado and fecha in sustituciones:
                origen, titulo = "sustitucion", sustituciones[fecha].titulo
                if fecha not in componentes_sustitucion:
                    _, componentes_sustitucion[fecha] = self.repo.sustitucion_componentes(fecha)
                componentes = [c.nombre for c in componentes_sustitucion[fecha]]
            elif habilitado:
                if posicion is not None and posicion not in componentes_plantilla:
                    componentes_plantilla[posicion] = self.repo.plantilla_componentes(*posicion)
                plantilla, componentes_menu = componentes_plantilla.get(posicion, (None, []))
                if plantilla and plantilla.activo:
                    origen, titulo = "plantilla", plantilla.titulo
                    componentes = [c.nombre for c in componentes_menu]
                else:
                    origen = "sin_menu"
            dias.append({
                "fecha": fecha, "habilitado": habilitado, "esLectivo": es_lectivo,
                "semana": (posicion[0] if es_lectivo and habilitado and posicion else None),
                "dia": fecha.isoweekday(), "diaMes": fecha.day,
                "motivo": excepcion.motivo if excepcion else None,
                "origen": origen, "titulo": titulo,
                "componentes": componentes,
                "publicado": False,
                "tieneSustitucion": es_lectivo and fecha in sustituciones,
            })
            fecha += timedelta(days=1)
        return dias

    def listar_sustituciones(self):
        return [
            {
                "id": sustitucion.id,
                "fecha": sustitucion.fecha,
                "titulo": sustitucion.titulo,
                "observaciones": sustitucion.observaciones,
                "componentes": [
                    {"nombre": c.nombre, "tipo": c.tipo, "orden": c.orden}
                    for c in componentes
                ],
            }
            for sustitucion, componentes in self.repo.listar_sustituciones()
        ]

    def actualizar_calendario(self, datos):
        registro = self.repo.calendario_fecha(datos.fecha)
        if registro is None:
            registro = CalendarioMenu(
                fecha=datos.fecha, habilitado=datos.habilitado, motivo=datos.motivo
            )
        else:
            registro.habilitado = datos.habilitado
            registro.motivo = datos.motivo
        return self.repo.guardar(registro)

    def guardar_sustitucion(self, datos):
        ordenes = [c.orden for c in datos.componentes]
        if len(ordenes) != len(set(ordenes)):
            raise HTTPException(422, "Los componentes no pueden repetir orden")
        componentes = [ComponenteSustitucionMenu(**c.model_dump()) for c in datos.componentes]
        sustitucion = self.repo.reemplazar_sustitucion(datos, componentes)
        return {"id": sustitucion.id, **datos.model_dump()}
