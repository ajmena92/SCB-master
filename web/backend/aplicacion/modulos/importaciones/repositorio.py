import json
from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioImportaciones(Protocol):
    def crear_lote(
        self, nombre: str, filas: list[dict[str, str]], errores: list[dict], usuario: int
    ) -> dict: ...
    def lote(self, id_lote: int) -> dict: ...
    def revertir(self, id_lote: int, usuario: int) -> dict: ...


class RepositorioSqlImportaciones:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _fila(c: CursorSql) -> dict | None:
        f = c.fetchone()
        return None if f is None else dict(zip((x[0] for x in c.description), f))

    def crear_lote(
        self, nombre: str, filas: list[dict[str, str]], errores: list[dict], usuario: int
    ) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "INSERT INTO importaciones.lote (nombre_archivo, total_filas, errores_json, creado_por) OUTPUT INSERTED.id_lote, INSERTED.nombre_archivo, INSERTED.estado, INSERTED.total_filas, INSERTED.errores_json, INSERTED.creado_en VALUES (?, ?, ?, ?)",
                nombre,
                len(filas),
                json.dumps(errores, ensure_ascii=False),
                usuario,
            )
            lote = self._fila(c)
            if lote is None:
                raise RuntimeError("No se pudo crear el lote")
            for fila in filas:
                c.execute(
                    "INSERT INTO importaciones.fila (id_lote, datos_json) VALUES (?, ?)",
                    lote["id_lote"],
                    json.dumps(fila, ensure_ascii=False),
                )
            lote["errores"] = errores
            return lote

    def lote(self, id_lote: int) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "SELECT id_lote, nombre_archivo, estado, total_filas, errores_json, creado_en, revertido_en FROM importaciones.lote WHERE id_lote=?",
                id_lote,
            )
            r = self._fila(c)
            if r is None:
                raise ValueError("El lote no existe")
            r["errores"] = json.loads(r.pop("errores_json") or "[]")
            return r

    def revertir(self, id_lote: int, usuario: int) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "UPDATE importaciones.lote SET estado='revertido', revertido_en=SYSUTCDATETIME(), revertido_por=? WHERE id_lote=? AND estado='aplicado'",
                usuario,
                id_lote,
            )
            if c.rowcount == 0:
                raise ValueError("El lote no existe o ya fue revertido")
        return self.lote(id_lote)
