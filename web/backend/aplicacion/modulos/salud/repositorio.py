"""Persistencia mínima para comprobar la disponibilidad de SQL Server."""

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioSalud:
    """Encapsula la consulta técnica de disponibilidad de la base de datos."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def comprobar_conexion(self) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute("SELECT 1 AS ok").fetchone()
