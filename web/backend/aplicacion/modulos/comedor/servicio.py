from .esquemas import RegistroComedorEntrada, RegistroComedorSalida
from .repositorio import RepositorioComedor


class ServicioComedor:
    def __init__(self, r: RepositorioComedor) -> None:
        self.r = r

    def registrar(self, d: RegistroComedorEntrada, u: int) -> RegistroComedorSalida:
        return RegistroComedorSalida(**self.r.registrar(d.id_estudiante, d.fecha, u))
