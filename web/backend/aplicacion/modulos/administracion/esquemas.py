from pydantic import BaseModel, ConfigDict, Field


class UsuarioSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_usuario: int = Field(alias="idUsuario")
    nombre_usuario: str = Field(alias="nombreUsuario")
    activo: bool
    permisos: list[str] = []
    roles: list[str] = []


class UsuarioEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    nombre_usuario: str = Field(alias="nombreUsuario", min_length=1, max_length=100)
    contrasena: str | None = Field(default=None, min_length=10, max_length=200)
    activo: bool = True
    roles: list[str] = []
    permisos: list[str] = []


class RolSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    id_rol: int = Field(alias="idRol")
    nombre: str
    descripcion: str | None = None
    permisos: list[str] = []


class RolEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    nombre: str = Field(min_length=1, max_length=100)
    descripcion: str | None = Field(default=None, max_length=300)
    permisos: list[str] = []


class PermisoSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)
    clave: str
    descripcion: str | None = None
    activo: bool = True
