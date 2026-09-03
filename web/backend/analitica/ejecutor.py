"""Ejecutor pandas contra PostgreSQL, separado del proceso FastAPI."""

from __future__ import annotations

import argparse
import json
import os
from datetime import date, timedelta
from pathlib import Path

import pandas as pd
from sqlalchemy import create_engine, text

from .proyecciones import proyectar_asistencia


def cargar_datos(cadena: str, fecha_fin: date, dias: int) -> tuple[pd.DataFrame, pd.DataFrame, pd.DataFrame]:
    inicio = fecha_fin - timedelta(days=dias - 1)
    motor = create_engine(cadena, pool_pre_ping=True)
    with motor.connect() as conexion:
        marcas = pd.read_sql(text("""SELECT persona_id AS id_estudiante, fecha, 'presente' AS estado
            FROM ingreso_comedor WHERE fecha BETWEEN :inicio AND :fin"""), conexion, params={"inicio": inicio, "fin": fecha_fin})
        estudiantes = pd.read_sql(text("""SELECT p.id AS id_estudiante, CASE WHEN m.becado THEN 1 ELSE 2 END AS id_estado_comedor
            FROM persona p JOIN matricula m ON m.persona_id=p.id JOIN anio_lectivo a ON a.id=m.anio_lectivo_id
            WHERE p.tipo='estudiante' AND p.activo AND m.estado='activo' AND a.anio=:anio"""), conexion, params={"anio": fecha_fin.year})
        consumos = pd.read_sql(text("""SELECT i.persona_id AS id_estudiante, i.fecha,
            CASE WHEN i.consumio_tiquete THEN 'tiquete' ELSE 'beca' END AS modalidad
            FROM ingreso_comedor i WHERE i.fecha BETWEEN :inicio AND :fin"""), conexion, params={"inicio": inicio, "fin": fecha_fin})
    motor.dispose()
    return marcas, estudiantes, consumos


def ejecutar(cadena: str, fecha_fin: date, dias: int) -> list[dict]:
    resultado = proyectar_asistencia(*cargar_datos(cadena, fecha_fin, dias))
    motor = create_engine(cadena, pool_pre_ping=True)
    with motor.begin() as conexion:
        conexion.execute(text("DELETE FROM indicador_analitico_comedor WHERE fecha_corte=:fecha"), {"fecha": fecha_fin})
        for fila in resultado:
            parametros = fila | {"persona_id": fila["id_estudiante"], "fecha": fecha_fin}
            conexion.execute(text("""INSERT INTO indicador_analitico_comedor
                (persona_id,fecha_corte,dias_observados,dias_presentes,porcentaje_asistencia,consumos_comedor,consumos_tiquete,senal)
                VALUES (:persona_id,:fecha,:dias_observados,:dias_presentes,:porcentaje_asistencia,:consumos_comedor,:consumos_tiquete,:senal)"""), parametros)
    motor.dispose()
    return resultado


def main() -> int:
    parser = argparse.ArgumentParser(description="Genera señales históricas del comedor")
    parser.add_argument("--fecha-fin", default=date.today().isoformat())
    parser.add_argument("--dias", type=int, default=20)
    parser.add_argument("--salida", type=Path, required=True)
    args = parser.parse_args()
    cadena = os.environ.get("DATABASE_URL", "").strip()
    if not cadena:
        parser.error("DATABASE_URL es requerida")
    datos = ejecutar(cadena, date.fromisoformat(args.fecha_fin), args.dias)
    args.salida.write_text(json.dumps(datos, ensure_ascii=False, default=str, indent=2), encoding="utf-8")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
