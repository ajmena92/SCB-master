import json
from typing import Protocol, cast

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql

from .esquemas import EventoEntrada


class RepositorioAuditoria(Protocol):
    def registrar(self, evento: EventoEntrada, usuario: int | None, ip: str | None) -> dict: ...
    def consultar(self, limite: int) -> list[dict]: ...


class RepositorioSqlAuditoria:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _filas(c: CursorSql) -> list[dict]:
        return [dict(zip((x[0] for x in c.description), f)) for f in c.fetchall()]

    def registrar(self, e: EventoEntrada, usuario: int | None, ip: str | None) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "INSERT INTO auditoria.evento (modulo, accion, entidad, id_entidad, detalle_json, id_usuario, direccion_ip) OUTPUT INSERTED.id_evento, INSERTED.modulo, INSERTED.accion, INSERTED.entidad, INSERTED.id_entidad, INSERTED.detalle_json, INSERTED.id_usuario, INSERTED.direccion_ip, INSERTED.creado_en VALUES (?, ?, ?, ?, ?, ?, ?)",
                e.modulo,
                e.accion,
                e.entidad,
                e.id_entidad,
                json.dumps(e.detalle, ensure_ascii=False),
                usuario,
                ip,
            )
            r = c.fetchone()
            if r is None:
                raise RuntimeError("No se pudo registrar la auditoría")
            d = dict(zip((x[0] for x in c.description), r))
            d["detalle"] = json.loads(cast(str, d.pop("detalle_json")))
            return d

    def consultar(self, limite: int) -> list[dict]:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "SELECT TOP (?) id_evento, modulo, accion, entidad, id_entidad, detalle_json, id_usuario, direccion_ip, creado_en FROM auditoria.evento ORDER BY creado_en DESC",
                limite,
            )
            out = self._filas(c)
            for d in out:
                d["detalle"] = json.loads(d.pop("detalle_json"))
            return out
