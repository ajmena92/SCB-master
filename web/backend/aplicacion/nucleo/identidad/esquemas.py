"""Contratos tipados independientes del transporte HTTP."""

from __future__ import annotations

from datetime import datetime

from pydantic import BaseModel, ConfigDict, Field


class CredencialesUsuario(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_usuario: int = Field(alias="idUsuario", gt=0)
    nombre_usuario: str = Field(alias="nombreUsuario", min_length=1)
    hash_contrasena: str = Field(alias="hashContrasena", min_length=1)
    activo: bool = True
    permisos: frozenset[str] = frozenset()


class SesionPersistida(BaseModel):
    id_sesion: str = Field(alias="idSesion", min_length=1)
    id_usuario: int = Field(alias="idUsuario", gt=0)
    secreto_hash: str = Field(alias="secretoHash", min_length=1)
    expira_en: datetime = Field(alias="expiraEn")
    csrf_hash: str | None = Field(default=None, alias="csrfHash")
    revocada: bool = False


class ResultadoAutenticacion(BaseModel):
    id_sesion: str = Field(alias="idSesion", min_length=1)
    id_usuario: int = Field(alias="idUsuario", gt=0)
    nombre_usuario: str = Field(alias="nombreUsuario", min_length=1)
    secreto_sesion: str = Field(alias="secretoSesion", min_length=1)
    expira_en: datetime = Field(alias="expiraEn")
    permisos: frozenset[str] = frozenset()
