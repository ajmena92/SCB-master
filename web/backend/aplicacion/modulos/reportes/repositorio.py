"""Consultas agregadas del dashboard sobre los esquemas web canónicos."""

from __future__ import annotations

from datetime import date, timedelta
from typing import Protocol

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql
from .repositorio_profesores import RepositorioSqlProfesores


class RepositorioReportes(Protocol):
    def estudiantes(self) -> list[dict]: ...
    def transporte(self) -> list[dict]: ...
    def resumen(self) -> dict: ...
    def dashboard(self, fecha: date, **filtros: object) -> dict: ...


class RepositorioSqlReportes(RepositorioSqlProfesores):
    """Persistencia de solo lectura; no consulta el sistema histórico."""

    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    @staticmethod
    def _filas(cursor: CursorSql) -> list[dict]:
        columnas = [descripcion[0] for descripcion in cursor.description or ()]
        return [dict(zip(columnas, fila)) for fila in cursor.fetchall()]

    @staticmethod
    def _porcentaje(parte: int, total: int) -> float:
        return round(parte * 100 / total, 1) if total else 0

    @staticmethod
    def _calendario(cursor: CursorSql, inicio: date, fin: date) -> dict[date, bool]:
        cursor.execute(
            "SELECT fecha, habilitado FROM menu.calendario WHERE fecha>=? AND fecha<=?",
            inicio,
            fin,
        )
        return {fila[0]: bool(fila[1]) for fila in cursor.fetchall()}

    @classmethod
    def _laborables(cls, cursor: CursorSql, fecha: date, cantidad: int) -> list[date]:
        dias: list[date] = []
        actual = fecha
        calendario = cls._calendario(cursor, fecha - timedelta(days=90), fecha)
        while len(dias) < cantidad:
            if actual.weekday() < 5 and calendario.get(actual, True):
                dias.append(actual)
            actual -= timedelta(days=1)
        return list(reversed(dias))

    def estudiantes(self) -> list[dict]:
        consulta = """
            SELECT id_estudiante, carne,
                   CONCAT(nombre, N' ', primer_apellido, N' ', ISNULL(segundo_apellido, N'')) AS nombre_completo,
                   seccion, activo
            FROM estudiantes.estudiante
            WHERE activo = 1
            ORDER BY primer_apellido, nombre, id_estudiante
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta)
            return self._filas(cursor)

    def transporte(self) -> list[dict]:
        consulta = """
            SELECT r.id_ruta, r.codigo, r.descripcion, r.activo,
                   COUNT(DISTINCT a.id_estudiante) AS estudiantes_asignados
            FROM transporte.ruta AS r
            LEFT JOIN transporte.asignacion_ruta AS a
              ON a.id_ruta = r.id_ruta AND a.activa = 1
            WHERE LTRIM(RTRIM(r.codigo)) <> N'0000'
            GROUP BY r.id_ruta, r.codigo, r.descripcion, r.activo
            ORDER BY r.activo DESC, r.codigo, r.id_ruta
        """
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(consulta)
            return self._filas(cursor)

    def resumen(self) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT COUNT(*) FROM estudiantes.estudiante WHERE activo=1")
            estudiantes = cursor.fetchone()
            cursor.execute("SELECT COUNT(*) FROM asistencia.marca WHERE estado IN ('presente','confirmada')")
            confirmaciones = cursor.fetchone()
            cursor.execute("SELECT COUNT(*) FROM asistencia.marca WHERE estado IN ('ausente','cancelada')")
            cancelaciones = cursor.fetchone()
        return {
            "estudiantes": int(estudiantes[0]) if estudiantes else 0,
            "confirmaciones": int(confirmaciones[0]) if confirmaciones else 0,
            "cancelaciones": int(cancelaciones[0]) if cancelaciones else 0,
        }

    def _tendencia(
        self, cursor: CursorSql, fechas: list[date], horario: str | None = None
    ) -> list[dict]:
        if not fechas:
            return []
        filtro_horario = ""
        parametros_horario: list[object] = []
        if horario:
            filtro_horario = " AND LOWER(LTRIM(RTRIM(e.turno)))=?"
            parametros_horario.append(horario)
        cursor.execute(
            """SELECT m.fecha,
                COUNT(DISTINCT CASE WHEN m.estado IN ('presente','confirmada') THEN m.id_estudiante END),
                COUNT(DISTINCT CASE WHEN m.estado IN ('ausente','cancelada') THEN m.id_estudiante END)
            FROM asistencia.marca m
            JOIN estudiantes.estudiante e ON e.id_estudiante=m.id_estudiante
            WHERE e.activo=1 AND m.fecha BETWEEN ? AND ?"""
            + filtro_horario
            + """ GROUP BY m.fecha ORDER BY m.fecha""",
            fechas[0],
            fechas[-1],
            *parametros_horario,
        )
        filas = {fila[0]: (int(fila[1]), int(fila[2])) for fila in cursor.fetchall()}
        consulta_total = "SELECT COUNT(*) FROM estudiantes.estudiante WHERE activo=1" + (
            " AND LOWER(LTRIM(RTRIM(turno)))=?" if horario else ""
        )
        cursor.execute(consulta_total, *parametros_horario)
        total = int((cursor.fetchone() or (0,))[0])
        resultado = []
        for dia in fechas:
            presentes, ausentes = filas.get(dia, (0, 0))
            resultado.append({
                "fecha": dia,
                "dia": dia.strftime("%A"),
                "total": total,
                "presentes": presentes,
                "ausentes": ausentes,
                "sin_registro": max(total - presentes - ausentes, 0),
                "porcentaje": self._porcentaje(presentes, total),
            })
        return resultado

    def dashboard(
        self,
        fecha: date,
        *,
        pagina: int = 1,
        por_pagina: int = 25,
        busqueda: str | None = None,
        id_ruta: int | None = None,
        id_estado_comedor: int | None = None,
        beneficio_transporte: str | None = None,
        seccion: str | None = None,
        estado: str | None = None,
        tipo_persona: str = "estudiante",
        horario: str | None = None,
    ) -> dict:
        if tipo_persona == "profesor":
            return self._dashboard_profesores(fecha, pagina, por_pagina, busqueda, estado)
        if tipo_persona != "estudiante":
            raise ValueError("El tipo de persona no es válido")
        filtros = ["(e.activo=1 OR m.id_marca IS NOT NULL)"]
        filtro_parametros: list[object] = []
        if busqueda:
            filtros.append("(e.nombre LIKE ? OR e.primer_apellido LIKE ? OR e.cedula LIKE ? OR e.seccion LIKE ?)")
            filtro_parametros.extend([f"%{busqueda}%"] * 4)
        if id_ruta is not None:
            filtros.append("r.id_ruta=?")
            filtro_parametros.append(id_ruta)
        if beneficio_transporte == "beneficiario":
            filtros.append("r.id_ruta IS NOT NULL")
        elif beneficio_transporte == "no_beneficiario":
            filtros.append("r.id_ruta IS NULL")
        if id_estado_comedor:
            filtros.append("cp.id_estado_comedor=?")
            filtro_parametros.append(id_estado_comedor)
        if seccion:
            filtros.append("e.seccion=?")
            filtro_parametros.append(seccion)
        if estado == "sin_registro":
            filtros.append("m.id_marca IS NULL")
        elif estado:
            filtros.append("m.estado=?")
            filtro_parametros.append(estado)
        where = " AND ".join(filtros)
        base = f"""
            FROM estudiantes.estudiante e
            OUTER APPLY (SELECT TOP 1 id_marca, estado FROM asistencia.marca
                         WHERE id_estudiante=e.id_estudiante AND fecha=? ORDER BY id_marca DESC) m
            INNER JOIN comedor.persona cp
              ON cp.id_estudiante=e.id_estudiante AND cp.tipo_persona='estudiante'
            OUTER APPLY (SELECT TOP 1 id_ruta FROM transporte.asignacion_ruta
                         WHERE id_estudiante=e.id_estudiante AND activa=1
                         ORDER BY fecha_creacion DESC, id_asignacion DESC) ar
            LEFT JOIN transporte.ruta r ON r.id_ruta=ar.id_ruta AND r.activo=1
                AND LTRIM(RTRIM(r.codigo))<>N'0000'
            LEFT JOIN comedor.ingreso ci ON ci.id_persona=cp.id_persona AND ci.fecha=?
            INNER JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=cp.id_estado_comedor
            WHERE {where}
        """
        base_parametros = [fecha, fecha, *filtro_parametros]
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT DISTINCT LOWER(LTRIM(RTRIM(e.turno)))
                """ + base + """ AND LOWER(LTRIM(RTRIM(e.turno))) IN ('diurno','nocturno')
                ORDER BY LOWER(LTRIM(RTRIM(e.turno)))""",
                *base_parametros,
            )
            horarios = [fila[0] for fila in cursor.fetchall()]
            if horario:
                filtros.append("LOWER(LTRIM(RTRIM(e.turno)))=?")
                filtro_parametros.append(horario)
                where = " AND ".join(filtros)
                base = f"""
                    FROM estudiantes.estudiante e
                    OUTER APPLY (SELECT TOP 1 id_marca, estado FROM asistencia.marca
                                 WHERE id_estudiante=e.id_estudiante AND fecha=? ORDER BY id_marca DESC) m
                    INNER JOIN comedor.persona cp
                      ON cp.id_estudiante=e.id_estudiante AND cp.tipo_persona='estudiante'
                    OUTER APPLY (SELECT TOP 1 id_ruta FROM transporte.asignacion_ruta
                                 WHERE id_estudiante=e.id_estudiante AND activa=1
                                 ORDER BY fecha_creacion DESC, id_asignacion DESC) ar
                    LEFT JOIN transporte.ruta r ON r.id_ruta=ar.id_ruta AND r.activo=1
                        AND LTRIM(RTRIM(r.codigo))<>N'0000'
                    LEFT JOIN comedor.ingreso ci ON ci.id_persona=cp.id_persona AND ci.fecha=?
                    INNER JOIN comedor.estado_comedor ec ON ec.id_estado_comedor=cp.id_estado_comedor
                    WHERE {where}
                """
                base_parametros = [fecha, fecha, *filtro_parametros]
            cursor.execute(
                """SELECT COUNT(DISTINCT e.id_estudiante),
                    COUNT(DISTINCT CASE WHEN m.estado IN ('presente','confirmada') THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN m.estado IN ('ausente','cancelada') THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN m.estado='tardanza' THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN m.estado='justificada' THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN ci.id_ingreso IS NOT NULL THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN cp.id_estado_comedor=1 THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN cp.id_estado_comedor=2 THEN e.id_estudiante END)
                """ + base,
                *base_parametros,
            )
            fila = cursor.fetchone() or (0,) * 8
            total, presentes, ausentes, tardanzas, justificadas, consumo, beneficiarios, no_beneficiarios = map(int, fila)

            cursor.execute(
                """SELECT
                    COUNT(DISTINCT CASE WHEN cp.id_estado_comedor=1 AND m.id_marca IS NULL THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN cp.id_estado_comedor=1 AND ci.id_ingreso IS NULL THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN cp.id_estado_comedor=2 AND ci.id_ingreso IS NULL THEN e.id_estudiante END)
                """ + base,
                *base_parametros,
            )
            sin_asistencia, beneficiarios_sin_consumo, no_beneficiarios_sin_ingreso = map(
                int, cursor.fetchone() or (0, 0, 0)
            )
            alertas = [
                {"tipo": "beneficiarios_sin_asistencia", "titulo": "Beneficiarios sin asistencia", "cantidad": sin_asistencia},
                {"tipo": "beneficiarios_sin_consumo", "titulo": "Beneficiarios sin consumo", "cantidad": beneficiarios_sin_consumo},
                {"tipo": "no_beneficiarios_sin_ingreso", "titulo": "No beneficiarios sin ingreso", "cantidad": no_beneficiarios_sin_ingreso},
            ]

            grupos: dict[str, list[dict]] = {}
            for expresion, clave in (("COALESCE(e.turno,N'Sin horario')", "por_horario"), ("COALESCE(e.seccion,N'Sin sección')", "por_seccion"), ("ec.descripcion", "por_estado_comedor")):
                cursor.execute(
                    f"""SELECT {expresion}, COUNT(DISTINCT e.id_estudiante),
                        COUNT(DISTINCT CASE WHEN m.estado IN ('presente','confirmada') THEN e.id_estudiante END),
                        COUNT(DISTINCT CASE WHEN m.estado IN ('ausente','cancelada') THEN e.id_estudiante END),
                        COUNT(DISTINCT CASE WHEN m.id_marca IS NULL THEN e.id_estudiante END),
                        COUNT(DISTINCT CASE WHEN ci.id_ingreso IS NOT NULL THEN e.id_estudiante END)
                    {base} GROUP BY {expresion} ORDER BY {expresion}""",
                    *base_parametros,
                )
                filas_grupo = []
                for grupo in cursor.fetchall():
                    nombre, total_grupo, presentes_grupo, ausentes_grupo, sin_registro, consumo_grupo = grupo
                    filas_grupo.append({
                        "nombre": nombre, "total": int(total_grupo), "presentes": int(presentes_grupo),
                        "ausentes": int(ausentes_grupo), "sin_registro": int(sin_registro), "consumo": int(consumo_grupo),
                        "porcentaje": self._porcentaje(int(presentes_grupo), int(total_grupo)),
                    })
                grupos[clave] = filas_grupo

            cursor.execute(
                f"""SELECT r.id_ruta, COALESCE(r.codigo,N'Sin ruta'), COUNT(DISTINCT e.id_estudiante),
                    COUNT(DISTINCT CASE WHEN m.estado IN ('presente','confirmada') THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN m.estado IN ('ausente','cancelada') THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN m.id_marca IS NULL THEN e.id_estudiante END),
                    COUNT(DISTINCT CASE WHEN ci.id_ingreso IS NOT NULL THEN e.id_estudiante END)
                {base} GROUP BY r.id_ruta,r.codigo ORDER BY COALESCE(r.codigo,N'Sin ruta')""",
                *base_parametros,
            )
            por_ruta = []
            for grupo in cursor.fetchall():
                id_grupo, nombre, total_grupo, presentes_grupo, ausentes_grupo, sin_registro, consumo_grupo = grupo
                por_ruta.append({
                    "id_ruta": id_grupo, "nombre": nombre, "total": int(total_grupo),
                    "presentes": int(presentes_grupo), "ausentes": int(ausentes_grupo),
                    "sin_registro": int(sin_registro), "consumo": int(consumo_grupo),
                    "porcentaje": self._porcentaje(int(presentes_grupo), int(total_grupo)),
                })

            cursor.execute("SELECT COUNT(DISTINCT e.id_estudiante) " + base, *base_parametros)
            total_nominal = int((cursor.fetchone() or (0,))[0])
            cursor.execute(
                """SELECT e.id_estudiante, CONCAT(e.nombre,N' ',e.primer_apellido,N' ',ISNULL(e.segundo_apellido,N'')), e.cedula,
                    COALESCE(e.turno,N'Sin horario'), COALESCE(e.seccion,N'Sin sección'),
                    cp.id_estado_comedor, ec.descripcion,
                    CASE WHEN r.id_ruta IS NULL THEN N'No beneficiario'
                         ELSE CONCAT(N'Beneficiario – ',r.descripcion) END,
                    m.estado, m.id_marca, e.activo, cp.id_persona
                """ + base + " ORDER BY e.primer_apellido,e.nombre,e.id_estudiante OFFSET ? ROWS FETCH NEXT ? ROWS ONLY",
                *base_parametros,
                (pagina - 1) * por_pagina,
                por_pagina,
            )
            estados = {"presente": "Confirmada", "confirmada": "Confirmada", "ausente": "No asistirá", "cancelada": "No asistirá", "tardanza": "Tardanza", "justificada": "Justificada"}
            nominal = [
                {"id_persona": fila[11], "id_estudiante": fila[0], "nombre_completo": fila[1], "cedula": fila[2], "horario": fila[3], "seccion": fila[4], "tipo_persona": "estudiante", "id_estado_comedor": fila[5], "beneficio_comedor": fila[6], "ruta": fila[7], "estado": estados.get(fila[8], "Sin registro"), "origen": "Portal" if fila[9] else "Sin registro", "historico": not bool(fila[10])}
                for fila in cursor.fetchall()
            ]
            calendario = self._calendario(cursor, fecha - timedelta(days=7), fecha + timedelta(days=7))
            semana_fechas = [fecha - timedelta(days=fecha.weekday() - i) for i in range(5)]
            semana_fechas = [dia for dia in semana_fechas if calendario.get(dia, True)]
            semana = self._tendencia(cursor, semana_fechas, horario)
            ultimos = self._tendencia(cursor, self._laborables(cursor, fecha, 5), horario)
        return {
            "fecha": fecha,
            "tipo_persona": "estudiante",
            "horarios": horarios,
            "alertas": alertas,
            "asistencia": {"total": total, "presentes": presentes, "ausentes": ausentes, "tardanzas": tardanzas, "justificadas": justificadas, "sin_registro": max(total - presentes - ausentes - tardanzas - justificadas, 0), "cobertura_registro": self._porcentaje(presentes + ausentes + tardanzas + justificadas, total), "porcentaje": self._porcentaje(presentes, total)},
            "consumo_comedor": consumo,
            "beneficiarios_comedor": beneficiarios,
            "no_beneficiarios": no_beneficiarios,
            **grupos, "por_ruta": por_ruta, "semana": semana, "ultimos_cinco_dias": ultimos,
            "nominal": {"elementos": nominal, "total": total_nominal, "pagina": pagina, "por_pagina": por_pagina},
        }
