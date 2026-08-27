from collections.abc import Callable, Iterator

from fastapi import APIRouter, Depends, HTTPException

from .esquemas import PermisoSalida, RolEntrada, RolSalida, UsuarioEntrada, UsuarioSalida
from .repositorio import RepositorioAdministracion
from .servicio import ServicioAdministracion


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioAdministracion]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
) -> APIRouter:
    r = APIRouter(prefix="/administracion", tags=["administracion"])

    def servicio(
        repo: RepositorioAdministracion = Depends(obtener_repositorio),
    ) -> ServicioAdministracion:
        return ServicioAdministracion(repo)

    @r.get("/usuarios", response_model=list[UsuarioSalida], response_model_by_alias=True)
    def usuarios(
        _: dict = Depends(exigir_permiso("administracion.usuarios.leer")),
        s: ServicioAdministracion = Depends(servicio),
    ) -> list[UsuarioSalida]:
        return s.usuarios()

    @r.post("/usuarios", response_model=UsuarioSalida, response_model_by_alias=True)
    def crear_usuario(
        datos: UsuarioEntrada,
        _: dict = Depends(exigir_csrf),
        __: dict = Depends(exigir_permiso("administracion.usuarios.editar")),
        s: ServicioAdministracion = Depends(servicio),
    ) -> UsuarioSalida:
        try:
            return s.crear_usuario(datos)
        except ValueError as e:
            raise HTTPException(400, str(e)) from e

    @r.put("/usuarios/{id_usuario}", response_model=UsuarioSalida, response_model_by_alias=True)
    def editar_usuario(
        id_usuario: int,
        datos: UsuarioEntrada,
        _: dict = Depends(exigir_csrf),
        __: dict = Depends(exigir_permiso("administracion.usuarios.editar")),
        s: ServicioAdministracion = Depends(servicio),
    ) -> UsuarioSalida:
        return s.actualizar_usuario(id_usuario, datos)

    @r.get("/roles", response_model=list[RolSalida], response_model_by_alias=True)
    def roles(
        _: dict = Depends(exigir_permiso("administracion.usuarios.leer")),
        s: ServicioAdministracion = Depends(servicio),
    ) -> list[RolSalida]:
        return s.roles()

    @r.post("/roles", response_model=RolSalida, response_model_by_alias=True)
    def crear_rol(
        datos: RolEntrada,
        _: dict = Depends(exigir_csrf),
        __: dict = Depends(exigir_permiso("administracion.permisos.editar")),
        s: ServicioAdministracion = Depends(servicio),
    ) -> RolSalida:
        return s.crear_rol(datos)

    @r.get("/permisos", response_model=list[PermisoSalida], response_model_by_alias=True)
    def permisos(
        _: dict = Depends(exigir_permiso("administracion.usuarios.leer")),
        s: ServicioAdministracion = Depends(servicio),
    ) -> list[PermisoSalida]:
        return s.permisos()

    return r
