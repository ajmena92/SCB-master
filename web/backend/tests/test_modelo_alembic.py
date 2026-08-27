from pathlib import Path

from aplicacion.modulos.asistencia import modelos as _modelos_asistencia  # noqa: F401
from aplicacion.modulos.auditoria import modelos as _modelos_auditoria  # noqa: F401
from aplicacion.modulos.beneficios import modelos as _modelos_beneficios  # noqa: F401
from aplicacion.modulos.comedor import modelos as _modelos_comedor  # noqa: F401
from aplicacion.modulos.cuentas import modelos as _modelos_cuentas  # noqa: F401
from aplicacion.modulos.estudiantes.modelos import Estudiante
from aplicacion.modulos.identidad.modelos import Usuario
from aplicacion.modulos.importaciones import modelos as _modelos_importaciones  # noqa: F401
from aplicacion.modulos.menu import modelos as _modelos_menu  # noqa: F401
from aplicacion.modulos.parametros import modelos as _modelos_parametros  # noqa: F401
from aplicacion.modulos.reportes import modelos as _modelos_reportes  # noqa: F401
from aplicacion.modulos.soporte import modelos as _modelos_soporte  # noqa: F401
from aplicacion.modulos.transporte.modelos import Ruta
from aplicacion.nucleo.modelos_base import BaseDeclarativa

RAIZ = Path(__file__).resolve().parents[1]


def test_modelos_canonicos_declaran_todos_los_dominios() -> None:
    esquemas = {tabla.schema for tabla in BaseDeclarativa.metadata.tables.values()}
    assert esquemas == {
        "identidad",
        "transporte",
        "estudiantes",
        "asistencia",
        "beneficios",
        "cuentas",
        "reportes",
        "importaciones",
        "auditoria",
        "menu",
        "comedor",
        "soporte",
    }
    assert Usuario.__table__.schema == "identidad"
    assert Ruta.__table__.schema == "transporte"
    assert Estudiante.__table__.schema == "estudiantes"
    assert Estudiante.__table__.c.id_ruta.foreign_keys


def test_nucleo_no_contiene_modelos_de_dominio() -> None:
    nucleo = RAIZ / "aplicacion" / "nucleo"
    assert not (nucleo / "modelos.py").exists()
    clases_orm = [
        clase
        for clase in BaseDeclarativa.registry.mappers
        if clase.class_.__module__.startswith("aplicacion.nucleo")
    ]
    assert clases_orm == []


def test_cada_dominio_expone_modelos_orm_sobre_la_base_comun() -> None:
    dominios = (
        "asistencia",
        "auditoria",
        "beneficios",
        "comedor",
        "cuentas",
        "estudiantes",
        "identidad",
        "importaciones",
        "menu",
        "parametros",
        "reportes",
        "soporte",
        "transporte",
    )
    for dominio in dominios:
        modulo = __import__(f"aplicacion.modulos.{dominio}.modelos", fromlist=["*"])
        clases = [
            valor
            for valor in vars(modulo).values()
            if isinstance(valor, type)
            and issubclass(valor, BaseDeclarativa)
            and valor is not BaseDeclarativa
        ]
        assert clases, f"El dominio {dominio} no declara modelos ORM"
        assert all(clase.__module__ == modulo.__name__ for clase in clases)


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
