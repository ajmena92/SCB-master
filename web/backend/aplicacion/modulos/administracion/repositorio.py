from typing import Any, Protocol, cast

from aplicacion.nucleo.base_datos import FabricaConexionSql


class RepositorioAdministracion(Protocol):
    def listar_usuarios(self) -> list[dict[str, Any]]: ...
    def crear_usuario(self, datos: dict[str, Any]) -> dict[str, Any]: ...
    def actualizar_usuario(self, id_usuario: int, datos: dict[str, Any]) -> dict[str, Any]: ...
    def listar_roles(self) -> list[dict[str, Any]]: ...
    def crear_rol(self, datos: dict[str, Any]) -> dict[str, Any]: ...
    def listar_permisos(self) -> list[dict[str, Any]]: ...


class RepositorioSqlAdministracion:
    def __init__(self, fabrica: FabricaConexionSql) -> None:
        self._fabrica = fabrica

    def listar_usuarios(self) -> list[dict[str, Any]]:
        with self._fabrica.conexion() as c:
            rows = (
                c.cursor()
                .execute("""
                SELECT u.id_usuario, u.nombre_usuario, u.activo,
                    STRING_AGG(p.clave, ',') AS permisos,
                    STRING_AGG(r.nombre, ',') AS roles
                FROM identidad.usuario u
                LEFT JOIN identidad.usuario_permiso up ON up.id_usuario=u.id_usuario
                LEFT JOIN identidad.permiso p ON p.id_permiso=up.id_permiso
                LEFT JOIN identidad.usuario_rol ur ON ur.id_usuario=u.id_usuario
                LEFT JOIN identidad.rol r ON r.id_rol=ur.id_rol
                GROUP BY u.id_usuario, u.nombre_usuario, u.activo
                ORDER BY u.nombre_usuario
            """)
                .fetchall()
            )
        return [self._usuario(r) for r in rows]

    @staticmethod
    def _usuario(row: Any) -> dict[str, Any]:
        return {
            "idUsuario": int(row[0]),
            "nombreUsuario": str(row[1]),
            "activo": bool(row[2]),
            "permisos": str(row[3]).split(",") if row[3] else [],
            "roles": str(row[4]).split(",") if row[4] else [],
        }

    def crear_usuario(self, datos: dict[str, Any]) -> dict[str, Any]:
        with self._fabrica.conexion() as c:
            cur = c.cursor()
            cur.execute(
                "INSERT INTO identidad.usuario(nombre_usuario,hash_contrasena,activo) OUTPUT INSERTED.id_usuario VALUES (?,?,?)",
                datos["nombreUsuario"],
                datos["hashContrasena"],
                datos["activo"],
            )
            fila = cast(tuple[object, ...], cur.fetchone())
            id_usuario = int(str(fila[0]))
        return {
            "idUsuario": id_usuario,
            "nombreUsuario": datos["nombreUsuario"],
            "activo": datos["activo"],
            "permisos": [],
            "roles": [],
        }

    def actualizar_usuario(self, id_usuario: int, datos: dict[str, Any]) -> dict[str, Any]:
        with self._fabrica.conexion() as c:
            c.cursor().execute(
                "UPDATE identidad.usuario SET nombre_usuario=?, activo=?, fecha_actualizacion=SYSUTCDATETIME() WHERE id_usuario=?",
                datos["nombreUsuario"],
                datos["activo"],
                id_usuario,
            )
        return next(
            (u for u in self.listar_usuarios() if u["idUsuario"] == id_usuario),
            {"idUsuario": id_usuario, **datos, "permisos": [], "roles": []},
        )

    def listar_roles(self) -> list[dict[str, Any]]:
        with self._fabrica.conexion() as c:
            rows = (
                c.cursor()
                .execute(
                    "SELECT r.id_rol,r.nombre,r.descripcion,STRING_AGG(p.clave,',') FROM identidad.rol r LEFT JOIN identidad.rol_permiso rp ON rp.id_rol=r.id_rol LEFT JOIN identidad.permiso p ON p.id_permiso=rp.id_permiso GROUP BY r.id_rol,r.nombre,r.descripcion ORDER BY r.nombre"
                )
                .fetchall()
            )
        return [
            {
                "idRol": int(str(r[0])),
                "nombre": str(r[1]),
                "descripcion": r[2],
                "permisos": str(r[3]).split(",") if r[3] else [],
            }
            for r in rows
        ]

    def crear_rol(self, datos: dict[str, Any]) -> dict[str, Any]:
        with self._fabrica.conexion() as c:
            cur = c.cursor()
            cur.execute(
                "INSERT INTO identidad.rol(nombre,descripcion) OUTPUT INSERTED.id_rol VALUES (?,?)",
                datos["nombre"],
                datos.get("descripcion"),
            )
            fila = cast(tuple[object, ...], cur.fetchone())
            id_rol = int(str(fila[0]))
        return {"idRol": id_rol, **datos}

    def listar_permisos(self) -> list[dict[str, Any]]:
        with self._fabrica.conexion() as c:
            rows = (
                c.cursor()
                .execute("SELECT clave,descripcion,activo FROM identidad.permiso ORDER BY clave")
                .fetchall()
            )
        return [{"clave": str(r[0]), "descripcion": r[1], "activo": bool(r[2])} for r in rows]
