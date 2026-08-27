from datetime import datetime, timezone

from aplicacion.nucleo.tiempo import fecha_local


def test_fecha_local_resuelve_el_dia_de_la_zona_configurada() -> None:
    instante = datetime(2026, 1, 2, 5, 30, tzinfo=timezone.utc)
    assert fecha_local("America/Costa_Rica", lambda: instante).isoformat() == "2026-01-01"
