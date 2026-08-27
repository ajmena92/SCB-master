"""Contrato mínimo de la línea base de integración continua de Fase 0."""

from __future__ import annotations

from pathlib import Path

RAIZ_REPOSITORIO = Path(__file__).resolve().parents[3]
RUTA_FLUJO = RAIZ_REPOSITORIO / ".github" / "workflows" / "verificacion.yml"
RUTA_DOCKER_API = RAIZ_REPOSITORIO / "web" / "ops" / "Dockerfile.api"


def test_flujo_ci_instala_dependencias_bloqueadas_y_ejecuta_puertas() -> None:
    contenido = RUTA_FLUJO.read_text(encoding="utf-8")

    comandos_obligatorios = {
        "run: npm ci",
        "run: npm run verificar",
        "run: npm test",
        "run: npm run build",
        "run: python -m pip install -r requirements.txt",
        "run: ruff check .",
        "run: ruff format --check .",
        "run: mypy",
        "run: pytest -q",
    }

    assert comandos_obligatorios <= {linea.strip() for linea in contenido.splitlines()}


def test_flujo_ci_usa_versiones_de_runtime_declaradas_por_el_proyecto() -> None:
    contenido = RUTA_FLUJO.read_text(encoding="utf-8")

    assert "node-version-file: web/frontend/.nvmrc" in contenido
    assert 'python-version: "3.12"' in contenido
    assert "\n      - master\n" in contenido
    assert "\n      - main\n" not in contenido
    assert "\npermissions:\n  contents: read\n" in contenido
    assert "\njobs:\n  frontend:\n" in contenido
    assert "\n  backend:\n" in contenido
    assert contenido.count("working-directory: web/frontend") == 1
    assert contenido.count("working-directory: web/backend") == 1


def test_imagen_api_usa_la_entrada_modular_y_healthcheck_canonicos() -> None:
    contenido = RUTA_DOCKER_API.read_text(encoding="utf-8")

    assert "uvicorn aplicacion.entrada:crear_aplicacion --factory" in contenido
    assert "uvicorn server:app" not in contenido
    assert "/api/ready" in contenido
