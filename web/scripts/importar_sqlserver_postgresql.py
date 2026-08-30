#!/usr/bin/env python3
"""Importación única y auditable del estado activo 2026.

El modo predeterminado es simulación. No lee marcas, ventas, saldos,
credenciales ni auditoría del origen.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import json
import os
import secrets
import sys
from collections import Counter
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Iterable

import pyodbc
from argon2 import PasswordHasher
from sqlalchemy import create_engine, text


@dataclass(frozen=True)
class PersonaOrigen:
    id_origen: int
    cedula: str
    nombres: str
    tipo: str
    seccion: str | None
    turno: str | None
    becado: bool
    id_ruta: int | None


def _texto(valor: Any) -> str:
    return " ".join(str(valor or "").strip().split())


def _digito_verificador(numero: str) -> str:
    total = sum(int(d) * (3 if indice % 2 == 0 else 1) for indice, d in enumerate(numero))
    return str((10 - total % 10) % 10)


def _codigo(tipo: str, id_origen: int, semilla: str) -> str:
    material = f"{semilla}:{tipo}:{id_origen}".encode()
    numero = f"{int.from_bytes(hashlib.sha256(material).digest()[:8], 'big') % 10_000_000:07d}"
    return f"{'E' if tipo == 'estudiante' else 'P'}-{numero}{_digito_verificador(numero)}"


def _filas(cursor: pyodbc.Cursor, consulta: str) -> list[dict[str, Any]]:
    cursor.execute(consulta)
    columnas = [columna[0] for columna in cursor.description]
    return [dict(zip(columnas, fila, strict=True)) for fila in cursor.fetchall()]


def extraer(cadena: str) -> tuple[list[PersonaOrigen], list[dict[str, Any]], list[dict[str, Any]]]:
    with pyodbc.connect(cadena, autocommit=False, timeout=60) as conexion:
        cursor = conexion.cursor()
        usuarios = _filas(
            cursor,
            """
            SELECT IdUsuario, Cedula, Nombre, PrimerApellido, SegundoApellido,
                   CodTipo, Seccion, CONVERT(nvarchar(30), IdHorario) AS Turno,
                   TipoBeca, IdRuta
            FROM dbo.Usuario
            WHERE Activo=1 AND CodTipo IN (1,2)
            ORDER BY IdUsuario
            """,
        )
        rutas = _filas(
            cursor,
            """
            SELECT IdRuta, COALESCE(NULLIF(LTRIM(RTRIM(Descripcion)),''),
                   NULLIF(LTRIM(RTRIM(Codigo)),''), CONCAT('Ruta ',IdRuta)) AS Nombre
            FROM dbo.Ruta WHERE Activo=1 ORDER BY IdRuta
            """,
        )
        tablas = {fila[0] for fila in cursor.execute(
            "SELECT CONCAT(TABLE_SCHEMA,'.',TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES"
        ).fetchall()}
        menu: list[dict[str, Any]] = []
        if {"ComedorPortal.MenuPlantilla", "ComedorPortal.MenuComponente"} <= tablas:
            menu = _filas(
                cursor,
                """
                SELECT p.IdMenuPlantilla, p.SemanaMes, p.DiaSemana, p.Titulo,
                       p.Observaciones, c.Nombre AS Componente, c.Orden
                FROM ComedorPortal.MenuPlantilla p
                LEFT JOIN ComedorPortal.MenuComponente c
                  ON c.IdMenuPlantilla=p.IdMenuPlantilla
                WHERE p.Activo=1
                ORDER BY p.IdMenuPlantilla,c.Orden
                """,
            )
        conexion.rollback()

    personas = [
        PersonaOrigen(
            id_origen=int(fila["IdUsuario"]),
            cedula=_texto(fila["Cedula"]),
            nombres=_texto(" ".join(filter(None, [fila["Nombre"], fila["PrimerApellido"], fila["SegundoApellido"]]))),
            tipo="estudiante" if int(fila["CodTipo"]) == 1 else "profesor",
            seccion=_texto(fila["Seccion"]) or None,
            turno=_texto(fila["Turno"]) or None,
            becado=int(fila["TipoBeca"] or 0) == 2,
            id_ruta=int(fila["IdRuta"]) if fila["IdRuta"] is not None else None,
        )
        for fila in usuarios
    ]
    return personas, rutas, menu


def validar(personas: Iterable[PersonaOrigen], rutas: Iterable[dict[str, Any]]) -> dict[str, Any]:
    personas = list(personas)
    ids_ruta = {int(ruta["IdRuta"]) for ruta in rutas}
    cedulas = Counter(persona.cedula for persona in personas if persona.cedula)
    errores: list[dict[str, Any]] = []
    for persona in personas:
        if not persona.cedula:
            errores.append({"tipo": "cedula_ausente", "id_origen": persona.id_origen})
        elif cedulas[persona.cedula] > 1:
            errores.append({"tipo": "cedula_duplicada", "id_origen": persona.id_origen})
        if not persona.nombres:
            errores.append({"tipo": "nombre_ausente", "id_origen": persona.id_origen})
        if persona.tipo == "estudiante" and not persona.seccion:
            errores.append({"tipo": "seccion_ausente", "id_origen": persona.id_origen})
        if persona.tipo == "estudiante" and not persona.turno:
            errores.append({"tipo": "turno_ausente", "id_origen": persona.id_origen})
        if persona.id_ruta is not None and persona.id_ruta not in ids_ruta:
            errores.append({"tipo": "ruta_invalida", "id_origen": persona.id_origen})
    return {
        "personas_activas": len(personas),
        "estudiantes": sum(p.tipo == "estudiante" for p in personas),
        "profesores": sum(p.tipo == "profesor" for p in personas),
        "becados": sum(p.becado for p in personas if p.tipo == "estudiante"),
        "errores": errores,
    }


def aplicar(
    url: str,
    personas: list[PersonaOrigen],
    rutas: list[dict[str, Any]],
    menu: list[dict[str, Any]],
    semilla: str,
    credenciales: Path,
) -> dict[str, int]:
    motor = create_engine(url, pool_pre_ping=True)
    hasher = PasswordHasher()
    creadas: list[tuple[str, str, str]] = []
    conteos = Counter()
    ids_ruta: dict[int, int] = {}
    with motor.begin() as conexion:
        anio_id = conexion.execute(text(
            """INSERT INTO anio_lectivo(anio,vigente) VALUES (2026,true)
            ON CONFLICT(anio) DO UPDATE SET vigente=excluded.vigente RETURNING id"""
        )).scalar_one()
        for ruta in rutas:
            nombre = _texto(ruta["Nombre"])
            id_ruta = conexion.execute(text(
                """INSERT INTO ruta(nombre,activo) VALUES (:nombre,true)
                ON CONFLICT(nombre) DO UPDATE SET activo=true RETURNING id"""
            ), {"nombre": nombre}).scalar_one()
            ids_ruta[int(ruta["IdRuta"])] = id_ruta
            conteos["rutas"] += 1

        cedulas_invalidas = {e["id_origen"] for e in validar(personas, rutas)["errores"]}
        for persona in personas:
            if persona.id_origen in cedulas_invalidas:
                continue
            codigo = _codigo(persona.tipo, persona.id_origen, semilla)
            existente = conexion.execute(
                text("SELECT id,codigo,tipo FROM persona WHERE cedula=:cedula"), {"cedula": persona.cedula}
            ).mappings().first()
            if existente:
                if existente["tipo"] != persona.tipo:
                    raise ValueError(f"La persona origen {persona.id_origen} cambió de tipo")
                persona_id = existente["id"]
                conexion.execute(text(
                    "UPDATE persona SET nombres=:nombres,tipo=:tipo,activo=true WHERE id=:id"
                ), {"id": persona_id, "nombres": persona.nombres, "tipo": persona.tipo})
                conteos["personas_actualizadas"] += 1
            else:
                persona_id = conexion.execute(text(
                    """INSERT INTO persona(codigo,cedula,nombres,tipo,activo)
                    VALUES (:codigo,:cedula,:nombres,:tipo,true) RETURNING id"""
                ), {"codigo": codigo, "cedula": persona.cedula, "nombres": persona.nombres, "tipo": persona.tipo}).scalar_one()
                pin = f"{secrets.randbelow(1_000_000):06d}"
                conexion.execute(text(
                    "INSERT INTO credencial_portal(persona_id,pin_hash,cambio_obligatorio) VALUES (:id,:hash,true)"
                ), {"id": persona_id, "hash": hasher.hash(pin)})
                creadas.append((codigo, persona.cedula, pin))
                conteos["personas_creadas"] += 1

            if persona.tipo == "estudiante":
                matricula_id = conexion.execute(text(
                    """INSERT INTO matricula(persona_id,anio_lectivo_id,seccion,turno,becado,estado)
                    VALUES (:persona,:anio,:seccion,:turno,:becado,'activo')
                    ON CONFLICT(persona_id,anio_lectivo_id) DO UPDATE SET
                      seccion=excluded.seccion,turno=excluded.turno,becado=excluded.becado,estado='activo'
                    RETURNING id"""
                ), {"persona": persona_id, "anio": anio_id, "seccion": persona.seccion,
                    "turno": persona.turno, "becado": persona.becado}).scalar_one()
                if persona.id_ruta in ids_ruta:
                    parametros_ruta = {"matricula": matricula_id, "ruta": ids_ruta[persona.id_ruta]}
                    actualizado = conexion.execute(text(
                        "UPDATE asignacion_ruta SET ruta_id=:ruta WHERE matricula_id=:matricula AND fecha_fin IS NULL"
                    ), parametros_ruta)
                    if actualizado.rowcount == 0:
                        conexion.execute(text(
                            """INSERT INTO asignacion_ruta(matricula_id,ruta_id,fecha_inicio,fecha_fin)
                            VALUES (:matricula,:ruta,DATE '2026-01-01',NULL)"""
                        ), parametros_ruta)
                conteos["matriculas"] += 1

        plantillas: dict[int, int] = {}
        for fila in menu:
            legado = int(fila["IdMenuPlantilla"])
            if legado not in plantillas:
                nombre = _texto(fila["Titulo"]) or f"Semana {fila['SemanaMes']} día {fila['DiaSemana']}"
                plantilla_id = conexion.execute(text(
                    """INSERT INTO plantilla_menu(nombre,activo) VALUES (:nombre,true)
                    ON CONFLICT(nombre) DO UPDATE SET activo=true RETURNING id"""
                ), {"nombre": nombre}).scalar_one()
                plantillas[legado] = plantilla_id
                conteos["plantillas"] += 1
            if fila["Componente"]:
                conexion.execute(text(
                    """INSERT INTO componente_menu(plantilla_id,nombre,orden)
                    VALUES (:plantilla,:nombre,:orden)
                    ON CONFLICT(plantilla_id,orden) DO UPDATE SET nombre=excluded.nombre"""
                ), {"plantilla": plantillas[legado], "nombre": _texto(fila["Componente"]),
                    "orden": int(fila["Orden"] or 0)})
                conteos["componentes"] += 1

    credenciales.parent.mkdir(parents=True, exist_ok=True)
    with credenciales.open("w", encoding="utf-8", newline="") as archivo:
        escritor = csv.writer(archivo)
        escritor.writerow(["codigo", "cedula", "pin_temporal"])
        escritor.writerows(creadas)
    try:
        credenciales.chmod(0o600)
    except OSError:
        pass
    return dict(conteos)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--aplicar", action="store_true", help="confirma escritura atómica en PostgreSQL")
    parser.add_argument("--reporte", type=Path, default=Path("reporte-importacion-2026.json"))
    parser.add_argument("--credenciales", type=Path, default=Path("credenciales-2026.csv"))
    args = parser.parse_args()
    origen = os.getenv("SQL_SERVER_ORIGEN", "").strip()
    if not origen:
        parser.error("SQL_SERVER_ORIGEN es requerida")
    personas, rutas, menu = extraer(origen)
    reporte = validar(personas, rutas)
    reporte.update({"modo": "aplicar" if args.aplicar else "simulacion", "rutas": len(rutas),
                    "plantillas": len({m['IdMenuPlantilla'] for m in menu})})
    if args.aplicar:
        if reporte["errores"]:
            print("Hay errores bloqueantes; no se aplicó la importación.", file=sys.stderr)
            resultado = 2
        else:
            url = os.getenv("DATABASE_URL", "").strip()
            semilla = os.getenv("CODIGO_MIGRACION_SEMILLA", "").strip()
            if not url or len(semilla) < 32:
                parser.error("DATABASE_URL y CODIGO_MIGRACION_SEMILLA (mínimo 32 caracteres) son requeridas")
            reporte["aplicados"] = aplicar(url, personas, rutas, menu, semilla, args.credenciales)
            resultado = 0
    else:
        resultado = 0 if not reporte["errores"] else 2
    args.reporte.write_text(json.dumps(reporte, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(reporte, ensure_ascii=False, indent=2))
    return resultado


if __name__ == "__main__":
    raise SystemExit(main())
