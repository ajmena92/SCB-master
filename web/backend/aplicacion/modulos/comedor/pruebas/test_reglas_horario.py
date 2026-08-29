from datetime import time

from aplicacion.modulos.comedor.reglas_horario import (
    esta_dentro_de_hora_limite,
    normalizar_horario,
)


def test_hora_limite_incluye_exactamente_el_minuto_configurado() -> None:
    assert esta_dentro_de_hora_limite(time(12, 0), time(12, 0))
    assert not esta_dentro_de_hora_limite(time(12, 1), time(12, 0))


def test_normaliza_solo_horarios_canónicos() -> None:
    assert normalizar_horario(" DIURNO ") == "diurno"
    assert normalizar_horario("vespertino") is None
