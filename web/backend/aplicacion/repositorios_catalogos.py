"""Fachada de persistencia del dominio de catálogos."""

from sqlalchemy.orm import Session

from aplicacion.repositorios_catalogos_menu import RepositorioCatalogosMenu
from aplicacion.repositorios_catalogos_personas import RepositorioCatalogosPersonas
from aplicacion.repositorios_catalogos_rutas import RepositorioCatalogosRutas


class RepositorioCatalogos:
    def __init__(self, sesion: Session):
        self.sesion = sesion
        self.personas = RepositorioCatalogosPersonas(sesion)
        self.rutas = RepositorioCatalogosRutas(sesion)
        self.menu = RepositorioCatalogosMenu(sesion)

    def __getattr__(self, nombre):
        """Conserva el contrato público mientras se migra a composición."""
        for componente in (self.personas, self.rutas, self.menu):
            try:
                return getattr(componente, nombre)
            except AttributeError:
                continue
        raise AttributeError(nombre)

    def guardar(self, registro):
        self.sesion.add(registro)
        self.sesion.flush()
        return registro
