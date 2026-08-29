from datetime import date
from typing import Protocol

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioParametros(Protocol):
    def obtener(self) -> dict: ...
    def guardar(
        self,
        minutos: int,
        horarios: list[dict],
        permitir_marca_tardia: bool = False,
        permitir_sin_marca_transporte: bool = True,
    ) -> dict: ...
    def calendario(self, anio: int, mes: int) -> list[dict]: ...


class RepositorioSqlParametros:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def obtener(self) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT minutos_aviso_previo,permitir_marca_tardia,
                permitir_sin_marca_transporte FROM comedor.parametro WHERE id_parametro=1"""
            )
            fila = cursor.fetchone()
            cursor.execute(
                """SELECT id_horario,codigo,descripcion,CONVERT(varchar(5),hora_limite,108),activo
                FROM comedor.horario_operacion ORDER BY id_horario"""
            )
            horarios = [
                {
                    "id_horario": r[0],
                    "codigo": r[1],
                    "descripcion": r[2],
                    "hora_limite": r[3],
                    "activo": bool(r[4]),
                }
                for r in cursor.fetchall()
            ]
        return {
            "minutos_aviso_previo": int(str(fila[0])) if fila else 15,
            "permitir_marca_tardia": bool(fila[1]) if fila else False,
            "permitir_sin_marca_transporte": bool(fila[2]) if fila else True,
            "horarios": horarios,
        }

    def guardar(
        self,
        minutos: int,
        horarios: list[dict],
        permitir_marca_tardia: bool = False,
        permitir_sin_marca_transporte: bool = True,
    ) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "MERGE comedor.parametro AS t USING (SELECT 1 id_parametro, ? minutos_aviso_previo) s "
                "ON t.id_parametro=s.id_parametro WHEN MATCHED THEN UPDATE SET minutos_aviso_previo=s.minutos_aviso_previo "
                "WHEN NOT MATCHED THEN INSERT(id_parametro,minutos_aviso_previo,permitir_marca_tardia,permitir_sin_marca_transporte,actualizado_en) VALUES(1,?,?,?,SYSUTCDATETIME());",
                minutos,
                minutos,
                permitir_marca_tardia,
                permitir_sin_marca_transporte,
            )
            cursor.execute(
                """UPDATE comedor.parametro SET permitir_marca_tardia=?,
                permitir_sin_marca_transporte=?,actualizado_en=SYSUTCDATETIME()
                WHERE id_parametro=1""",
                permitir_marca_tardia,
                permitir_sin_marca_transporte,
            )
            for horario in horarios:
                cursor.execute(
                    """UPDATE comedor.horario_operacion SET hora_limite=?,actualizado_en=SYSUTCDATETIME()
                    WHERE id_horario=? AND activo=1""",
                    horario["hora_limite"],
                    horario["id_horario"],
                )
            cursor.execute(
                """SELECT id_horario,codigo,descripcion,CONVERT(varchar(5),hora_limite,108),activo
                FROM comedor.horario_operacion ORDER BY id_horario"""
            )
            filas = cursor.fetchall()
            cursor.execute(
                """SELECT permitir_marca_tardia,permitir_sin_marca_transporte
                FROM comedor.parametro WHERE id_parametro=1"""
            )
            politicas = cursor.fetchone()
        return {
            "minutos_aviso_previo": minutos,
            "permitir_marca_tardia": bool(politicas[0]) if politicas else False,
            "permitir_sin_marca_transporte": bool(politicas[1]) if politicas else True,
            "horarios": [
                {
                    "id_horario": r[0],
                    "codigo": r[1],
                    "descripcion": r[2],
                    "hora_limite": r[3],
                    "activo": bool(r[4]),
                }
                for r in filas
            ],
        }

    def calendario(self, anio: int, mes: int) -> list[dict]:
        inicio = date(anio, mes, 1)
        fin = date(anio + (mes == 12), (mes % 12) + 1, 1)
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT fecha,habilitado FROM menu.calendario WHERE fecha>=? AND fecha<? ORDER BY fecha",
                inicio,
                fin,
            )
            return [{"fecha": fila[0], "habilitado": bool(fila[1])} for fila in cursor.fetchall()]
