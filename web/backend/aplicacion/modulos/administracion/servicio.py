from aplicacion.modulos.identidad.servicio import preparar_hash_contrasena

from .esquemas import PermisoSalida, RolEntrada, RolSalida, UsuarioEntrada, UsuarioSalida
from .repositorio import RepositorioAdministracion


class ServicioAdministracion:
    def __init__(self, repositorio: RepositorioAdministracion) -> None:
        self._repositorio = repositorio

    def usuarios(self) -> list[UsuarioSalida]:
        return [UsuarioSalida(**u) for u in self._repositorio.listar_usuarios()]

    def crear_usuario(self, datos: UsuarioEntrada) -> UsuarioSalida:
        if not datos.contrasena:
            raise ValueError("La contraseña es obligatoria")
        salida = self._repositorio.crear_usuario(
            {
                "nombreUsuario": datos.nombre_usuario,
                "hashContrasena": preparar_hash_contrasena(datos.contrasena),
                "activo": datos.activo,
            }
        )
        return UsuarioSalida(**salida)

    def actualizar_usuario(self, id_usuario: int, datos: UsuarioEntrada) -> UsuarioSalida:
        return UsuarioSalida(
            **self._repositorio.actualizar_usuario(
                id_usuario, {"nombreUsuario": datos.nombre_usuario, "activo": datos.activo}
            )
        )

    def roles(self) -> list[RolSalida]:
        return [RolSalida(**r) for r in self._repositorio.listar_roles()]

    def crear_rol(self, datos: RolEntrada) -> RolSalida:
        return RolSalida(**self._repositorio.crear_rol(datos.model_dump(by_alias=True)))

    def permisos(self) -> list[PermisoSalida]:
        return [PermisoSalida(**p) for p in self._repositorio.listar_permisos()]
