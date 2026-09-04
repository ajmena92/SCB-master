"""Casos de uso de plantillas, calendario y sustituciones de menú."""

from datetime import timedelta

from fastapi import HTTPException

from aplicacion.modelos.menu import CalendarioMenu, ComponenteMenu, ComponenteSustitucionMenu


class CasosCatalogosMenu:
    def configuracion_ciclo_menu(self):
        return self.repo.configuracion_ciclo_menu()

    def configurar_ciclo_menu(self, datos):
        return self.repo.guardar_configuracion_ciclo_menu(datos.inicio_ciclo_menu)

    def listar_plantillas(self):
        return [{"id": plantilla.id, "semana": plantilla.semana, "dia": plantilla.dia, "titulo": plantilla.titulo, "observaciones": plantilla.observaciones, "activo": plantilla.activo, "componentes": [{"nombre": componente.nombre, "tipo": componente.tipo, "orden": componente.orden} for componente in componentes]} for plantilla, componentes in self.repo.listar_plantillas()]

    def crear_plantilla(self, datos):
        return self.guardar_plantilla(datos)

    def actualizar_plantilla(self, semana, dia, datos):
        if semana != datos.semana or dia != datos.dia:
            raise HTTPException(422, "La posición de la plantilla no coincide con el cuerpo")
        return self.guardar_plantilla(datos)

    def guardar_plantilla(self, datos):
        ordenes = [componente.orden for componente in datos.componentes]
        if len(ordenes) != len(set(ordenes)):
            raise HTTPException(422, "Los componentes no pueden repetir orden")
        componentes = [ComponenteMenu(**componente.model_dump()) for componente in datos.componentes]
        plantilla = self.repo.reemplazar_plantilla(datos, componentes)
        return {"id": plantilla.id, **datos.model_dump()}

    def listar_calendario(self, desde, hasta):
        excepciones = {registro.fecha: registro for registro in self.repo.listar_calendario(desde, hasta)}
        sustituciones = {registro.fecha: registro for registro in self.repo.sustituciones_rango(desde, hasta)}
        componentes_plantilla = {}
        componentes_sustitucion = {}
        dias = []
        fecha = desde
        while fecha <= hasta:
            es_lectivo = fecha.isoweekday() <= 5
            excepcion = excepciones.get(fecha) if es_lectivo else None
            habilitado = (excepcion.habilitado if excepcion else True) if es_lectivo else False
            origen, titulo, componentes = ("cerrado" if es_lectivo and not habilitado else "no_lectivo"), None, []
            posicion = ((fecha.day - 1) // 7 + 1, fecha.isoweekday()) if es_lectivo else None
            if habilitado and fecha in sustituciones:
                origen, titulo = "sustitucion", sustituciones[fecha].titulo
                if fecha not in componentes_sustitucion:
                    _, componentes_sustitucion[fecha] = self.repo.sustitucion_componentes(fecha)
                componentes = [componente.nombre for componente in componentes_sustitucion[fecha]]
            elif habilitado:
                if posicion is not None and posicion not in componentes_plantilla:
                    componentes_plantilla[posicion] = self.repo.plantilla_componentes(*posicion)
                plantilla, componentes_menu = componentes_plantilla.get(posicion, (None, [])) if posicion else (None, [])
                if plantilla and plantilla.activo:
                    origen, titulo = "plantilla", plantilla.titulo
                    componentes = [componente.nombre for componente in componentes_menu]
                else:
                    origen = "sin_menu"
            dias.append({"fecha": fecha, "habilitado": habilitado, "esLectivo": es_lectivo, "semana": posicion[0] if es_lectivo and habilitado and posicion else None, "dia": fecha.isoweekday(), "diaMes": fecha.day, "motivo": excepcion.motivo if excepcion else None, "origen": origen, "titulo": titulo, "componentes": componentes, "publicado": False, "tieneSustitucion": es_lectivo and fecha in sustituciones})
            fecha += timedelta(days=1)
        return dias

    def listar_sustituciones(self):
        return [{"id": sustitucion.id, "fecha": sustitucion.fecha, "titulo": sustitucion.titulo, "observaciones": sustitucion.observaciones, "componentes": [{"nombre": componente.nombre, "tipo": componente.tipo, "orden": componente.orden} for componente in componentes]} for sustitucion, componentes in self.repo.listar_sustituciones()]

    def actualizar_calendario(self, datos):
        registro = self.repo.calendario_fecha(datos.fecha)
        if registro is None:
            registro = CalendarioMenu(fecha=datos.fecha, habilitado=datos.habilitado, motivo=datos.motivo)
        else:
            registro.habilitado, registro.motivo = datos.habilitado, datos.motivo
        return self.repo.guardar(registro)

    def guardar_sustitucion(self, datos):
        ordenes = [componente.orden for componente in datos.componentes]
        if len(ordenes) != len(set(ordenes)):
            raise HTTPException(422, "Los componentes no pueden repetir orden")
        componentes = [ComponenteSustitucionMenu(**componente.model_dump()) for componente in datos.componentes]
        sustitucion = self.repo.reemplazar_sustitucion(datos, componentes)
        return {"id": sustitucion.id, **datos.model_dump()}
