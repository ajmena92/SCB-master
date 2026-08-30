"""Ejecutor bajo demanda de indicadores históricos del comedor.

Se mantiene fuera del proceso FastAPI para no cargar pandas durante el arranque.
"""

from __future__ import annotations

import argparse
import json
import os
from datetime import date, timedelta
from pathlib import Path

import pandas as pd
from sqlalchemy import create_engine
from sqlalchemy.engine import URL

from aplicacion.nucleo.dialecto_sql_server import DialectoSqlServerCompatible

from .proyecciones import proyectar_asistencia


def cargar_datos(cadena: str, fecha_fin: date, dias: int) -> tuple[pd.DataFrame, pd.DataFrame, pd.DataFrame]:
    inicio = fecha_fin - timedelta(days=dias - 1)
    motor = create_engine(
        URL.create("mssql+pyodbc", query={"odbc_connect": cadena}),
        pool_pre_ping=True,
        dialect=DialectoSqlServerCompatible(),
    )
    with motor.connect() as conexion:
        marcas = pd.read_sql("""SELECT id_estudiante,fecha,estado FROM asistencia.marca
            WHERE fecha BETWEEN ? AND ?""", conexion, params=(inicio, fecha_fin))
        estudiantes = pd.read_sql("""SELECT id_estudiante,id_estado_comedor
            FROM comedor.persona
            WHERE tipo_persona='estudiante' AND id_estudiante IS NOT NULL""", conexion)
        consumos = pd.read_sql("""SELECT p.id_estudiante,i.fecha,i.modalidad
            FROM comedor.ingreso i JOIN comedor.persona p ON p.id_persona=i.id_persona
            WHERE p.tipo_persona='estudiante' AND i.fecha BETWEEN ? AND ?""", conexion, params=(inicio, fecha_fin))
    motor.dispose()
    return marcas, estudiantes, consumos


def ejecutar(cadena: str, fecha_fin: date, dias: int) -> list[dict]:
    return proyectar_asistencia(*cargar_datos(cadena, fecha_fin, dias))


def main() -> int:
    parser = argparse.ArgumentParser(description="Genera señales históricas del comedor")
    parser.add_argument("--fecha-fin", default=date.today().isoformat())
    parser.add_argument("--dias", type=int, default=20)
    parser.add_argument("--salida", type=Path, required=True)
    args = parser.parse_args()
    cadena = os.environ.get("SQL_CONNECTION_STRING", "").strip()
    if not cadena:
        parser.error("SQL_CONNECTION_STRING es requerida")
    datos = ejecutar(cadena, date.fromisoformat(args.fecha_fin), args.dias)
    args.salida.write_text(json.dumps(datos, ensure_ascii=False, default=str, indent=2), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
