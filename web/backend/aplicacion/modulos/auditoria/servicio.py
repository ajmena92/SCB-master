from .esquemas import EventoEntrada, EventoSalida
from .repositorio import RepositorioAuditoria


class ServicioAuditoria:
    def __init__(self, repositorio: RepositorioAuditoria) -> None:
        self._repositorio = repositorio

    def registrar(self, evento: EventoEntrada, usuario: int | None, ip: str | None) -> EventoSalida:
        return EventoSalida(**self._repositorio.registrar(evento, usuario, ip))

    def consultar(self, limite: int = 100) -> list[EventoSalida]:
        return [EventoSalida(**e) for e in self._repositorio.consultar(limite)]
