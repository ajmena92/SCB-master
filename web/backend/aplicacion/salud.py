"""Rutas operativas de salud y disponibilidad de la aplicación."""

from fastapi import FastAPI, HTTPException

from aplicacion.modulos.salud.repositorio import RepositorioSalud


def registrar_rutas_salud(aplicacion: FastAPI, repositorio: RepositorioSalud) -> None:
    """Registra los endpoints operativos sin acoplarlos a la entrada."""

    @aplicacion.get("/api/health", tags=["operacion"])
    def consultar_salud() -> dict[str, str]:
        """Comprueba que el proceso puede abrir SQL Server."""
        try:
            repositorio.comprobar_conexion()
        except Exception as exc:
            raise HTTPException(503, "SQL no disponible") from exc
        return {"status": "ok"}

    @aplicacion.get("/api/ready", tags=["operacion"])
    def consultar_disponibilidad() -> dict[str, str]:
        """Readiness para el orquestador; exige la misma comprobación que health."""
        try:
            repositorio.comprobar_conexion()
        except Exception as exc:
            raise HTTPException(503, "SQL no disponible") from exc
        return {"status": "ready"}
