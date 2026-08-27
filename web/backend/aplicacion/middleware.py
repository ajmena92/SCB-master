"""Configuración de middleware HTTP transversal."""

from fastapi import FastAPI
from starlette.middleware.cors import CORSMiddleware

from config import Settings


def configurar_cors(aplicacion: FastAPI, configuracion: Settings | None) -> None:
    """Añade CORS solo cuando la aplicación se crea desde configuración real."""

    if configuracion is None:
        return
    aplicacion.add_middleware(
        CORSMiddleware,
        allow_origins=[configuracion.cors_origin],
        allow_credentials=True,
        allow_methods=["GET", "POST", "PUT"],
        allow_headers=["Content-Type", "X-CSRF-Token"],
    )
