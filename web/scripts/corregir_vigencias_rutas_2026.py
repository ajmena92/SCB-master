#!/usr/bin/env python3
"""Cierra rutas solapadas usando rutas_estudiants.xlsx como fuente primaria."""
from __future__ import annotations

import argparse
import json
import os
from datetime import date, timedelta
from pathlib import Path

from sqlalchemy import create_engine, text

from sincronizar_estudiantes_2026 import leer_rutas, validar_rutas, validar_padron, leer_padron


def corregir(url: str, padron: set[str], rutas: dict[str, str], fecha: date, aplicar: bool) -> dict:
    cierre = fecha - timedelta(days=1)
    motor = create_engine(url, pool_pre_ping=True)
    resultado = {"estudiantes_regulares": len(padron), "rutas_fuente": len(rutas), "solapamientos": 0, "cierres": 0, "errores": []}
    with motor.begin() as conexion:
        conexion.execute(text("SELECT pg_advisory_xact_lock(:bloqueo)"), {"bloqueo": fecha.year})
        for cedula, codigo_origen in rutas.items():
            filas = conexion.execute(text("""
                SELECT ar.id, ar.fecha_inicio, r.codigo
                FROM asignacion_ruta ar
                JOIN matricula m ON m.id=ar.matricula_id
                JOIN persona p ON p.id=m.persona_id
                JOIN anio_lectivo al ON al.id=m.anio_lectivo_id
                JOIN ruta r ON r.id=ar.ruta_id
                WHERE p.cedula=:cedula AND p.activo=true AND p.tipo='estudiante'
                  AND al.anio=:anio AND m.estado='activo'
                  AND ar.fecha_inicio<=:fecha AND (ar.fecha_fin IS NULL OR ar.fecha_fin>=:fecha)
                ORDER BY ar.fecha_inicio DESC, ar.id DESC
            """), {"cedula": cedula, "anio": fecha.year, "fecha": fecha}).mappings().all()
            if len(filas) <= 1:
                continue
            esperados = {codigo_origen, f"RUTA-{codigo_origen}"}
            vigente = next((fila for fila in filas if fila["codigo"] in esperados), None)
            if vigente is None:
                resultado["errores"].append("ruta_fuente_no_encontrada")
                continue
            for fila in filas:
                if fila["id"] == vigente["id"]:
                    continue
                resultado["solapamientos"] += 1
                if fila["fecha_inicio"] > cierre:
                    resultado["errores"].append("vigencia_imposible_de_cerrar")
                    continue
                if aplicar:
                    cambio = conexion.execute(text("UPDATE asignacion_ruta SET fecha_fin=:cierre WHERE id=:id"), {"cierre": cierre, "id": fila["id"]})
                    resultado["cierres"] += cambio.rowcount
    return resultado


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("padron", type=Path)
    parser.add_argument("rutas", type=Path)
    parser.add_argument("--fecha", type=date.fromisoformat, required=True)
    parser.add_argument("--aplicar", action="store_true")
    args = parser.parse_args()
    padron, errores_padron = validar_padron(leer_padron(args.padron))
    rutas, errores_rutas, _ = validar_rutas(leer_rutas(args.rutas), set(padron))
    if errores_padron or errores_rutas or not os.getenv("DATABASE_URL"):
        raise SystemExit(2)
    resultado = corregir(os.environ["DATABASE_URL"], set(padron), rutas, args.fecha, args.aplicar)
    print(json.dumps(resultado, ensure_ascii=False))
    return 2 if resultado["errores"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
