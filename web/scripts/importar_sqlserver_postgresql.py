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
from datetime import date
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


def extraer(
    cadena: str,
) -> tuple[list[PersonaOrigen], list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]], list[dict[str, Any]]]:
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
        tablas = {
            fila[0]
            for fila in cursor.execute(
                "SELECT CONCAT(TABLE_SCHEMA,'.',TABLE_NAME) FROM INFORMATION_SCHEMA.TABLES"
            ).fetchall()
        }
        menu: list[dict[str, Any]] = []
        calendario: list[dict[str, Any]] = []
        sustituciones: list[dict[str, Any]] = []
        if {"menu.plantilla", "menu.componente"} <= tablas:
            menu = _filas(
                cursor,
                """
                SELECT p.id_plantilla AS IdMenuPlantilla, p.semana AS SemanaMes, p.dia AS DiaSemana,
                       p.titulo AS Titulo, p.observaciones AS Observaciones, p.activo AS Activo,
                       c.nombre AS Componente, c.tipo AS TipoComponente, c.orden AS Orden
                FROM menu.plantilla p LEFT JOIN menu.componente c ON c.id_plantilla=p.id_plantilla
                WHERE p.activo=1 ORDER BY p.semana,p.dia,c.orden
                """,
            )
        elif {"ComedorPortal.MenuPlantilla", "ComedorPortal.MenuComponente"} <= tablas:
            menu = _filas(
                cursor,
                """
                SELECT p.IdMenuPlantilla, p.SemanaMes, p.DiaSemana, p.Titulo,
                       p.Observaciones, p.Activo, c.Nombre AS Componente,
                       c.TipoComponente, c.Orden
                FROM ComedorPortal.MenuPlantilla p
                LEFT JOIN ComedorPortal.MenuComponente c
                  ON c.IdMenuPlantilla=p.IdMenuPlantilla
                WHERE p.Activo=1
                ORDER BY p.IdMenuPlantilla,c.Orden
                """,
            )
        if "menu.calendario" in tablas:
            calendario = _filas(cursor, "SELECT fecha AS Fecha, habilitado AS Habilitado FROM menu.calendario")
        if {"menu.sustitucion", "menu.componente_sustitucion"} <= tablas:
            sustituciones = _filas(
                cursor,
                """
                SELECT s.fecha AS Fecha, s.titulo AS Titulo, s.observaciones AS Observaciones,
                       c.nombre AS Componente, c.tipo AS TipoComponente, c.orden AS Orden
                FROM menu.sustitucion s
                LEFT JOIN menu.componente_sustitucion c ON c.id_sustitucion=s.id_sustitucion
                ORDER BY s.fecha,c.orden
                """,
            )
        elif {"ComedorPortal.MenuSustitucion", "ComedorPortal.MenuSustitucionComponente"} <= tablas:
            sustituciones = _filas(
                cursor,
                """
                SELECT s.Fecha, s.Titulo, s.Observaciones, c.Nombre AS Componente,
                       c.TipoComponente, c.Orden
                FROM ComedorPortal.MenuSustitucion s
                LEFT JOIN ComedorPortal.MenuSustitucionComponente c
                  ON c.IdMenuSustitucion=s.IdMenuSustitucion
                ORDER BY s.Fecha,c.Orden
                """,
            )
        conexion.rollback()

    personas = [
        PersonaOrigen(
            id_origen=int(fila["IdUsuario"]),
            cedula=_texto(fila["Cedula"]),
            nombres=_texto(
                " ".join(
                    filter(None, [fila["Nombre"], fila["PrimerApellido"], fila["SegundoApellido"]])
                )
            ),
            tipo="estudiante" if int(fila["CodTipo"]) == 1 else "profesor",
            seccion=_texto(fila["Seccion"]) or None,
            turno=_texto(fila["Turno"]) or None,
            becado=int(fila["TipoBeca"] or 0) == 2,
            id_ruta=int(fila["IdRuta"]) if fila["IdRuta"] is not None else None,
        )
        for fila in usuarios
    ]
    return personas, rutas, menu, calendario, sustituciones


def validar(
    personas: Iterable[PersonaOrigen],
    rutas: Iterable[dict[str, Any]],
    menu: Iterable[dict[str, Any]] = (),
) -> dict[str, Any]:
    personas = list(personas)
    menu = list(menu)
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
    for fila in menu:
        titulo = _texto(fila.get("Titulo"))
        componente = _texto(fila.get("Componente"))
        if len(titulo) > 180:
            errores.append(
                {
                    "tipo": "titulo_menu_muy_largo",
                    "id_origen": int(fila["IdMenuPlantilla"]),
                    "longitud": len(titulo),
                }
            )
        if len(componente) > 180:
            errores.append(
                {
                    "tipo": "componente_menu_muy_largo",
                    "id_origen": int(fila["IdMenuPlantilla"]),
                    "longitud": len(componente),
                }
            )
    return {
        "personas_activas": len(personas),
        "estudiantes": sum(p.tipo == "estudiante" for p in personas),
        "profesores": sum(p.tipo == "profesor" for p in personas),
        "becados": sum(p.becado for p in personas if p.tipo == "estudiante"),
        "errores": errores,
    }


def validar_sustituciones(sustituciones: Iterable[dict[str, Any]]) -> dict[str, Any]:
    """Valida el contenido que puede copiarse sin tocar el padrón ni el menú base."""
    errores: list[dict[str, Any]] = []
    por_fecha: dict[Any, list[dict[str, Any]]] = {}
    for fila in sustituciones:
        fecha = fila.get("Fecha")
        por_fecha.setdefault(fecha, []).append(fila)
        titulo = _texto(fila.get("Titulo"))
        componente = _texto(fila.get("Componente"))
        if not fecha:
            errores.append({"tipo": "fecha_sustitucion_ausente"})
        if not titulo:
            errores.append({"tipo": "titulo_sustitucion_ausente", "fecha": str(fecha)})
        elif len(titulo) > 180:
            errores.append({"tipo": "titulo_sustitucion_muy_largo", "fecha": str(fecha)})
        if componente and len(componente) > 180:
            errores.append({"tipo": "componente_sustitucion_muy_largo", "fecha": str(fecha)})

    for fecha, filas in por_fecha.items():
        ordenes = [int(fila["Orden"]) for fila in filas if fila.get("Componente") and fila.get("Orden")]
        if len(ordenes) != len(set(ordenes)):
            errores.append({"tipo": "orden_componente_sustitucion_duplicado", "fecha": str(fecha)})
    return {
        "sustituciones": len(por_fecha),
        "componentes_sustitucion": sum(1 for fila in sustituciones if _texto(fila.get("Componente"))),
        "errores": errores,
    }


def aplicar_sustituciones(url: str, sustituciones: list[dict[str, Any]]) -> dict[str, int]:
    """Inserta sustituciones históricas sin reemplazar decisiones ya tomadas en PostgreSQL."""
    motor = create_engine(url, pool_pre_ping=True)
    por_fecha: dict[Any, list[dict[str, Any]]] = {}
    for fila in sustituciones:
        por_fecha.setdefault(fila["Fecha"], []).append(fila)

    conteos = Counter()
    with motor.begin() as conexion:
        for fecha, filas in por_fecha.items():
            origen = {
                "titulo": _texto(filas[0]["Titulo"]),
                "observaciones": _texto(filas[0].get("Observaciones")) or None,
                "componentes": [
                    (
                        _texto(fila["Componente"]),
                        _texto(fila.get("TipoComponente")) or "Principal",
                        int(fila["Orden"] or 1),
                    )
                    for fila in filas
                    if _texto(fila.get("Componente"))
                ],
            }
            existente = conexion.execute(
                text("SELECT id,titulo,observaciones FROM sustitucion_menu WHERE fecha=:fecha"),
                {"fecha": fecha},
            ).mappings().first()
            if existente:
                componentes_existentes = [
                    (fila["nombre"], fila["tipo"], fila["orden"])
                    for fila in conexion.execute(
                        text(
                            """SELECT nombre,tipo,orden FROM componente_sustitucion_menu
                            WHERE sustitucion_id=:id ORDER BY orden"""
                        ),
                        {"id": existente["id"]},
                    ).mappings()
                ]
                misma_sustitucion = (
                    existente["titulo"] == origen["titulo"]
                    and existente["observaciones"] == origen["observaciones"]
                    and componentes_existentes == origen["componentes"]
                )
                if misma_sustitucion:
                    conteos["sustituciones_ya_importadas"] += 1
                    continue
                raise ValueError(
                    f"Conflicto en sustitución {fecha}: ya existe una versión diferente en PostgreSQL"
                )

            sustitucion_id = conexion.execute(
                text(
                    """INSERT INTO sustitucion_menu(fecha,titulo,observaciones)
                    VALUES (:fecha,:titulo,:observaciones) RETURNING id"""
                ),
                {"fecha": fecha, **origen},
            ).scalar_one()
            for nombre, tipo, orden in origen["componentes"]:
                conexion.execute(
                    text(
                        """INSERT INTO componente_sustitucion_menu(sustitucion_id,nombre,tipo,orden)
                        VALUES (:id,:nombre,:tipo,:orden)"""
                    ),
                    {"id": sustitucion_id, "nombre": nombre, "tipo": tipo, "orden": orden},
                )
                conteos["componentes_sustitucion"] += 1
            conteos["sustituciones_importadas"] += 1
    return dict(conteos)


def aplicar(
    url: str,
    personas: list[PersonaOrigen],
    rutas: list[dict[str, Any]],
    menu: list[dict[str, Any]],
    calendario: list[dict[str, Any]],
    sustituciones: list[dict[str, Any]],
    semilla: str,
    credenciales: Path,
) -> dict[str, int]:
    motor = create_engine(url, pool_pre_ping=True)
    hasher = PasswordHasher()
    creadas: list[tuple[str, str, str]] = []
    conteos = Counter()
    ids_ruta: dict[int, int] = {}
    with motor.begin() as conexion:
        anio_id = conexion.execute(
            text(
                """INSERT INTO anio_lectivo(anio,vigente) VALUES (2026,true)
            ON CONFLICT(anio) DO UPDATE SET vigente=excluded.vigente RETURNING id"""
            )
        ).scalar_one()
        for ruta in rutas:
            nombre = _texto(ruta["Nombre"])
            id_ruta = conexion.execute(
                text(
                    """INSERT INTO ruta(nombre,activo) VALUES (:nombre,true)
                ON CONFLICT(nombre) DO UPDATE SET activo=true RETURNING id"""
                ),
                {"nombre": nombre},
            ).scalar_one()
            ids_ruta[int(ruta["IdRuta"])] = id_ruta
            conteos["rutas"] += 1

        cedulas_invalidas = {
            e["id_origen"]
            for e in validar(personas, rutas, menu)["errores"]
            if e["tipo"].startswith(("cedula_", "nombre_", "seccion_", "turno_", "ruta_"))
        }
        for persona in personas:
            if persona.id_origen in cedulas_invalidas:
                continue
            codigo = _codigo(persona.tipo, persona.id_origen, semilla)
            existente = (
                conexion.execute(
                    text("SELECT id,codigo,tipo FROM persona WHERE cedula=:cedula"),
                    {"cedula": persona.cedula},
                )
                .mappings()
                .first()
            )
            if existente:
                if existente["tipo"] != persona.tipo:
                    raise ValueError(f"La persona origen {persona.id_origen} cambió de tipo")
                persona_id = existente["id"]
                conexion.execute(
                    text("UPDATE persona SET nombres=:nombres,tipo=:tipo,activo=true WHERE id=:id"),
                    {"id": persona_id, "nombres": persona.nombres, "tipo": persona.tipo},
                )
                conteos["personas_actualizadas"] += 1
            else:
                persona_id = conexion.execute(
                    text(
                        """INSERT INTO persona(codigo,cedula,nombres,tipo,activo)
                    VALUES (:codigo,:cedula,:nombres,:tipo,true) RETURNING id"""
                    ),
                    {
                        "codigo": codigo,
                        "cedula": persona.cedula,
                        "nombres": persona.nombres,
                        "tipo": persona.tipo,
                    },
                ).scalar_one()
                pin = f"{secrets.randbelow(1_000_000):06d}"
                conexion.execute(
                    text(
                        "INSERT INTO credencial_portal(persona_id,pin_hash,cambio_obligatorio) VALUES (:id,:hash,true)"
                    ),
                    {"id": persona_id, "hash": hasher.hash(pin)},
                )
                creadas.append((codigo, persona.cedula, pin))
                conteos["personas_creadas"] += 1

            if persona.tipo == "estudiante":
                matricula_id = conexion.execute(
                    text(
                        """INSERT INTO matricula(persona_id,anio_lectivo_id,seccion,turno,becado,estado)
                    VALUES (:persona,:anio,:seccion,:turno,:becado,'activo')
                    ON CONFLICT(persona_id,anio_lectivo_id) DO UPDATE SET
                      seccion=excluded.seccion,turno=excluded.turno,becado=excluded.becado,estado='activo'
                    RETURNING id"""
                    ),
                    {
                        "persona": persona_id,
                        "anio": anio_id,
                        "seccion": persona.seccion,
                        "turno": persona.turno,
                        "becado": persona.becado,
                    },
                ).scalar_one()
                if persona.id_ruta in ids_ruta:
                    parametros_ruta = {"matricula": matricula_id, "ruta": ids_ruta[persona.id_ruta]}
                    actualizado = conexion.execute(
                        text(
                            "UPDATE asignacion_ruta SET ruta_id=:ruta WHERE matricula_id=:matricula AND fecha_fin IS NULL"
                        ),
                        parametros_ruta,
                    )
                    if actualizado.rowcount == 0:
                        conexion.execute(
                            text(
                                """INSERT INTO asignacion_ruta(matricula_id,ruta_id,fecha_inicio,fecha_fin)
                            VALUES (:matricula,:ruta,DATE '2026-01-01',NULL)"""
                            ),
                            parametros_ruta,
                        )
                conteos["matriculas"] += 1

        plantillas: dict[int, int] = {}
        for fila in menu:
            legado = int(fila["IdMenuPlantilla"])
            if legado not in plantillas:
                titulo = _texto(fila["Titulo"]) or f"Semana {fila['SemanaMes']} día {fila['DiaSemana']}"
                plantilla_id = conexion.execute(
                    text(
                        """INSERT INTO plantilla_menu(semana,dia,titulo,observaciones,activo)
                        VALUES (:semana,:dia,:titulo,:observaciones,true)
                        ON CONFLICT(semana,dia) DO UPDATE SET titulo=excluded.titulo,
                          observaciones=excluded.observaciones,activo=true RETURNING id"""
                    ),
                    {"semana": int(fila["SemanaMes"]), "dia": int(fila["DiaSemana"]),
                     "titulo": titulo, "observaciones": _texto(fila.get("Observaciones")) or None},
                ).scalar_one()
                plantillas[legado] = plantilla_id
                conteos["plantillas"] += 1
            if fila["Componente"]:
                conexion.execute(
                    text(
                        """INSERT INTO componente_menu(plantilla_id,nombre,tipo,orden)
                    VALUES (:plantilla,:nombre,:tipo,:orden)
                    ON CONFLICT(plantilla_id,orden) DO UPDATE SET nombre=excluded.nombre,tipo=excluded.tipo"""
                    ),
                    {
                        "plantilla": plantillas[legado],
                        "nombre": _texto(fila["Componente"]),
                        "tipo": _texto(fila.get("TipoComponente")) or "Principal",
                        "orden": int(fila["Orden"] or 1),
                    },
                )
                conteos["componentes"] += 1

        for fila in calendario:
            conexion.execute(
                text("""INSERT INTO calendario_menu(fecha,habilitado) VALUES (:fecha,:habilitado)
                ON CONFLICT(fecha) DO UPDATE SET habilitado=excluded.habilitado"""),
                {"fecha": fila["Fecha"], "habilitado": bool(fila["Habilitado"])},
            )
            conteos["dias_calendario"] += 1

        sustituciones_por_fecha: dict[Any, int] = {}
        for fila in sustituciones:
            fecha = fila["Fecha"]
            if fecha not in sustituciones_por_fecha:
                sustitucion_id = conexion.execute(
                    text("""INSERT INTO sustitucion_menu(fecha,titulo,observaciones)
                    VALUES (:fecha,:titulo,:observaciones)
                    ON CONFLICT(fecha) DO UPDATE SET titulo=excluded.titulo,
                    observaciones=excluded.observaciones RETURNING id"""),
                    {"fecha": fecha, "titulo": _texto(fila["Titulo"]),
                     "observaciones": _texto(fila.get("Observaciones")) or None},
                ).scalar_one()
                sustituciones_por_fecha[fecha] = sustitucion_id
                conexion.execute(text("DELETE FROM componente_sustitucion_menu WHERE sustitucion_id=:id"), {"id": sustitucion_id})
                conteos["sustituciones"] += 1
            if fila.get("Componente"):
                conexion.execute(
                    text("""INSERT INTO componente_sustitucion_menu(sustitucion_id,nombre,tipo,orden)
                    VALUES (:id,:nombre,:tipo,:orden)"""),
                    {"id": sustituciones_por_fecha[fecha], "nombre": _texto(fila["Componente"]),
                     "tipo": _texto(fila.get("TipoComponente")) or "Principal", "orden": int(fila["Orden"] or 1)},
                )
                conteos["componentes_sustitucion"] += 1

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
    parser.add_argument(
        "--aplicar", action="store_true", help="confirma escritura atómica en PostgreSQL"
    )
    parser.add_argument("--reporte", type=Path, default=Path("reporte-importacion-2026.json"))
    parser.add_argument("--credenciales", type=Path, default=Path("credenciales-2026.csv"))
    parser.add_argument(
        "--fecha-corte", type=date.fromisoformat,
        help="opcional: importa únicamente sustituciones desde AAAA-MM-DD",
    )
    parser.add_argument(
        "--solo-sustituciones",
        action="store_true",
        help="recupera exclusivamente sustituciones de menú; no modifica personas, rutas ni plantillas",
    )
    args = parser.parse_args()
    origen = os.getenv("SQL_SERVER_ORIGEN", "").strip()
    if not origen:
        parser.error("SQL_SERVER_ORIGEN es requerida")
    personas, rutas, menu, calendario, sustituciones = extraer(origen)
    if args.fecha_corte:
        sustituciones = [fila for fila in sustituciones if fila["Fecha"] >= args.fecha_corte]
    reporte = validar_sustituciones(sustituciones) if args.solo_sustituciones else validar(personas, rutas, menu)
    reporte.update(
        {
            "modo": "aplicar" if args.aplicar else "simulacion",
            "alcance": "solo_sustituciones" if args.solo_sustituciones else "importacion_inicial",
            "rutas": len(rutas),
            "plantillas": len({m["IdMenuPlantilla"] for m in menu}),
            "dias_calendario": len(calendario),
            "sustituciones": len({m["Fecha"] for m in sustituciones}),
        }
    )
    if args.aplicar:
        if reporte["errores"]:
            print("Hay errores bloqueantes; no se aplicó la importación.", file=sys.stderr)
            resultado = 2
        else:
            url = os.getenv("DATABASE_URL", "").strip()
            semilla = os.getenv("CODIGO_MIGRACION_SEMILLA", "").strip()
            if not url:
                parser.error("DATABASE_URL es requerida")
            if args.solo_sustituciones:
                reporte["aplicados"] = aplicar_sustituciones(url, sustituciones)
                resultado = 0
            elif len(semilla) < 32:
                parser.error(
                    "CODIGO_MIGRACION_SEMILLA (mínimo 32 caracteres) es requerida"
                )
            else:
                reporte["aplicados"] = aplicar(
                    url, personas, rutas, menu, calendario, sustituciones, semilla, args.credenciales
                )
                resultado = 0
    else:
        resultado = 0 if not reporte["errores"] else 2
    args.reporte.write_text(
        json.dumps(reporte, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
    )
    print(json.dumps(reporte, ensure_ascii=False, indent=2))
    return resultado


if __name__ == "__main__":
    raise SystemExit(main())
