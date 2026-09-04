"""Composición pública de los casos de uso de catálogos."""

from aplicacion.casos_catalogos_menu import CasosCatalogosMenu
from aplicacion.casos_catalogos_personas import CasosCatalogosPersonas
from aplicacion.casos_catalogos_rutas import CasosCatalogosRutas


class ServicioCatalogos(CasosCatalogosPersonas, CasosCatalogosRutas, CasosCatalogosMenu):
    def __init__(self, repo):
        self.repo = repo
