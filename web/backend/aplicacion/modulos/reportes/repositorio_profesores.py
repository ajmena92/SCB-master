"""Consultas del dashboard de comedor para profesores."""

from __future__ import annotations

from datetime import date

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioSqlProfesores:
    _fabrica: FabricaConexionSql

    @staticmethod
    def _porcentaje(parte: int, total: int) -> float:
        return round(parte * 100 / total, 1) if total else 0

    @staticmethod
    def _filas(cursor: CursorSql) -> list[dict]:
        columnas = [descripcion[0] for descripcion in cursor.description or ()]
        return [dict(zip(columnas, fila)) for fila in cursor.fetchall()]

    def _dashboard_profesores(
        self,
        fecha: date,
        pagina: int,
        por_pagina: int,
        busqueda: str | None,
        estado: str | None,
    ) -> dict:
        filtros = ["p.tipo_persona='profesor'", "(p.activo=1 OR i.id_ingreso IS NOT NULL)"]
        parametros: list[object] = [fecha]
        if busqueda:
            filtros.append(
                "(p.nombre_completo LIKE ? OR p.codigo_barras LIKE ? OR p.colegio LIKE ?)"
            )
            parametros.extend([f"%{busqueda}%"] * 3)
        if estado in ("presente", "confirmada"):
            filtros.append("i.id_ingreso IS NOT NULL")
        elif estado in ("sin_registro", "ausente"):
            filtros.append("i.id_ingreso IS NULL")
        where = " AND ".join(filtros)
        base = f"""FROM comedor.persona p
            LEFT JOIN comedor.ingreso i ON i.id_persona=p.id_persona AND i.fecha=?
            LEFT JOIN comedor.cuenta_tiquetes c ON c.id_persona=p.id_persona
            WHERE {where}"""
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT COUNT(DISTINCT p.id_persona) " + base, *parametros)
            total = int((cursor.fetchone() or (0,))[0])
            cursor.execute(
                "SELECT COUNT(DISTINCT p.id_persona) " + base + " AND i.id_ingreso IS NOT NULL",
                *parametros,
            )
            presentes = int((cursor.fetchone() or (0,))[0])
            cursor.execute(
                "SELECT COALESCE(SUM(c.saldo),0), COALESCE(SUM(c.reservados),0) " + base,
                *parametros,
            )
            saldo, reservados = cursor.fetchone() or (0, 0)
            cursor.execute(
                """SELECT COUNT(*) FROM comedor.movimiento_tiquetes mt
                INNER JOIN comedor.cuenta_tiquetes c ON c.id_cuenta=mt.id_cuenta
                INNER JOIN comedor.persona p2 ON p2.id_persona=c.id_persona
                WHERE p2.tipo_persona='profesor' AND mt.tipo='consumo'"""
            )
            consumidos = int((cursor.fetchone() or (0,))[0])
            cursor.execute(
                "SELECT COUNT(*) FROM comedor.ingreso i2 "
                "INNER JOIN comedor.persona p2 ON p2.id_persona=i2.id_persona "
                "WHERE p2.tipo_persona='profesor'"
            )
            ingresos_historicos = int((cursor.fetchone() or (0,))[0])
            cursor.execute(
                "SELECT p.id_persona,p.nombre_completo,p.colegio,p.codigo_barras,"
                "i.id_ingreso,p.activo "
                + base
                + " ORDER BY p.nombre_completo,p.id_persona OFFSET ? ROWS FETCH NEXT ? ROWS ONLY",
                *parametros,
                (pagina - 1) * por_pagina,
                por_pagina,
            )
            nominal = [
                {
                    "id_persona": fila[0],
                    "id_estudiante": None,
                    "nombre_completo": fila[1],
                    "cedula": None,
                    "horario": "No aplica",
                    "seccion": "No aplica",
                    "tipo_persona": "profesor",
                    "id_estado_comedor": 2,
                    "beneficio_comedor": "No beneficiario",
                    "ruta": "No aplica",
                    "estado": "Confirmada" if fila[4] else "Sin registro",
                    "origen": "Comedor",
                    "historico": not bool(fila[5]),
                }
                for fila in cursor.fetchall()
            ]
        sin_registro = max(total - presentes, 0)
        return {
            "fecha": fecha,
            "tipo_persona": "profesor",
            "saldo_tiquetes": int(saldo),
            "tiquetes_reservados": int(reservados),
            "tiquetes_consumidos": consumidos,
            "ingresos_historicos": ingresos_historicos,
            "asistencia": {
                "total": total,
                "presentes": presentes,
                "ausentes": 0,
                "tardanzas": 0,
                "justificadas": 0,
                "sin_registro": sin_registro,
                "cobertura_registro": self._porcentaje(presentes, total),
                "porcentaje": self._porcentaje(presentes, total),
            },
            "consumo_comedor": presentes,
            "beneficiarios_comedor": 0,
            "no_beneficiarios": 0,
            "por_horario": [],
            "por_seccion": [],
            "por_estado_comedor": [],
            "por_ruta": [],
            "semana": [],
            "ultimos_cinco_dias": [],
            "nominal": {
                "elementos": nominal,
                "total": total,
                "pagina": pagina,
                "por_pagina": por_pagina,
            },
        }
