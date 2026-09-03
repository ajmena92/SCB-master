from datetime import date
import pytest

from aplicacion.codigo_qr_carnet import CodigoQrCarnet, ErrorCodigoQrCarnet


CLAVE_PRUEBA = "MDEyMzQ1Njc4OWFiY2RlZjAxMjM0NTY3ODlhYmNkZWY="


def test_emite_y_resuelve_un_codigo_qr_sin_exponer_la_cedula() -> None:
    codigo = CodigoQrCarnet(CLAVE_PRUEBA)

    token = codigo.emitir(id_persona=18, anio_lectivo=2026, hoy=date(2026, 9, 2))

    assert token.startswith("SCBQR1.")
    assert "121560069" not in token
    assert codigo.resolver(token, hoy=date(2026, 9, 2)) == 18


def test_rechaza_codigo_qr_alterado_o_vencido() -> None:
    codigo = CodigoQrCarnet(CLAVE_PRUEBA)
    token = codigo.emitir(id_persona=18, anio_lectivo=2026, hoy=date(2026, 9, 2))

    with pytest.raises(ErrorCodigoQrCarnet):
        codigo.resolver(f"{token[:-1]}X", hoy=date(2026, 9, 2))
    with pytest.raises(ErrorCodigoQrCarnet, match="vencido"):
        codigo.resolver(token, hoy=date(2027, 1, 1))
