"""Contrato mínimo de la línea base de integración continua de Fase 0."""

from __future__ import annotations

from pathlib import Path

RAIZ_REPOSITORIO = Path(__file__).resolve().parents[3]
RUTA_FLUJO = RAIZ_REPOSITORIO / ".github" / "workflows" / "verificacion.yml"
RUTA_DOCKER_API = RAIZ_REPOSITORIO / "web" / "ops" / "Dockerfile.api"
RUTA_DOCKER_MIGRACION = RAIZ_REPOSITORIO / "web" / "ops" / "Dockerfile.migracion"
RUTA_COMPOSE_PRODUCCION = RAIZ_REPOSITORIO / "web" / "ops" / "compose.production.yml"
RUTA_MEDICION_MEMORIA = RAIZ_REPOSITORIO / "web" / "scripts" / "medir_memoria_operativa.sh"
RUTA_NGINX = RAIZ_REPOSITORIO / "web" / "ops" / "nginx" / "default.conf"


def test_flujo_ci_instala_dependencias_bloqueadas_y_ejecuta_puertas() -> None:
    contenido = RUTA_FLUJO.read_text(encoding="utf-8")

    comandos_obligatorios = {
        "run: npm ci",
        "run: npm run verificar",
        "run: npm test",
        "run: npm run build",
        "run: python -m pip install -r requirements-desarrollo.txt",
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


def test_imagen_api_no_contiene_dependencias_ni_fuentes_de_pruebas() -> None:
    contenido = RUTA_DOCKER_API.read_text(encoding="utf-8")
    produccion = (RAIZ_REPOSITORIO / "web" / "backend" / "requirements-produccion.txt").read_text(
        encoding="utf-8"
    )

    assert "requirements-produccion.txt" in contenido
    assert "COPY backend/aplicacion ./aplicacion" in contenido
    assert "COPY backend/tests" not in contenido
    assert "pytest" not in produccion
    assert "ruff" not in produccion
    assert "mypy" not in produccion
    assert "alembic" not in produccion


def test_migracion_y_puerta_de_memoria_tienen_entradas_separadas() -> None:
    migracion = RUTA_DOCKER_MIGRACION.read_text(encoding="utf-8")
    compose = RUTA_COMPOSE_PRODUCCION.read_text(encoding="utf-8")
    medicion = RUTA_MEDICION_MEMORIA.read_text(encoding="utf-8")
    soporte_medicion = (RAIZ_REPOSITORIO / "web" / "scripts" / "medir_memoria_docker.sh").read_text(
        encoding="utf-8"
    )
    entrada = (RAIZ_REPOSITORIO / "web" / "ops" / "entrada_migracion.sh").read_text(
        encoding="utf-8"
    )

    assert "requirements-migracion.txt" in migracion
    assert 'ENTRYPOINT ["/usr/local/bin/entrada_migracion.sh"]' in migracion
    assert "MIGRACION_MANUAL_DBA:-" in entrada
    assert '!= "confirmada"' in entrada
    assert "dockerfile: ops/Dockerfile.migracion" in compose
    assert "profiles:\n      - migracion" in compose
    assert 'restart: "no"' in compose
    assert 'source "$directorio_script/medir_memoria_docker.sh"' in medicion
    assert "docker stats --no-stream" in soporte_medicion
    assert "UMBRAL_MEMORIA_PORCENTAJE" in medicion
    assert "respuestas_413" in medicion
    assert "errores_5xx" in medicion
    assert "latencia_p95_ms" in medicion
    assert "LATENCIA_P95_UMBRAL_MS" in medicion
    assert "AUMENTO_LATENCIA_MAXIMO_PORCENTAJE" in medicion
    assert "oom_killed_api" in medicion
    assert "USUARIOS_PRUEBA" in medicion


def test_cargas_http_tienen_limite_en_proxy_y_lectura_acotada() -> None:
    nginx = RUTA_NGINX.read_text(encoding="utf-8")
    fotos = (
        RAIZ_REPOSITORIO / "web" / "backend" / "aplicacion" / "modulos" / "estudiantes" / "fotos.py"
    ).read_text(encoding="utf-8")
    importaciones = (
        RAIZ_REPOSITORIO / "web" / "backend" / "aplicacion" / "modulos" / "importaciones" / "api.py"
    ).read_text(encoding="utf-8")

    assert "client_max_body_size 12m;" in nginx
    assert "await archivo.read()" not in fotos
    assert "await archivo.read()" not in importaciones
