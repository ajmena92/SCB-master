"""Persistencia tipada de catalogos, matriculas, rutas y menu."""

from sqlalchemy import delete, func, select
from sqlalchemy.orm import Session

from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    Matricula,
    Persona,
    Ruta,
)
from aplicacion.modelos.menu import (
    ComponenteMenu,
    ComponentePublicado,
    PlantillaMenu,
    PublicacionMenu,
)
from aplicacion.repositorios import desactivar_anios


class RepositorioCatalogos:
    def __init__(self, sesion: Session):
        self.sesion = sesion

    def listar_personas(self):
        return self.sesion.scalars(select(Persona).order_by(Persona.nombres)).all()

    def persona(self, persona_id: int):
        return self.sesion.get(Persona, persona_id)

    def codigo_existe(self, codigo: str) -> bool:
        return self.sesion.scalar(select(Persona.id).where(Persona.codigo == codigo)) is not None

    def guardar_persona(self, persona, credencial, cuenta) -> None:
        self.sesion.add(persona)
        self.sesion.flush()
        credencial.persona_id = cuenta.persona_id = persona.id
        self.sesion.add_all([credencial, cuenta])

    def listar_anios(self):
        return self.sesion.scalars(select(AnioLectivo).order_by(AnioLectivo.anio.desc())).all()

    def guardar_anio(self, registro: AnioLectivo):
        if registro.vigente:
            desactivar_anios(self.sesion)
        self.sesion.add(registro)
        self.sesion.flush()
        return registro

    def activar_anio(self, anio_id: int):
        registro = self.sesion.get(AnioLectivo, anio_id)
        if registro:
            desactivar_anios(self.sesion)
            registro.vigente = True
        return registro

    def listar_matriculas(self, anio_id: int | None):
        consulta = select(Matricula)
        if anio_id:
            consulta = consulta.where(Matricula.anio_lectivo_id == anio_id)
        return self.sesion.scalars(consulta.order_by(Matricula.id)).all()

    def matricula(self, matricula_id: int):
        return self.sesion.get(Matricula, matricula_id)

    def guardar(self, registro):
        self.sesion.add(registro)
        self.sesion.flush()
        return registro

    def listar_rutas(self):
        return self.sesion.execute(
            select(Ruta, func.count(AsignacionRuta.id))
            .outerjoin(AsignacionRuta, AsignacionRuta.ruta_id == Ruta.id)
            .group_by(Ruta.id)
            .order_by(Ruta.codigo)
        ).all()

    def contar_asignados(self, ruta_id: int) -> int:
        return int(
            self.sesion.scalar(
                select(func.count(AsignacionRuta.id)).where(AsignacionRuta.ruta_id == ruta_id)
            )
            or 0
        )

    def ruta(self, ruta_id: int):
        return self.sesion.get(Ruta, ruta_id)

    def asignacion_solapada(self, entrada) -> bool:
        from datetime import date

        from sqlalchemy import or_

        return (
            self.sesion.scalar(
                select(AsignacionRuta.id).where(
                    AsignacionRuta.matricula_id == entrada.matricula_id,
                    AsignacionRuta.fecha_inicio <= (entrada.fecha_fin or date.max),
                    or_(
                        AsignacionRuta.fecha_fin.is_(None),
                        AsignacionRuta.fecha_fin >= entrada.fecha_inicio,
                    ),
                )
            )
            is not None
        )

    def listar_plantillas(self):
        salida = []
        for plantilla in self.sesion.scalars(select(PlantillaMenu).order_by(PlantillaMenu.nombre)):
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

    def actualizar_plantilla(self, plantilla_id, nombre, componentes):
        plantilla = self.sesion.get(PlantillaMenu, plantilla_id)
        if not plantilla:
            return None
        plantilla.nombre = nombre
        self.sesion.execute(
            delete(ComponenteMenu).where(ComponenteMenu.plantilla_id == plantilla_id)
        )
        self.sesion.flush()
        for componente in componentes:
            componente.plantilla_id = plantilla_id
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

    def plantilla_componentes(self, plantilla_id: int):
        plantilla = self.sesion.get(PlantillaMenu, plantilla_id)
        componentes = (
            self.sesion.scalars(
                select(ComponenteMenu)
                .where(ComponenteMenu.plantilla_id == plantilla_id)
                .order_by(ComponenteMenu.orden)
            ).all()
            if plantilla
            else []
        )
        return plantilla, componentes

    def guardar_publicacion(self, publicacion, componentes):
        self.sesion.add(publicacion)
        self.sesion.flush()
        for componente in componentes:
            componente.publicacion_id = publicacion.id
        self.sesion.add_all(componentes)
        return publicacion

    def reemplazar_publicacion(self, fecha, nombre, componentes):
        publicacion = self.sesion.scalar(
            select(PublicacionMenu).where(PublicacionMenu.fecha == fecha)
        )
        if not publicacion:
            return self.guardar_publicacion(
                PublicacionMenu(fecha=fecha, nombre=nombre), componentes
            )
        publicacion.nombre = nombre
        self.sesion.execute(
            delete(ComponentePublicado).where(ComponentePublicado.publicacion_id == publicacion.id)
        )
        self.sesion.flush()
        for componente in componentes:
            componente.publicacion_id = publicacion.id
        self.sesion.add_all(componentes)
        return publicacion
