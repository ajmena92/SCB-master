"""Persistencia aislada del esquema estudiantes."""

from __future__ import annotations

from typing import Protocol, cast

from aplicacion.nucleo.base_datos import CursorSql, FabricaConexionSql


class RepositorioEstudiantes(Protocol):
    def listar(self, pagina: int, tamano: int, buscar: str) -> tuple[list[dict], int]: ...
    def buscar_por_id(self, id_estudiante: int) -> dict | None: ...
    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict: ...
    def actualizar(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict: ...
    def obtener_foto(self, id_estudiante: int) -> tuple[bytes, str] | None: ...
    def guardar_foto(self, id_estudiante: int, contenido: bytes, tipo: str) -> None: ...
    def eliminar_foto(self, id_estudiante: int) -> None: ...
    def perfil_detallado(self, id_estudiante: int) -> dict: ...
    def secciones(self, turno: str | None = None) -> list[dict]: ...
    def asignar_beneficio(self, id_estudiante: int, id_beneficio: int | None) -> None: ...
    def asignar_ruta(self, id_estudiante: int, id_ruta: int | None) -> None: ...
    def reiniciar_pin(self, id_estudiante: int, hash_contrasena: str) -> None: ...
    def listar_para_generacion_pines(self) -> list[dict]: ...
    def actualizar_pines_seccion(self, seccion: str | None, hashes: dict[int, str]) -> None: ...
    def buscar_credencial(self, carne: str) -> dict | None: ...
    def buscar_credencial_por_id(self, id_estudiante: int) -> dict | None: ...
    def actualizar_pin(self, id_estudiante: int, hash_pin: str) -> None: ...


class RepositorioSqlEstudiantes:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def obtener_foto(self, id_estudiante: int) -> tuple[bytes, str] | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT contenido,tipo_contenido FROM estudiantes.fotografia WHERE id_estudiante=?",
                id_estudiante,
            )
            fila = cursor.fetchone()
        return (bytes(cast(bytes, fila[0])), str(fila[1])) if fila else None

    def guardar_foto(self, id_estudiante: int, contenido: bytes, tipo: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "MERGE estudiantes.fotografia AS t USING (SELECT ? id_estudiante) s ON t.id_estudiante=s.id_estudiante "
                "WHEN MATCHED THEN UPDATE SET contenido=?,tipo_contenido=? "
                "WHEN NOT MATCHED THEN INSERT(id_estudiante,contenido,tipo_contenido) VALUES(?,?,?);",
                id_estudiante,
                contenido,
                tipo,
                id_estudiante,
                contenido,
                tipo,
            )

    def eliminar_foto(self, id_estudiante: int) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "DELETE FROM estudiantes.fotografia WHERE id_estudiante=?", id_estudiante
            )

    def perfil_detallado(self, id_estudiante: int) -> dict:
        resultado = self.buscar_por_id(id_estudiante)
        if resultado is None:
            raise ValueError("Estudiante no encontrado")
        return resultado

    def secciones(self, turno: str | None = None) -> list[dict]:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT seccion, COUNT(*) FROM estudiantes.estudiante WHERE activo=1 AND (? IS NULL OR turno=?) GROUP BY seccion ORDER BY seccion",
                turno, turno,
            )
            return [{"seccion": fila[0], "etiqueta": str(fila[0]) if fila[0] is not None else "__SIN_SECCION__", "total": int(str(fila[1]))} for fila in cursor.fetchall()]

    def asignar_beneficio(self, id_estudiante: int, id_beneficio: int | None) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET id_beneficio=? WHERE id_estudiante=?",
                id_beneficio,
                id_estudiante,
            )

    def asignar_ruta(self, id_estudiante: int, id_ruta: int | None) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET id_ruta=? WHERE id_estudiante=?",
                id_ruta,
                id_estudiante,
            )

    def reiniciar_pin(self, id_estudiante: int, hash_contrasena: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute(
                "UPDATE estudiantes.estudiante SET hash_contrasena=?, debe_cambiar_pin=1, fecha_expiracion_pin=DATEADD(day, 1, SYSUTCDATETIME()) WHERE id_estudiante=?",
                hash_contrasena,
                id_estudiante,
            )

    def listar_para_generacion_pines(self) -> list[dict]:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                "SELECT id_estudiante,carne,nombre,primer_apellido,segundo_apellido,cedula,seccion,turno FROM estudiantes.estudiante WHERE activo=1 ORDER BY primer_apellido,nombre"
            )
            return [
                dict(zip((col[0] for col in cursor.description), fila))
                for fila in cursor.fetchall()
            ]

    def actualizar_pines_seccion(self, seccion: str | None, hashes: dict[int, str]) -> None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            for id_estudiante, hash_pin in hashes.items():
                cursor.execute(
                    "UPDATE estudiantes.estudiante SET hash_contrasena=?, debe_cambiar_pin=1, fecha_expiracion_pin=DATEADD(day, 1, SYSUTCDATETIME()) WHERE id_estudiante=? AND activo=1 AND ((seccion=? ) OR (seccion IS NULL AND ? IS NULL))",
                    hash_pin, id_estudiante, seccion, seccion,
                )

    def buscar_credencial(self, carne: str) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute("SELECT id_estudiante, carne, nombre, hash_contrasena, debe_cambiar_pin, fecha_expiracion_pin, activo FROM estudiantes.estudiante WHERE carne=? AND (fecha_expiracion_pin IS NULL OR fecha_expiracion_pin > SYSUTCDATETIME())", carne.strip())
            return self._fila(cursor)

    def buscar_credencial_por_id(self, id_estudiante: int) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT e.id_estudiante, e.carne, e.nombre, e.primer_apellido,
                e.segundo_apellido, e.cedula, e.seccion, e.turno, e.id_ruta,
                e.id_beneficio, e.hash_contrasena, e.debe_cambiar_pin,
                e.fecha_expiracion_pin, e.activo, r.codigo AS ruta_codigo,
                r.descripcion AS ruta_descripcion, r.color_hex AS ruta_color,
                b.nombre AS tipo_beca,
                CAST(CASE WHEN f.id_estudiante IS NULL THEN 0 ELSE 1 END AS bit) AS tiene_foto
                FROM estudiantes.estudiante e
                LEFT JOIN estudiantes.fotografia f ON f.id_estudiante=e.id_estudiante
                LEFT JOIN transporte.asignacion_ruta ar ON ar.id_estudiante=e.id_estudiante AND ar.activa=1
                LEFT JOIN transporte.ruta r ON r.id_ruta=COALESCE(e.id_ruta, ar.id_ruta)
                LEFT JOIN beneficios.asignacion ba ON ba.id_estudiante=e.id_estudiante
                LEFT JOIN beneficios.tipo_beneficio b ON b.id_beneficio=COALESCE(e.id_beneficio, ba.id_beneficio)
                WHERE e.id_estudiante=?""",
                id_estudiante,
            )
            return self._fila(cursor)

    def actualizar_pin(self, id_estudiante: int, hash_pin: str) -> None:
        with self._fabrica.conexion() as conexion:
            conexion.cursor().execute("UPDATE estudiantes.estudiante SET hash_contrasena=?, debe_cambiar_pin=0, fecha_expiracion_pin=NULL WHERE id_estudiante=? AND activo=1", hash_pin, id_estudiante)

    @staticmethod
    def _fila(cursor: CursorSql) -> dict | None:
        fila = cursor.fetchone()
        if fila is None:
            return None
        return dict(zip((col[0] for col in cursor.description), fila))

    @classmethod
    def _filas(cls, cursor: CursorSql) -> list[dict]:
        return [
            dict(zip((col[0] for col in cursor.description), fila)) for fila in cursor.fetchall()
        ]

    def listar(self, pagina: int, tamano: int, buscar: str) -> tuple[list[dict], int]:
        termino = f"%{buscar.strip()}%"
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT e.id_estudiante, e.carne, e.nombre, e.primer_apellido,
                e.segundo_apellido, e.cedula, e.seccion, e.turno, e.id_ruta,
                e.id_beneficio, e.debe_cambiar_pin, e.activo,
                r.codigo AS ruta_codigo, r.descripcion AS ruta_descripcion,
                b.nombre AS tipo_beca,
                CAST(CASE WHEN f.id_estudiante IS NULL THEN 0 ELSE 1 END AS bit) AS tiene_foto
                FROM estudiantes.estudiante e
                LEFT JOIN estudiantes.fotografia f ON f.id_estudiante=e.id_estudiante
                LEFT JOIN transporte.asignacion_ruta ar ON ar.id_estudiante=e.id_estudiante AND ar.activa=1
                LEFT JOIN transporte.ruta r ON r.id_ruta=COALESCE(e.id_ruta, ar.id_ruta)
                LEFT JOIN beneficios.asignacion ba ON ba.id_estudiante=e.id_estudiante
                LEFT JOIN beneficios.tipo_beneficio b ON b.id_beneficio=COALESCE(e.id_beneficio, ba.id_beneficio)
                WHERE e.activo = 1 AND (? = N'' OR CONCAT(e.nombre, N' ', e.primer_apellido,
                N' ', ISNULL(e.segundo_apellido, N''), N' ', e.carne) LIKE ?)
                ORDER BY e.primer_apellido, e.nombre, e.id_estudiante
                OFFSET ? ROWS FETCH NEXT ? ROWS ONLY""",
                buscar.strip(),
                termino,
                (pagina - 1) * tamano,
                tamano,
            )
            elementos = self._filas(cursor)
            cursor.execute(
                """SELECT COUNT(*) FROM estudiantes.estudiante
                WHERE activo = 1 AND (? = N'' OR CONCAT(nombre, N' ', primer_apellido,
                N' ', ISNULL(segundo_apellido, N''), N' ', carne) LIKE ?)""",
                buscar.strip(),
                termino,
            )
            fila = cursor.fetchone()
            return elementos, cast(int, fila[0]) if fila else 0

    def buscar_por_id(self, id_estudiante: int) -> dict | None:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """SELECT e.id_estudiante, e.carne, e.nombre, e.primer_apellido,
                e.segundo_apellido, e.cedula, e.seccion, e.turno, e.debe_cambiar_pin,
                e.activo, CAST(CASE WHEN f.id_estudiante IS NULL THEN 0 ELSE 1 END AS bit) AS tiene_foto
                FROM estudiantes.estudiante e
                LEFT JOIN estudiantes.fotografia f ON f.id_estudiante=e.id_estudiante
                WHERE e.id_estudiante = ?""",
                id_estudiante,
            )
            return self._fila(cursor)

    def crear(self, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """INSERT INTO estudiantes.estudiante
                (carne, nombre, primer_apellido, segundo_apellido, cedula, seccion, activo, creado_por, direccion_ip)
                OUTPUT INSERTED.id_estudiante, INSERTED.carne, INSERTED.nombre, INSERTED.primer_apellido,
                INSERTED.segundo_apellido, INSERTED.cedula, INSERTED.seccion, INSERTED.activo
                VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)""",
                datos["carne"],
                datos["nombre"],
                datos["primer_apellido"],
                datos["segundo_apellido"],
                datos["cedula"],
                datos["seccion"],
                datos["activo"],
                id_usuario,
                ip,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo crear el estudiante")
            return resultado

    def actualizar(self, id_estudiante: int, datos: dict, id_usuario: int, ip: str) -> dict:
        with self._fabrica.conexion() as conexion:
            cursor = conexion.cursor()
            cursor.execute(
                """UPDATE estudiantes.estudiante SET carne=?, nombre=?, primer_apellido=?,
                segundo_apellido=?, cedula=?, seccion=?, activo=?, actualizado_por=?, direccion_ip=?,
                fecha_actualizacion=SYSUTCDATETIME() WHERE id_estudiante=?""",
                datos["carne"],
                datos["nombre"],
                datos["primer_apellido"],
                datos["segundo_apellido"],
                datos["cedula"],
                datos["seccion"],
                datos["activo"],
                id_usuario,
                ip,
                id_estudiante,
            )
            if cursor.rowcount == 0:
                raise ValueError("Estudiante no encontrado")
            cursor.execute(
                """SELECT id_estudiante, carne, nombre, primer_apellido, segundo_apellido,
                cedula, seccion, activo FROM estudiantes.estudiante WHERE id_estudiante=?""",
                id_estudiante,
            )
            resultado = self._fila(cursor)
            if resultado is None:
                raise RuntimeError("No se pudo leer el estudiante actualizado")
            return resultado
