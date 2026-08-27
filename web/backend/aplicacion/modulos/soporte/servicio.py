from .esquemas import SolicitudEntrada, SolicitudSalida


class ServicioSoporte:
    def __init__(self, r):
        self.r = r

    def crear(self, d: SolicitudEntrada, u: int) -> SolicitudSalida:
        return SolicitudSalida(**self.r.crear(d.model_dump(), u))
