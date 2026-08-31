"""Casos de uso de datos maestros y menu."""

import secrets

from fastapi import HTTPException
from sqlalchemy.exc import IntegrityError

from aplicacion.esquemas import PersonaSalida
from aplicacion.modelos.maestros import (
    AnioLectivo,
    AsignacionRuta,
    CredencialPortal,
    Matricula,
    Persona,
    Ruta,
)
from aplicacion.modelos.menu import (
    ComponenteMenu,
    ComponentePublicado,
    PlantillaMenu,
)
from aplicacion.modelos.operacion import CuentaTiquete
from aplicacion.seguridad import generar_codigo, hash_secreto


class ServicioCatalogos:
    def __init__(self, repo):
        self.repo = repo

    def listar_personas(self):
        return self.repo.listar_personas()

    def listar_anios(self):
        return self.repo.listar_anios()

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

    def crear_persona(self, datos):
        pin = f"{secrets.randbelow(1_000_000):06d}"
        persona = Persona(
            codigo=generar_codigo(self.repo.codigo_existe, datos.tipo),
            **datos.model_dump(),
        )
        try:
            self.repo.guardar_persona(
                persona,
                CredencialPortal(pin_hash=hash_secreto(pin), cambio_obligatorio=True),
                CuentaTiquete(saldo=0, reservados=0),
            )
        except IntegrityError as exc:
            raise HTTPException(409, "La cedula ya esta registrada") from exc
        return PersonaSalida.model_validate(persona).model_copy(update={"pin_temporal": pin})

    def crear_anio(self, datos):
        return self.repo.guardar_anio(AnioLectivo(**datos.model_dump()))

    def activar_anio(self, anio_id):
        registro = self.repo.activar_anio(anio_id)
        if not registro:
            raise HTTPException(404, "Año lectivo no encontrado")
        return registro

    def crear_matricula(self, datos):
        persona = self.repo.persona(datos.persona_id)
        if not persona or persona.tipo != "estudiante":
            raise HTTPException(409, "La matricula requiere estudiante")
        try:
            return self.repo.guardar(Matricula(**datos.model_dump(), turno="1"))
        except IntegrityError as exc:
            raise HTTPException(409, "Ya existe matricula para persona y año") from exc

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
                "nombre": p.nombre,
                "activa": p.activo,
                "componentes": [c.nombre for c in cs],
            }
            for p, cs in self.repo.listar_plantillas()
        ]

    def crear_plantilla(self, datos):
        p = PlantillaMenu(nombre=datos.nombre, activo=True)
        cs = [ComponenteMenu(nombre=n, orden=i) for i, n in enumerate(datos.componentes, 1)]
        self.repo.guardar_plantilla(p, cs)
        return {"id": p.id, **datos.model_dump()}

    def actualizar_plantilla(self, plantilla_id, datos):
        cs = [ComponenteMenu(nombre=n, orden=i) for i, n in enumerate(datos.componentes, 1)]
        p = self.repo.actualizar_plantilla(plantilla_id, datos.nombre, cs)
        if not p:
            raise HTTPException(404, "Plantilla no encontrada")
        return {"id": p.id, **datos.model_dump()}

    def listar_publicaciones(self):
        return [
            {
                "id": p.id,
                "fecha": p.fecha,
                "nombre": p.nombre,
                "componentes": [c.nombre for c in cs],
            }
            for p, cs in self.repo.listar_publicaciones()
        ]

    def publicar(self, datos):
        plantilla, origen = self.repo.plantilla_componentes(datos.plantilla_id)
        if not plantilla:
            raise HTTPException(404, "Plantilla no encontrada")
        cs = [ComponentePublicado(nombre=c.nombre, orden=c.orden) for c in origen]
        p = self.repo.reemplazar_publicacion(datos.fecha, plantilla.nombre, cs)
        return {
            "id": p.id,
            "fecha": p.fecha,
            "nombre": p.nombre,
            "componentes": [c.nombre for c in cs],
        }
