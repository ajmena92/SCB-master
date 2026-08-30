"""Alembic para una unica base PostgreSQL."""

import os
from logging.config import fileConfig

from sqlalchemy import engine_from_config, pool

import aplicacion.modelos  # noqa: F401
from alembic import context
from aplicacion.nucleo.modelos_base import BaseDeclarativa

config = context.config
if config.config_file_name:
    fileConfig(config.config_file_name)
target_metadata = BaseDeclarativa.metadata


def _url() -> str:
    valor = os.getenv("DATABASE_URL", "").strip()
    if valor.startswith("postgresql://"):
        valor = valor.replace("postgresql://", "postgresql+psycopg://", 1)
    return valor or config.get_main_option("sqlalchemy.url")


def offline() -> None:
    context.configure(
        url=_url(),
        target_metadata=target_metadata,
        literal_binds=True,
        dialect_opts={"paramstyle": "named"},
        compare_type=True,
    )
    with context.begin_transaction():
        context.run_migrations()


def online() -> None:
    seccion = config.get_section(config.config_ini_section, {})
    seccion["sqlalchemy.url"] = _url()
    motor = engine_from_config(seccion, prefix="sqlalchemy.", poolclass=pool.NullPool)
    with motor.connect() as conexion:
        context.configure(connection=conexion, target_metadata=target_metadata, compare_type=True)
        with context.begin_transaction():
            context.run_migrations()


offline() if context.is_offline_mode() else online()
