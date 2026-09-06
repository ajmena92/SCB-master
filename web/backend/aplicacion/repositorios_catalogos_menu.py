"""Persistencia de configuración, plantillas y publicaciones de menú."""

from sqlalchemy import delete, select

from aplicacion.modelos.menu import (
    CalendarioMenu,
    ComponenteMenu,
    ComponentePublicado,
    ComponenteSustitucionMenu,
    ConfiguracionCicloMenu,
    PlantillaMenu,
    PublicacionMenu,
    SustitucionMenu,
)


class RepositorioCatalogosMenu:
    def __init__(self, sesion):
        self.sesion = sesion

    def configuracion_ciclo_menu(self):
        return self.sesion.get(ConfiguracionCicloMenu, 1)

    def guardar_configuracion_ciclo_menu(self, inicio_ciclo_menu):
        registro = self.configuracion_ciclo_menu()
        if registro is None:
            registro = ConfiguracionCicloMenu(id=1, inicio_ciclo_menu=inicio_ciclo_menu)
            self.sesion.add(registro)
        else:
            registro.inicio_ciclo_menu = inicio_ciclo_menu
        self.sesion.flush()
        return registro

    def listar_plantillas(self):
        salida = []
        for plantilla in self.sesion.scalars(
            select(PlantillaMenu).order_by(PlantillaMenu.semana, PlantillaMenu.dia)
        ):
            componentes = self.sesion.scalars(
                select(ComponenteMenu)
                .where(ComponenteMenu.plantilla_id == plantilla.id)
                .order_by(ComponenteMenu.orden)
            ).all()
            salida.append((plantilla, componentes))
        return salida

    def guardar_plantilla(self, plantilla, componentes):
        self.sesion.add(plantilla)
        self.sesion.flush()
        for componente in componentes:
            componente.plantilla_id = plantilla.id
        self.sesion.add_all(componentes)
        return plantilla

    def plantilla_posicion(self, semana, dia):
        return self.sesion.scalar(
            select(PlantillaMenu).where(PlantillaMenu.semana == semana, PlantillaMenu.dia == dia)
        )

    def reemplazar_plantilla(self, datos, componentes):
        plantilla = self.plantilla_posicion(datos.semana, datos.dia)
        if plantilla is None:
            plantilla = PlantillaMenu(
                semana=datos.semana,
                dia=datos.dia,
                titulo=datos.titulo,
                observaciones=datos.observaciones,
                activo=datos.activo,
            )
            return self.guardar_plantilla(plantilla, componentes)
        plantilla.titulo = datos.titulo
        plantilla.observaciones = datos.observaciones
        plantilla.activo = datos.activo
        self.sesion.execute(
            delete(ComponenteMenu).where(ComponenteMenu.plantilla_id == plantilla.id)
        )
        self.sesion.flush()
        for componente in componentes:
            componente.plantilla_id = plantilla.id
        self.sesion.add_all(componentes)
        return plantilla

    def listar_publicaciones(self):
        salida = []
        for publicacion in self.sesion.scalars(
            select(PublicacionMenu).order_by(PublicacionMenu.fecha.desc())
        ):
            componentes = self.sesion.scalars(
                select(ComponentePublicado)
                .where(ComponentePublicado.publicacion_id == publicacion.id)
                .order_by(ComponentePublicado.orden)
            ).all()
            salida.append((publicacion, componentes))
        return salida

    def plantilla_componentes(self, semana: int, dia: int):
        plantilla = self.plantilla_posicion(semana, dia)
        componentes = (
            self.sesion.scalars(
                select(ComponenteMenu)
                .where(ComponenteMenu.plantilla_id == plantilla.id)
                .order_by(ComponenteMenu.orden)
            ).all()
            if plantilla
            else []
        )
        return plantilla, componentes

    def sustitucion_componentes(self, fecha):
        sustitucion = self.sesion.scalar(
            select(SustitucionMenu).where(SustitucionMenu.fecha == fecha)
        )
        componentes = (
            self.sesion.scalars(
                select(ComponenteSustitucionMenu)
                .where(ComponenteSustitucionMenu.sustitucion_id == sustitucion.id)
                .order_by(ComponenteSustitucionMenu.orden)
            ).all()
            if sustitucion
            else []
        )
        return sustitucion, componentes

    def reemplazar_sustitucion(self, datos, componentes):
        sustitucion, _ = self.sustitucion_componentes(datos.fecha)
        if sustitucion is None:
            sustitucion = SustitucionMenu(
                fecha=datos.fecha, titulo=datos.titulo, observaciones=datos.observaciones
            )
            self.sesion.add(sustitucion)
            self.sesion.flush()
        else:
            sustitucion.titulo = datos.titulo
            sustitucion.observaciones = datos.observaciones
            self.sesion.execute(
                delete(ComponenteSustitucionMenu).where(
                    ComponenteSustitucionMenu.sustitucion_id == sustitucion.id
                )
            )
            self.sesion.flush()
        for componente in componentes:
            componente.sustitucion_id = sustitucion.id
        self.sesion.add_all(componentes)
        return sustitucion

    def guardar_publicacion(self, publicacion, componentes):
        self.sesion.add(publicacion)
        self.sesion.flush()
        for componente in componentes:
            componente.publicacion_id = publicacion.id
        self.sesion.add_all(componentes)
        return publicacion

    def listar_calendario(self, desde, hasta):
        return self.sesion.scalars(
            select(CalendarioMenu)
            .where(CalendarioMenu.fecha >= desde, CalendarioMenu.fecha <= hasta)
            .order_by(CalendarioMenu.fecha)
        ).all()

    def calendario_fecha(self, fecha):
        return self.sesion.get(CalendarioMenu, fecha)

    def reemplazar_publicacion(self, fecha, titulo, observaciones, origen, componentes):
        publicacion = self.sesion.scalar(
            select(PublicacionMenu).where(PublicacionMenu.fecha == fecha)
        )
        if publicacion:
            return None
        publicacion = PublicacionMenu(
            fecha=fecha, titulo=titulo, observaciones=observaciones, origen=origen
        )
        return self.guardar_publicacion(publicacion, componentes)

    def publicacion_fecha(self, fecha):
        return self.sesion.scalar(select(PublicacionMenu).where(PublicacionMenu.fecha == fecha))

    def publicaciones_rango(self, desde, hasta):
        return self.sesion.scalars(
            select(PublicacionMenu).where(
                PublicacionMenu.fecha >= desde, PublicacionMenu.fecha <= hasta
            )
        ).all()

    def sustituciones_rango(self, desde, hasta):
        return self.sesion.scalars(
            select(SustitucionMenu).where(
                SustitucionMenu.fecha >= desde, SustitucionMenu.fecha <= hasta
            )
        ).all()

    def listar_sustituciones(self):
        salida = []
        for sustitucion in self.sesion.scalars(
            select(SustitucionMenu).order_by(SustitucionMenu.fecha)
        ):
            componentes = self.sesion.scalars(
                select(ComponenteSustitucionMenu)
                .where(ComponenteSustitucionMenu.sustitucion_id == sustitucion.id)
                .order_by(ComponenteSustitucionMenu.orden)
            ).all()
            salida.append((sustitucion, componentes))
        return salida
