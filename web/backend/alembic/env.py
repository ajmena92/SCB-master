"""Configuración Alembic para el modelo web canónico."""

import os
from logging.config import fileConfig

from sqlalchemy import pool
from sqlalchemy.engine import URL

from alembic import context

# Importación explícita: registra todos los modelos de dominio en el metadata.
from aplicacion.modulos.asistencia import modelos as _modelos_asistencia  # noqa: F401
from aplicacion.modulos.auditoria import modelos as _modelos_auditoria  # noqa: F401
from aplicacion.modulos.beneficios import modelos as _modelos_beneficios  # noqa: F401
from aplicacion.modulos.comedor import modelos as _modelos_comedor  # noqa: F401
from aplicacion.modulos.cuentas import modelos as _modelos_cuentas  # noqa: F401
from aplicacion.modulos.estudiantes import modelos as _modelos_estudiantes  # noqa: F401
from aplicacion.modulos.identidad import modelos as _modelos_identidad  # noqa: F401
from aplicacion.modulos.importaciones import modelos as _modelos_importaciones  # noqa: F401
from aplicacion.modulos.menu import modelos as _modelos_menu  # noqa: F401
from aplicacion.modulos.parametros import modelos as _modelos_parametros  # noqa: F401
from aplicacion.modulos.reportes import modelos as _modelos_reportes  # noqa: F401
from aplicacion.modulos.soporte import modelos as _modelos_soporte  # noqa: F401
from aplicacion.modulos.transporte import modelos as _modelos_transporte  # noqa: F401
from aplicacion.nucleo.dialecto_sql_server import DialectoSqlServerCompatible
from aplicacion.nucleo.modelos_base import BaseDeclarativa

config = context.config
if config.config_file_name is not None:
    fileConfig(config.config_file_name)
target_metadata = BaseDeclarativa.metadata
_ESQUEMAS_CANONICOS = {
    "identidad",
    "estudiantes",
    "transporte",
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


def _incluir_objeto(objeto, nombre, tipo, reflejado, comparado):
    """No propone borrar objetos históricos fuera de los esquemas web."""
    if tipo == "table" and reflejado and getattr(objeto, "schema", None) not in _ESQUEMAS_CANONICOS:
        return False
    if (
        tipo == "column"
        and getattr(getattr(objeto, "table", None), "schema", None) == "estudiantes"
        and getattr(getattr(objeto, "table", None), "name", None) == "estudiante"
    ):
        return False
    return True


def _url(*, en_linea: bool) -> str | URL:
    cadena = os.getenv("SQL_CONNECTION_STRING", "").strip()
    if not cadena:
        if en_linea:
            raise RuntimeError("SQL_CONNECTION_STRING es requerida para Alembic en línea")
        # La URL del ini solo contiene dialecto; jamás debe contener secretos.
        return config.get_main_option("sqlalchemy.url")
    return URL.create("mssql+pyodbc", query={"odbc_connect": cadena})


def run_migrations_offline() -> None:
    context.configure(
        url=_url(en_linea=False),
        target_metadata=target_metadata,
        literal_binds=True,
        dialect_opts={"paramstyle": "named"},
        include_schemas=True,
        include_object=_incluir_objeto,
    )
    with context.begin_transaction():
        context.run_migrations()


def run_migrations_online() -> None:
    configuracion = config.get_section(config.config_ini_section, {})
    configuracion["sqlalchemy.url"] = str(_url(en_linea=True))
    configuracion.pop("sqlalchemy.url", None)
    from sqlalchemy import create_engine

    conexion = create_engine(
        str(_url(en_linea=True)), poolclass=pool.NullPool, dialect=DialectoSqlServerCompatible()
    )
    with conexion.connect() as connection:
        context.configure(
            connection=connection,
            target_metadata=target_metadata,
            include_schemas=True,
            include_object=_incluir_objeto,
        )
        with context.begin_transaction():
            context.run_migrations()


if context.is_offline_mode():
    run_migrations_offline()
else:
    run_migrations_online()
