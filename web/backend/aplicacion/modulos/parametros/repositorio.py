from datetime import date
from typing import Protocol

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioParametros(Protocol):
    def obtener(self) -> dict: ...
    def guardar(self, minutos: int) -> dict: ...
    def calendario(self, anio: int, mes: int) -> list[dict]: ...


class RepositorioSqlParametros:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def obtener(self) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT minutos_aviso_previo FROM comedor.parametro WHERE id_parametro=1")
            fila = cursor.fetchone()
        return {"minutos_aviso_previo": int(str(fila[0])) if fila else 15}

    def guardar(self, minutos: int) -> dict:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "MERGE comedor.parametro AS t USING (SELECT 1 id_parametro, ? minutos_aviso_previo) s "
                "ON t.id_parametro=s.id_parametro WHEN MATCHED THEN UPDATE SET minutos_aviso_previo=s.minutos_aviso_previo "
                "WHEN NOT MATCHED THEN INSERT(id_parametro,minutos_aviso_previo,actualizado_en) VALUES(1,?,SYSUTCDATETIME());",
                minutos, minutos,
            )
        return {"minutos_aviso_previo": minutos}

    def calendario(self, anio: int, mes: int) -> list[dict]:
        inicio = date(anio, mes, 1)
        fin = date(anio + (mes == 12), (mes % 12) + 1, 1)
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT fecha,habilitado FROM menu.calendario WHERE fecha>=? AND fecha<? ORDER BY fecha", inicio, fin)
            return [{"fecha": fila[0], "habilitado": bool(fila[1])} for fila in cursor.fetchall()]
