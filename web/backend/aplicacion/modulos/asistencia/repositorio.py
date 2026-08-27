"""Persistencia aislada del esquema asistencia."""

from __future__ import annotations

from datetime import date
from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioAsistencia(Protocol):
    def listar(self, fecha: date) -> list[dict]: ...
    def registrar(self, datos: dict, id_usuario: int, ip: str) -> dict: ...
    def corregir(
        self, id_marca: int, estado: str, motivo: str, id_usuario: int, ip: str
    ) -> dict: ...


class RepositorioSqlAsistencia:
    """Implementación SQL que solo consulta y modifica ``asistencia``."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _fila(cursor: CursorSql) -> dict | None:
        fila = cursor.fetchone()
        if fila is None:
            return None
        return dict(zip((col[0] for col in cursor.description), fila))

    @classmethod
    def _filas(cls, cursor: CursorSql) -> list[dict]:
        return [
            dict(zip((col[0] for col in cursor.description), fila)) for fila in cursor.fetchall()
        ]

    def listar(self, fecha: date) -> list[dict]:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT id_marca, id_estudiante, fecha, estado, observacion, corregida
                FROM asistencia.marca WHERE fecha = ? ORDER BY id_estudiante, id_marca""",
                fecha,
            )
            return self._filas(cursor)

    def registrar(self, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """INSERT INTO asistencia.marca
                (id_estudiante, fecha, estado, observacion, creado_por, direccion_ip)
                OUTPUT INSERTED.id_marca, INSERTED.id_estudiante, INSERTED.fecha,
                    INSERTED.estado, INSERTED.observacion, INSERTED.corregida
                VALUES (?, ?, ?, ?, ?, ?)""",
                datos["id_estudiante"],
                datos["fecha"],
                datos["estado"],
                datos.get("observacion"),
                id_usuario,
                ip or "WEB",
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo registrar la marca")
            return resultado

    def corregir(self, id_marca: int, estado: str, motivo: str, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """UPDATE asistencia.marca SET estado = ?, corregida = 1,
                actualizado_por = ?, direccion_ip = ?, fecha_actualizacion = SYSUTCDATETIME()
                WHERE id_marca = ?""",
                estado,
                id_usuario,
                ip or "WEB",
                id_marca,
            )
            if cursor.rowcount == 0:
                raise ValueError("Marca no encontrada")
            cursor.execute(
                """INSERT INTO asistencia.correccion
                (id_marca, motivo, id_usuario, direccion_ip) VALUES (?, ?, ?, ?)""",
                id_marca,
                motivo,
                id_usuario,
                ip or "WEB",
            )
            cursor.execute(
                """SELECT id_marca, id_estudiante, fecha, estado, observacion, corregida
                FROM asistencia.marca WHERE id_marca = ?""",
                id_marca,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo leer la marca corregida")
            return resultado
