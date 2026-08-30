"""Consultas no transaccionales de la pantalla operativa."""

from datetime import date

from .repositorio_base import RepositorioSqlComedorBase


class RepositorioSqlConsultasOperacion(RepositorioSqlComedorBase):
    def configuracion_operacion(self) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT CAST(GETDATE() AS date),CONVERT(varchar(8),CAST(GETDATE() AS time),108)"
            )
            reloj = cursor.fetchone()
            cursor.execute("""SELECT minutos_aviso_previo,permitir_marca_tardia,
                permitir_sin_marca_transporte FROM comedor.parametro WHERE id_parametro=1""")
            politica = cursor.fetchone() or (15, 0, 1)
            cursor.execute("""SELECT codigo,descripcion,CONVERT(varchar(8),hora_limite,108),activo
                FROM comedor.horario_operacion ORDER BY hora_limite""")
            horarios = [
                {"codigo": r[0], "descripcion": r[1], "hora_limite": r[2], "activo": bool(r[3])}
                for r in cursor.fetchall()
            ]
        return {
            "fecha_servidor": reloj[0],
            "hora_servidor": reloj[1],
            "minutos_aviso_previo": int(politica[0]),
            "permitir_marca_tardia": bool(politica[1]),
            "permitir_sin_marca_transporte": bool(politica[2]),
            "horarios": horarios,
        }

    def historial_operacion(self, fecha: date, limite: int) -> list[dict]:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT TOP (?) i.id_ingreso,i.id_persona,i.fecha,i.modalidad,
                i.codigo_horario,i.hora_marca,i.marca_transporte_existente,i.registrado_por,
                i.resultado,p.nombre_completo,i.hora_limite_aplicada,i.advertencias
                FROM comedor.ingreso i INNER JOIN comedor.persona p ON p.id_persona=i.id_persona
                WHERE i.fecha=? ORDER BY i.id_ingreso DESC""",
                limite,
                fecha,
            )
            columnas = [c[0] for c in cursor.description or ()]
            return [dict(zip(columnas, fila)) for fila in cursor.fetchall()]

    def estado_operacion(self) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT CAST(GETDATE() AS date)")
            fecha_servidor = cursor.fetchone()[0]
            cursor.execute("SELECT COUNT_BIG(*) FROM comedor.ingreso WHERE fecha=?", fecha_servidor)
            ingresos = int(cursor.fetchone()[0])
        return {"fecha_servidor": fecha_servidor, "ingresos_hoy": ingresos}
