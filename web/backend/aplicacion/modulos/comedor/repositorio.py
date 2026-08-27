from typing import Any, Protocol, cast

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioComedor(Protocol):
    def registrar(self, id_estudiante: int, fecha: str, usuario: int) -> dict: ...


class RepositorioSqlComedor:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def registrar(self, id_estudiante: int, fecha: str, usuario: int) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "INSERT comedor.registro(id_estudiante,fecha,registrado_por) OUTPUT INSERTED.id_registro,INSERTED.id_estudiante,INSERTED.fecha,INSERTED.registrado_por VALUES(?,?,?)",
                id_estudiante,
                fecha,
                usuario,
            )
            f = c.fetchone()
            if f is None:
                raise RuntimeError("No se pudo registrar el consumo")
            columnas = c.description or ()
            return dict(zip((x[0] for x in columnas), cast(Any, f)))
