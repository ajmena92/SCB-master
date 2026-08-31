"""Sesion SQLAlchemy y dependencia transaccional."""

from __future__ import annotations

from collections.abc import AsyncIterator

from sqlalchemy import create_engine
from sqlalchemy.engine import Engine
from sqlalchemy.orm import Session, sessionmaker
from sqlalchemy.pool import StaticPool


def crear_motor(url: str) -> Engine:
    argumentos = {"check_same_thread": False} if url.startswith("sqlite") else {}
    opciones = {"poolclass": StaticPool} if url in {"sqlite://", "sqlite:///:memory:"} else {}
    return create_engine(url, pool_pre_ping=True, connect_args=argumentos, **opciones)


def crear_fabrica_sesiones(motor: Engine) -> sessionmaker[Session]:
    return sessionmaker(bind=motor, autoflush=False, expire_on_commit=False)


def dependencia_sesion(fabrica: sessionmaker[Session]):
    async def obtener() -> AsyncIterator[Session]:
        with fabrica() as sesion:
            try:
                yield sesion
                sesion.commit()
            except Exception:
                sesion.rollback()
                raise

    return obtener
