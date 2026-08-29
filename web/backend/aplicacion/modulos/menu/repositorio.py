from typing import Protocol

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioMenu(Protocol):
    def listar(self) -> list[dict]: ...
    def guardar(self, datos: dict, usuario: int) -> dict: ...
    def listar_sustituciones(self) -> list[dict]: ...
    def guardar_sustitucion(self, datos: dict, usuario: int) -> dict: ...


class RepositorioSqlMenu:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _fila(c) -> dict:
        fila = c.fetchone()
        if fila is None:
            raise RuntimeError("No se obtuvo el menú")
        return dict(zip((x[0] for x in c.description), fila))

    def listar(self) -> list[dict]:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                "SELECT id_plantilla, semana, dia, titulo, observaciones, activo FROM menu.plantilla ORDER BY semana,dia"
            )
            filas = [dict(zip((x[0] for x in c.description), f)) for f in c.fetchall()]
            for f in filas:
                c.execute(
                    "SELECT nombre,tipo,orden FROM menu.componente WHERE id_plantilla=? ORDER BY orden",
                    f["id_plantilla"],
                )
                f["componentes"] = [
                    dict(zip((x[0] for x in c.description), v)) for v in c.fetchall()
                ]
            return filas

    def guardar(self, datos: dict, usuario: int) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                """MERGE menu.plantilla AS t USING (SELECT ? semana, ? dia) s ON t.semana=s.semana AND t.dia=s.dia
            WHEN MATCHED THEN UPDATE SET titulo=?,observaciones=?,activo=?,actualizado_por=?
            WHEN NOT MATCHED THEN INSERT(semana,dia,titulo,observaciones,activo,creado_por) VALUES(?,?,?,?,?,?)
            OUTPUT INSERTED.id_plantilla,INSERTED.semana,INSERTED.dia,INSERTED.titulo,INSERTED.observaciones,INSERTED.activo;""",
                datos["semana"],
                datos["dia"],
                datos["titulo"],
                datos.get("observaciones"),
                datos["activo"],
                usuario,
                datos["semana"],
                datos["dia"],
                datos["titulo"],
                datos.get("observaciones"),
                datos["activo"],
                usuario,
            )
            out = self._fila(c)
            c.execute("DELETE FROM menu.componente WHERE id_plantilla=?", out["id_plantilla"])
            for x in datos.get("componentes", []):
                c.execute(
                    "INSERT menu.componente(id_plantilla,nombre,tipo,orden) VALUES(?,?,?,?)",
                    out["id_plantilla"],
                    x["nombre"],
                    x["tipo"],
                    x["orden"],
                )
            out["componentes"] = datos.get("componentes", [])
            return out

    def listar_sustituciones(self) -> list[dict]:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                """SELECT id_sustitucion,fecha,titulo,observaciones
                FROM menu.sustitucion ORDER BY fecha"""
            )
            filas = [dict(zip((x[0] for x in c.description), f)) for f in c.fetchall()]
            for fila in filas:
                c.execute(
                    """SELECT nombre,tipo,orden FROM menu.componente_sustitucion
                    WHERE id_sustitucion=? ORDER BY orden""",
                    fila["id_sustitucion"],
                )
                fila["componentes"] = [
                    dict(zip((x[0] for x in c.description), componente))
                    for componente in c.fetchall()
                ]
            return filas

    def guardar_sustitucion(self, datos: dict, usuario: int) -> dict:
        with self._fabrica.conexion() as cn:
            c = cn.cursor()
            c.execute(
                """MERGE menu.sustitucion AS destino
                USING (SELECT ? AS fecha) AS origen
                ON destino.fecha=origen.fecha
                WHEN MATCHED THEN UPDATE SET titulo=?,observaciones=?,actualizado_por=?
                WHEN NOT MATCHED THEN
                    INSERT(fecha,titulo,observaciones,creado_por)
                    VALUES(?,?,?,?)
                OUTPUT INSERTED.id_sustitucion,INSERTED.fecha,
                       INSERTED.titulo,INSERTED.observaciones;""",
                datos["fecha"],
                datos["titulo"],
                datos.get("observaciones"),
                usuario,
                datos["fecha"],
                datos["titulo"],
                datos.get("observaciones"),
                usuario,
            )
            salida = self._fila(c)
            c.execute(
                "DELETE FROM menu.componente_sustitucion WHERE id_sustitucion=?",
                salida["id_sustitucion"],
            )
            for componente in datos.get("componentes", []):
                c.execute(
                    """INSERT menu.componente_sustitucion
                    (id_sustitucion,nombre,tipo,orden) VALUES(?,?,?,?)""",
                    salida["id_sustitucion"],
                    componente["nombre"],
                    componente["tipo"],
                    componente["orden"],
                )
            salida["componentes"] = datos.get("componentes", [])
            return salida
