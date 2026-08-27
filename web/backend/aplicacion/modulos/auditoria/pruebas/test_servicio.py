from aplicacion.modulos.auditoria.esquemas import EventoEntrada
from aplicacion.modulos.auditoria.servicio import ServicioAuditoria


class RepositorioMemoria:
    def registrar(self, evento, usuario, ip):
        return {
            **evento.model_dump(),
            "id_evento": 1,
            "id_usuario": usuario,
            "direccion_ip": ip,
            "creado_en": "2026-01-01T00:00:00Z",
        }

    def consultar(self, limite):
        return []


def test_registra_evento_con_detalle() -> None:
    caso = ServicioAuditoria(RepositorioMemoria())
    salida = caso.registrar(
        EventoEntrada(modulo="cuentas", accion="recarga", entidad="cuenta", detalle={"monto": 10}),
        3,
        "127.0.0.1",
    )
    assert salida.id_evento == 1
    assert salida.detalle["monto"] == 10
