"""Pruebas unitarias del validador de configuración de operaciones."""

import importlib.util
import subprocess
import sys
from pathlib import Path


RAIZ_WEB = Path(__file__).resolve().parents[2]
RUTA_VALIDADOR = RAIZ_WEB / "scripts" / "validar_variables.py"
ESPECIFICACION = importlib.util.spec_from_file_location("validar_variables", RUTA_VALIDADOR)
assert ESPECIFICACION and ESPECIFICACION.loader
VALIDADOR = importlib.util.module_from_spec(ESPECIFICACION)
ESPECIFICACION.loader.exec_module(VALIDADOR)


def test_cargar_env_convierte_booleanos_y_enteros(tmp_path: Path) -> None:
    archivo = tmp_path / ".env"
    archivo.write_text("COOKIE_SECURE=false\nUVICORN_WORKERS=4\nCORS_ORIGIN=http://localhost\n")

    configuracion = VALIDADOR.cargar_env(str(archivo))

    assert configuracion == {
        "COOKIE_SECURE": False,
        "UVICORN_WORKERS": 4,
        "CORS_ORIGIN": "http://localhost",
    }


def test_validar_cors_rechaza_cookie_no_booleana_en_produccion() -> None:
    errores = VALIDADOR.validar_cors(
        {
            "APP_ENV": "production",
            "CORS_ORIGIN": "https://portal.ejemplo.edu.cr",
            "COOKIE_SECURE": "false",
        }
    )

    assert errores == ["ERROR: COOKIE_SECURE debe ser booleano."]


def test_template_local_y_produccion_no_exigen_archivos_secretos() -> None:
    for entorno in ("local", "production"):
        resultado = subprocess.run(
            [sys.executable, str(RUTA_VALIDADOR), "--template", entorno],
            check=False,
            capture_output=True,
            text=True,
        )
        assert resultado.returncode == 0, resultado.stderr + resultado.stdout


def test_preflight_exige_archivo_existente_para_secreto_local(tmp_path: Path) -> None:
    errores = VALIDADOR.validar_secretos(
        {"CSRF_SECRET_FILE": str(tmp_path / "csrf_secret")},
        validar_existencia=True,
    )

    assert errores == [
        f"ERROR: Archivo secreto no encontrado: {tmp_path / 'csrf_secret'} (CSRF_SECRET_FILE)"
    ]
