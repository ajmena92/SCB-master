"""Casos de uso para reportes operativos."""


class ServicioReportes:
    def __init__(self, repo):
        self.repo = repo

    def comedor(self, desde, hasta):
        return self.repo.comedor(desde, hasta)

    def transporte(self, desde, hasta):
        return self.repo.transporte(desde, hasta)

    def ventas(self, desde, hasta):
        return self.repo.ventas(desde, hasta)
