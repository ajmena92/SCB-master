"""Pruebas unitarias sin HTTP para el contrato criptográfico de CSRF y sesión."""

from datetime import datetime, timedelta, timezone

import pytest

from aplicacion.seguridad import (
    csrf_anonimo,
    csrf_sesion,
    csrf_valido,
    generar_codigo,
    nueva_sesion,
)


def test_csrf_anonimo_tiene_emision_vencimiento_y_no_es_reutilizable_tras_ttl() -> None:
    ahora = datetime(2026, 9, 3, 12, 0, tzinfo=timezone.utc)
    token = csrf_anonimo("secreto-pruebas", ttl_seconds=60, ahora=ahora)

    partes = token.split(".")
    assert partes[0] == "a"
    assert int(partes[2]) - int(partes[1]) == 60
    assert csrf_valido(
        token, token=None, secreto="secreto-pruebas", ahora=ahora + timedelta(seconds=60)
    )
    assert not csrf_valido(
        token, token=None, secreto="secreto-pruebas", ahora=ahora + timedelta(seconds=61)
    )
    assert not csrf_valido(token, token=None, secreto="otro-secreto", ahora=ahora)


def test_csrf_de_sesion_exige_el_token_opaco_correcto() -> None:
    csrf = csrf_sesion("token-opaco", "secreto-pruebas")

    assert csrf_valido(csrf, token="token-opaco", secreto="secreto-pruebas")
    assert not csrf_valido(csrf, token="otro-token", secreto="secreto-pruebas")
    assert not csrf_valido(csrf, token=None, secreto="secreto-pruebas")


def test_sesiones_usan_vencimiento_absoluto_por_tipo() -> None:
    antes = datetime.now(timezone.utc)
    _, portal = nueva_sesion(tipo="portal", student_session_days=2, admin_session_minutes=15)
    _, administracion = nueva_sesion(
        tipo="administracion", student_session_days=2, admin_session_minutes=15
    )
    despues = datetime.now(timezone.utc)

    assert antes + timedelta(days=2) <= portal.expira_en <= despues + timedelta(days=2)
    assert (
        antes + timedelta(minutes=15) <= administracion.expira_en <= despues + timedelta(minutes=15)
    )


def test_nueva_sesion_exige_una_politica_de_vigencia_explicita() -> None:
    with pytest.raises(TypeError):
        nueva_sesion(tipo="portal")


def test_seguridad_rechaza_sesiones_y_csrf_anonimo_invalidos() -> None:
    with pytest.raises(ValueError, match="Tipo de sesion no valido"):
        nueva_sesion(tipo="invalido", student_session_days=2, admin_session_minutes=15)

    assert not csrf_valido("no-es-un-token", token=None, secreto="secreto-pruebas")
    assert not csrf_valido(
        "a.1.2.nonce.firma", token=None, secreto="secreto-pruebas", ahora=datetime(2026, 1, 1)
    )


def test_generar_codigo_reintenta_hasta_encontrar_un_codigo_disponible(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    valores = iter((18, 19))
    monkeypatch.setattr("aplicacion.seguridad.secrets.randbelow", lambda _: next(valores))
    consultas: list[str] = []

    def codigo_existe(codigo: str) -> bool:
        consultas.append(codigo)
        return len(consultas) == 1

    codigo = generar_codigo(codigo_existe, "profesor")

    assert len(consultas) == 2
    assert codigo == consultas[-1]
    assert codigo.startswith("P-")
