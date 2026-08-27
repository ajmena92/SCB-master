from pathlib import Path


def test_script_alembic_docker_no_expone_secretos() -> None:
    raiz = Path(__file__).resolve().parents[2]
    script = (raiz / "scripts" / "validar_alembic_docker.sh").read_text()
    assert "docker compose --env-file .env" in script
    assert "--profile migracion" in script
    assert "GRUPO_DBA_MIGRACION" in script
    assert "CONFIRMAR_MIGRACION_DBA" in script
    assert '--env MIGRACION_MANUAL_DBA=confirmada migracion "$accion"' in script
    assert 'echo "$SQL_CONNECTION_STRING"' not in script
