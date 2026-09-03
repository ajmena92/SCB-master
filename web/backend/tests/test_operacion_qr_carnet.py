from datetime import date
from types import SimpleNamespace

from aplicacion.codigo_qr_carnet import CodigoQrCarnet
from aplicacion.servicios import ServicioOperacion


CLAVE_PRUEBA = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="


def test_captura_comedor_resuelve_el_qr_antes_de_buscar_por_cedula(monkeypatch) -> None:
    class Repositorio:
        def __init__(self) -> None:
            self.persona_encontrada = SimpleNamespace(id=18, cedula="121560069", activo=True)
            self.cedulas_buscadas: list[str] = []

        def persona(self, persona_id: int):
            return self.persona_encontrada if persona_id == 18 else None

        def persona_cedula(self, cedula: str):
            self.cedulas_buscadas.append(cedula)
            return None

    repo = Repositorio()
    servicio = ServicioOperacion(repo, CLAVE_PRUEBA)
    token = CodigoQrCarnet(CLAVE_PRUEBA).emitir(
        id_persona=18, anio_lectivo=2026, hoy=date(2026, 9, 2)
    )
    monkeypatch.setattr("aplicacion.servicios.date", SimpleNamespace(today=lambda: date(2026, 9, 2)))

    assert servicio._persona_por_carnet(token) is repo.persona_encontrada
    assert repo.cedulas_buscadas == []
    assert servicio._codigo_auditoria(token).startswith("SCBQR1.")
    assert len(servicio._codigo_auditoria(token)) == 39
    assert token not in servicio._codigo_auditoria(token)
