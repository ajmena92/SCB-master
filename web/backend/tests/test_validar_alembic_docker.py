from pathlib import Path


def test_script_alembic_docker_no_expone_secretos() -> None:
    raiz = Path(__file__).resolve().parents[2]
    script = (raiz / "scripts" / "validar_alembic_docker.sh").read_text()
    assert "docker compose --env-file .env" in script
    assert "python -m alembic -c alembic.ini current" in script
    assert "python -m alembic -c alembic.ini upgrade head" in script
    assert 'echo "$SQL_CONNECTION_STRING"' not in script
