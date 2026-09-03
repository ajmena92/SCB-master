"""Reglas de seguridad del sincronizador Excel 2026."""

import importlib.util
import stat
import sys
from datetime import date
from pathlib import Path


RUTA = Path(__file__).parents[2] / "scripts" / "sincronizar_estudiantes_2026.py"
ESPECIFICACION = importlib.util.spec_from_file_location("sincronizador_2026", RUTA)
assert ESPECIFICACION and ESPECIFICACION.loader
MODULO = importlib.util.module_from_spec(ESPECIFICACION)
sys.modules[ESPECIFICACION.name] = MODULO
ESPECIFICACION.loader.exec_module(MODULO)


def test_normaliza_identificacion_como_texto_sin_inventar_digitos() -> None:
    assert MODULO.normalizar_cedula("2-0910-0243") == "209100243"
    assert MODULO.normalizar_cedula("209100243") == "209100243"
    assert MODULO.normalizar_cedula(" C03451392 ") == "C03451392"
    assert MODULO.normalizar_cedula("2-0910-243") == "20910243"


def test_rechaza_solo_identificacion_vacia() -> None:
    assert MODULO.normalizar_cedula("") is None
    assert MODULO.normalizar_cedula("   ") is None


def test_solo_regular_y_ruta_activa_de_estudiante_regular_se_sincronizan() -> None:
    padron, errores_padron = MODULO.validar_padron([
        {"fila": 2, "cedula": "1-2172-0857", "nombre": "Ana", "seccion": "7-1", "estado": "REGULAR"},
        {"fila": 3, "cedula": "1-2000-0001", "nombre": "Traslado", "seccion": "7-2", "estado": "TRASLADO"},
    ])
    rutas, errores_rutas, advertencias_rutas = MODULO.validar_rutas([
        {"fila": 2, "cedula": "121720857", "ruta": "5369", "estado": "Activo"},
        {"fila": 3, "cedula": "120000001", "ruta": "5370", "estado": "Activo"},
    ], set(padron))

    assert set(padron) == {"121720857"}
    assert errores_padron == []
    assert rutas == {"121720857": "5369"}
    assert errores_rutas == []
    assert {error["tipo"] for error in advertencias_rutas} == {"ruta_para_estudiante_no_regular"}


def test_conflicto_activo_disminuido_bloquea_solo_a_estudiante_regular() -> None:
    _, errores, advertencias = MODULO.validar_rutas([
        {"fila": 2, "cedula": "1-2172-0857", "ruta": "5369", "estado": "Activo"},
        {"fila": 3, "cedula": "1-2172-0857", "ruta": "5369", "estado": "Disminuido"},
    ], {"121720857"})

    assert {error["tipo"] for error in errores} == {"estado_ruta_contradictorio"}
    assert advertencias == []


def test_conflicto_ruta_de_fuera_del_padron_se_omite_con_advertencia() -> None:
    rutas, errores, advertencias = MODULO.validar_rutas([
        {"fila": 2, "cedula": "C03451392", "ruta": "5369", "estado": "Activo"},
        {"fila": 3, "cedula": "C03451392", "ruta": "5369", "estado": "Disminuido"},
    ], {"121720857"})

    assert rutas == {}
    assert errores == []
    assert {advertencia["tipo"] for advertencia in advertencias} == {
        "ruta_para_estudiante_no_regular", "estado_ruta_contradictorio"
    }


def test_cierra_ruta_vigente_de_regular_sin_ruta_activa_en_la_fuente() -> None:
    matriculas = {"121720857": 11, "120000001": 12}

    rutas_activas = {"121720857": "5369"}

    assert MODULO.matriculas_sin_ruta_activa(matriculas, rutas_activas) == [12]


def test_cierre_de_ruta_es_el_dia_anterior_a_la_nueva_vigencia() -> None:
    assert MODULO.fecha_cierre_ruta(date(2026, 9, 2)) == date(2026, 9, 1)


def test_persona_con_identificacion_alfanumerica_se_desactiva_si_no_esta_en_padron() -> None:
    assert MODULO.debe_desactivar_persona("C03451392", {"121720857"})
    assert MODULO.debe_desactivar_persona("1-2172-0857", {"120000001"})


def test_plan_personas_considera_persona_historica_sin_matricula_del_anio() -> None:
    padron = {"C03451392": {"cedula": "C03451392", "nombres": "Eleana", "seccion": "7-1"}}
    actuales = [
        {"persona_id": 1, "cedula": "C03451392", "activo": False},
        {"persona_id": 2, "cedula": "1-2172-0857", "activo": True},
    ]

    personas, crear, actualizar, desactivar, errores = MODULO.plan_personas_fuente(padron, actuales)

    assert set(personas) == {"C03451392", "121720857"}
    assert crear == []
    assert actualizar == ["C03451392"]
    assert desactivar == ["121720857"]
    assert errores == []


def test_plan_de_rutas_expone_creacion_cambio_y_cierre() -> None:
    actuales = {"1": {"ruta_codigo": "RUTA-1"}, "2": {"ruta_codigo": "RUTA-2"}, "3": {"ruta_codigo": None}}
    assert MODULO.plan_rutas(actuales, {"1": "1", "2": "8", "3": "7"}) == {"rutas_a_crear": 1, "rutas_a_cambiar": 1, "rutas_a_cerrar": 0}


def test_nueva_persona_tiene_cuenta_tiquete_en_la_operacion_atomica() -> None:
    contenido = RUTA.read_text(encoding="utf-8")
    assert "INSERT INTO cuenta_tiquete(persona_id,saldo,reservados) VALUES (:id,0,0)" in contenido


def test_exporta_solo_credenciales_nuevas_con_permisos_privados(tmp_path: Path, capsys) -> None:
    salida = tmp_path / "privado" / "credenciales.csv"

    MODULO.escribir_credenciales(salida, [("E-00000001", "1-2345-0678", "012345")])

    assert salida.read_text(encoding="utf-8") == "codigo,cedula,pin_temporal\nE-00000001,1-2345-0678,012345\n"
    assert stat.S_IMODE(salida.stat().st_mode) == 0o600
    assert capsys.readouterr().out == ""
    assert capsys.readouterr().err == ""


def test_credenciales_se_escriben_despues_de_confirmar_la_transaccion_y_no_en_el_reporte() -> None:
    contenido = RUTA.read_text(encoding="utf-8")

    assert "p.add_argument(\"--credenciales\"" in contenido
    assert "credenciales-sincronizacion-2026.csv" in contenido
    assert contenido.index("with motor.begin() as c:") < contenido.index("escribir_credenciales(credenciales_salida, credenciales_nuevas)")
    assert '"pin_temporal"' not in contenido[contenido.index('reporte: dict[str, Any]'):]
