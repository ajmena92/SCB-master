from pathlib import Path

from aplicacion.nucleo.modelos import BaseDeclarativa, Estudiante, Ruta, Usuario

RAIZ = Path(__file__).resolve().parents[1]


def test_modelos_canonicos_declaran_todos_los_dominios() -> None:
    esquemas = {tabla.schema for tabla in BaseDeclarativa.metadata.tables.values()}
    assert esquemas == {
        "identidad", "transporte", "estudiantes", "asistencia", "beneficios", "cuentas",
        "reportes", "importaciones", "auditoria", "menu", "comedor", "soporte",
    }
    assert Usuario.__table__.schema == "identidad"
    assert Ruta.__table__.schema == "transporte"
    assert Estudiante.__table__.schema == "estudiantes"
    assert Estudiante.__table__.c.id_ruta.foreign_keys


def test_revision_alembic_dominios_es_reproducible_y_canonica() -> None:
    revision = (RAIZ / "alembic" / "versions" / "0002_dominios_web.py").read_text()
    assert 'down_revision: Union[str, None] = "0001_identidad_usuario"' in revision
    assert "server.py" not in revision
    assert "dbo." not in revision


def test_alembic_ini_offline_no_contiene_credenciales() -> None:
    configuracion = (RAIZ / "alembic.ini").read_text()
    assert "sqlalchemy.url = mssql+pyodbc://" in configuracion
    assert "SQL_CONNECTION_STRING" not in configuracion
    assert "Pwd=" not in configuracion


def test_script_staging_no_imprime_la_cadena_de_conexion() -> None:
    script = (RAIZ.parent / "scripts" / "validar_alembic_staging.sh").read_text()
    assert 'python_bin="${PYTHON_BIN:-python}"' in script
    assert '"$python_bin" -m alembic -c alembic.ini current' in script
    assert 'echo "$SQL_CONNECTION_STRING"' not in script
    assert "Encrypt=yes" in script
