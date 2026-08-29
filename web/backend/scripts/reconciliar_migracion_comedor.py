#!/usr/bin/env python3
"""Detecta diferencias del corte web sin convertir datos ambiguos."""

from __future__ import annotations

import argparse
import json
import os


CONSULTAS = {
    "ruta_multiple": """SELECT CONVERT(varchar(200),id_estudiante),N'Estudiante con más de una ruta activa'
        FROM transporte.asignacion_ruta WHERE activa=1 GROUP BY id_estudiante HAVING COUNT(*)>1""",
    "saldo_negativo": """SELECT CONVERT(varchar(200),id_cuenta),N'Cuenta de tiquetes con saldo negativo'
        FROM comedor.cuenta_tiquetes WHERE saldo<0""",
    "carnet_duplicado": """SELECT codigo_barras,N'Código de barras repetido'
        FROM comedor.persona GROUP BY codigo_barras HAVING COUNT(*)>1""",
    "persona_sin_vinculo": """SELECT CONVERT(varchar(200),id_persona),N'Estudiante sin vínculo canónico'
        FROM comedor.persona WHERE tipo_persona='estudiante' AND id_estudiante IS NULL
        UNION ALL
        SELECT CONVERT(varchar(200),id_persona),N'Profesor sin vínculo canónico'
        FROM comedor.persona WHERE tipo_persona='profesor' AND id_usuario IS NULL""",
    "horario_sin_origen": """SELECT CONVERT(varchar(200),id_horario),N'Horario sin hora límite de origen'
        FROM comedor.horario_operacion WHERE hora_limite IS NULL OR origen IS NULL""",
    "ingreso_duplicado": """SELECT CONCAT(CONVERT(varchar(30),id_persona),':',CONVERT(varchar(30),fecha)),N'Ingreso duplicado por persona y fecha'
        FROM comedor.ingreso GROUP BY id_persona,fecha HAVING COUNT(*)>1""",
}

CONSULTAS_COMPARATIVAS = {
    "saldo_local_web": (
        ("dbo.Usuario", "comedor.persona", "comedor.cuenta_tiquetes"),
        """SELECT CONVERT(varchar(200),u.IdUsuario),
            CONCAT(N'Saldo local=',u.CantidadTiquetes,N'; saldo web=',ct.saldo,
                   N'; reservados web=',ct.reservados)
            FROM dbo.Usuario u
            INNER JOIN comedor.persona p ON
                (u.CodTipo=1 AND p.tipo_persona='estudiante' AND p.id_estudiante=u.IdUsuario)
                OR (u.CodTipo=2 AND p.tipo_persona='profesor' AND p.id_usuario=u.IdUsuario)
            INNER JOIN comedor.cuenta_tiquetes ct ON ct.id_persona=p.id_persona
            WHERE ISNULL(u.CantidadTiquetes,0) <> ISNULL(ct.saldo,0)""",
    ),
    "conteo_ingresos_local_web": (
        ("dbo.RegistroComedor", "comedor.ingreso"),
        """SELECT N'total', CONCAT(N'Ingresos local=',COUNT_LOCAL.total,
            N'; ingresos web=',COUNT_WEB.total)
            FROM (SELECT COUNT_BIG(*) total FROM (SELECT IdUsuario,CONVERT(date,Fecha) fecha
                  FROM dbo.RegistroComedor GROUP BY IdUsuario,CONVERT(date,Fecha)) unicos) COUNT_LOCAL
            CROSS JOIN (SELECT SUM(row_count) total FROM sys.dm_db_partition_stats
                  WHERE object_id=OBJECT_ID(N'comedor.ingreso') AND index_id IN (0,1)) COUNT_WEB
            WHERE COUNT_LOCAL.total <> COUNT_WEB.total""",
    ),
    "estado_comedor_local_web": (
        ("dbo.Usuario", "comedor.persona"),
        """SELECT CONVERT(varchar(200),x.estado),
            CONCAT(N'Personas local=',x.locales,N'; personas web=',x.webs)
            FROM (
                SELECT estado, SUM(locales) locales, SUM(webs) webs
                FROM (
                    SELECT CASE WHEN u.TipoBeca=2 THEN 'becado_comedor' ELSE 'no_becado_comedor' END estado,
                           COUNT_BIG(*) locales, CONVERT(bigint,0) webs
                    FROM dbo.Usuario u WHERE u.CodTipo=1 AND u.Activo=1 GROUP BY CASE WHEN u.TipoBeca=2 THEN 'becado_comedor' ELSE 'no_becado_comedor' END
                    UNION ALL
                    SELECT p.estado_comedor, CONVERT(bigint,0), COUNT_BIG(*)
                    FROM comedor.persona p WHERE p.tipo_persona='estudiante' AND p.activo=1 GROUP BY p.estado_comedor
                ) datos GROUP BY estado
            ) x WHERE x.locales <> x.webs""",
    ),
    "profesores_habilitados_local_web": (
        ("dbo.Usuario", "comedor.persona"),
        """SELECT N'profesores', CONCAT(N'Profesores local=',locales,N'; profesores web=',webs)
            FROM (SELECT COUNT_BIG(*) locales FROM dbo.Usuario WHERE Activo=1 AND CodTipo=2) l
            CROSS JOIN (SELECT COUNT_BIG(*) webs FROM comedor.persona WHERE tipo_persona='profesor' AND activo=1) w
            WHERE l.locales <> w.webs""",
    ),
    "ingresos_por_fecha_local_web": (
        ("dbo.RegistroComedor", "comedor.ingreso"),
        """SELECT CONVERT(varchar(30),COALESCE(l.fecha,w.fecha)),
            CONCAT(N'Ingresos local=',ISNULL(l.total,0),N'; ingresos web=',ISNULL(w.total,0))
            FROM (SELECT CONVERT(date,fecha) fecha,COUNT(DISTINCT IdUsuario) total
                  FROM dbo.RegistroComedor GROUP BY CONVERT(date,fecha)) l
            FULL OUTER JOIN (SELECT fecha,COUNT_BIG(*) total FROM comedor.ingreso GROUP BY fecha) w
              ON w.fecha=l.fecha
            WHERE ISNULL(l.total,0) <> ISNULL(w.total,0)""",
    ),
}


def _tabla_existe(cursor, tabla: str) -> bool:
    # Los nombres provienen del inventario constante del propio script.
    cursor.execute(f"SELECT OBJECT_ID(N'{tabla}', N'U')")
    return cursor.fetchone()[0] is not None


def ejecutar(cadena: str, aplicar: bool) -> list[dict[str, str]]:
    import pyodbc

    hallazgos: list[dict[str, str]] = []
    with pyodbc.connect(cadena, autocommit=False, timeout=60) as conexion:
        cursor = conexion.cursor()
        # El corte es de lectura; no debe quedar bloqueado por una escritura
        # operativa ajena. Las diferencias se persisten en una fase separada.
        cursor.execute("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED")
        for tipo, consulta in CONSULTAS.items():
            print(f"reconciliando:{tipo}", flush=True)
            cursor.execute(consulta)
            for clave, detalle in cursor.fetchall():
                hallazgo = {"tipo": tipo, "clave": str(clave), "detalle": str(detalle)}
                hallazgos.append(hallazgo)
                if aplicar:
                    cursor.execute("""IF NOT EXISTS (SELECT 1 FROM comedor.reconciliacion_migracion
                        WHERE tipo=? AND clave=?) INSERT comedor.reconciliacion_migracion(tipo,clave,detalle)
                        VALUES (?,?,?)""", tipo, str(clave), tipo, str(clave), str(detalle))
        for tipo, (tablas, consulta) in CONSULTAS_COMPARATIVAS.items():
            if not all(_tabla_existe(cursor, tabla) for tabla in tablas):
                continue
            print(f"comparando:{tipo}", flush=True)
            cursor.execute(consulta)
            for clave, detalle in cursor.fetchall():
                hallazgo = {"tipo": tipo, "clave": str(clave), "detalle": str(detalle)}
                hallazgos.append(hallazgo)
                if aplicar:
                    cursor.execute("""IF NOT EXISTS (SELECT 1 FROM comedor.reconciliacion_migracion
                        WHERE tipo=? AND clave=?) INSERT comedor.reconciliacion_migracion(tipo,clave,detalle)
                        VALUES (?,?,?)""", tipo, str(clave), tipo, str(clave), str(detalle))
        if aplicar:
            conexion.commit()
        else:
            conexion.rollback()
    return hallazgos


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--apply", action="store_true", help="persiste hallazgos; por defecto solo lee")
    args = parser.parse_args()
    cadena = os.getenv("SQL_CONNECTION_STRING", "").strip()
    if not cadena:
        parser.error("SQL_CONNECTION_STRING es requerida")
    print(json.dumps(ejecutar(cadena, args.apply), ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
