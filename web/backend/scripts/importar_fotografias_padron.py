#!/usr/bin/env python3
"""Recupera fotografías históricas del padrón hacia PostgreSQL.

La fuente se conserva intacta. El nombre de cada archivo debe corresponder a
la cédula; se ignoran separadores y se usa una versión JPEG optimizada para el
carnet. Sin --aplicar el script solo calcula el resultado.
"""
from __future__ import annotations

import argparse
import os
import re
from pathlib import Path

import psycopg

from aplicacion.fotografias import FotografiaInvalida, preparar_fotografia

EXTENSIONES = {".jpg", ".jpeg", ".png", ".webp"}


def solo_digitos(valor: str) -> str:
    return "".join(re.findall(r"\d", valor))


def leer_secreto(ruta: str) -> str:
    return Path(ruta).read_text(encoding="utf-8").strip()


def conexion_postgresql() -> psycopg.Connection:
    password_file = os.getenv("POSTGRES_PASSWORD_FILE", "").strip()
    password = leer_secreto(password_file) if password_file else os.getenv("POSTGRES_PASSWORD", "")
    return psycopg.connect(
        host=os.getenv("POSTGRES_HOST", "localhost"),
        port=os.getenv("POSTGRES_PORT", "5432"),
        dbname=os.getenv("POSTGRES_DB", "scb"),
        user=os.getenv("POSTGRES_USER", "scb_migrador"),
        password=password,
    )


def optimizar(ruta: Path) -> bytes:
    return preparar_fotografia(ruta.read_bytes())


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--carpeta", type=Path, required=True, help="carpeta histórica de fotografías")
    parser.add_argument("--aplicar", action="store_true", help="persiste las fotografías recuperadas")
    parser.add_argument(
        "--reemplazar",
        action="store_true",
        help="reemplaza una fotografía ya cargada; por defecto la conserva",
    )
    args = parser.parse_args()
    if not args.carpeta.is_dir():
        raise SystemExit("La carpeta indicada no existe o no es accesible")

    resumen = {"archivos": 0, "asociadas": 0, "sin_persona": 0, "invalidas": 0, "existentes": 0}
    with conexion_postgresql() as conexion, conexion.cursor() as cursor:
        cursor.execute("SELECT id, cedula FROM persona WHERE cedula IS NOT NULL")
        personas = {solo_digitos(cedula): persona_id for persona_id, cedula in cursor.fetchall()}
        for ruta in sorted(args.carpeta.iterdir(), key=lambda archivo: archivo.name.lower()):
            if not ruta.is_file() or ruta.suffix.lower() not in EXTENSIONES:
                continue
            resumen["archivos"] += 1
            persona_id = personas.get(solo_digitos(ruta.stem))
            if persona_id is None:
                resumen["sin_persona"] += 1
                continue
            try:
                contenido = optimizar(ruta)
            except (FotografiaInvalida, OSError):
                resumen["invalidas"] += 1
                continue
            if args.reemplazar:
                sentencia = """
                    INSERT INTO fotografia_persona (persona_id, contenido, tipo_contenido)
                    VALUES (%s, %s, 'image/jpeg')
                    ON CONFLICT (persona_id) DO UPDATE
                    SET contenido = EXCLUDED.contenido, tipo_contenido = EXCLUDED.tipo_contenido
                """
            else:
                sentencia = """
                    INSERT INTO fotografia_persona (persona_id, contenido, tipo_contenido)
                    VALUES (%s, %s, 'image/jpeg')
                    ON CONFLICT (persona_id) DO NOTHING
                """
            if args.aplicar:
                cursor.execute(sentencia, (persona_id, contenido))
                if cursor.rowcount == 0:
                    resumen["existentes"] += 1
                else:
                    resumen["asociadas"] += 1
            else:
                resumen["asociadas"] += 1
        if args.aplicar:
            conexion.commit()
        else:
            conexion.rollback()
    modo = "aplicado" if args.aplicar else "simulación"
    print(f"Recuperación de fotografías ({modo}): {resumen}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
