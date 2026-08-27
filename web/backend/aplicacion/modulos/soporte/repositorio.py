from typing import Any, Protocol, cast

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioSoporte(Protocol):
    def crear(self, d: dict, u: int) -> dict: ...


class RepositorioSqlSoporte:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self.fabrica = fabrica

    def crear(self, d: dict, u: int) -> dict:
        with self.fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "INSERT soporte.solicitud(asunto,detalle,estado,creado_por) OUTPUT INSERTED.id_solicitud,INSERTED.asunto,INSERTED.detalle,INSERTED.estado,INSERTED.creado_por VALUES(?,?,?,?)",
                d["asunto"],
                d["detalle"],
                "abierta",
                u,
            )
            f = c.fetchone()
            if f is None:
                raise RuntimeError("No se pudo crear la solicitud")
            columnas = c.description or ()
            return dict(zip((x[0] for x in columnas), cast(Any, f)))
