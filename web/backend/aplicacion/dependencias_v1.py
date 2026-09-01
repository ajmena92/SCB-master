"""Dependencias de autenticacion y autorizacion para v1."""

from fastapi import Depends, Header, HTTPException


def crear_dependencias(obtener_servicio):
    async def actual(
        authorization: str | None = Header(default=None), servicio=Depends(obtener_servicio)
    ) -> dict:
        if not authorization or not authorization.startswith("Bearer "):
            raise HTTPException(401, "Autenticacion requerida")
        return servicio.identidad_por_token(authorization[7:])

    async def administrativo(identidad: dict = Depends(actual)) -> dict:
        if identidad["tipo"] != "administracion":
            raise HTTPException(403, "Se requiere una cuenta administrativa")
        if identidad["cuenta"].vinculacion_pendiente:
            raise HTTPException(403, "Debe completar la vinculacion inicial")
        if identidad["cuenta"].cambio_contrasena_obligatorio:
            raise HTTPException(403, "Debe cambiar la contrasena temporal")
        return identidad

    async def portal_operativo(identidad: dict = Depends(actual)) -> dict:
        if identidad["tipo"] == "portal" and identidad.get("cambioObligatorio"):
            raise HTTPException(403, "Debe cambiar el PIN temporal antes de continuar")
        return identidad

    async def administrador(identidad: dict = Depends(administrativo)) -> dict:
        if identidad["rol"] != "administrador":
            raise HTTPException(403, "Se requiere rol administrador")
        return identidad

    def exigir_permiso(clave: str):
        async def dependencia(identidad: dict = Depends(administrativo)) -> dict:
            if identidad["rol"] != "administrador" and clave not in identidad["permisos"]:
                raise HTTPException(403, "No tiene permiso para esta operacion")
            return identidad

        return dependencia

    def exigir_alguno(*claves: str):
        async def dependencia(identidad: dict = Depends(administrativo)) -> dict:
            if identidad["rol"] != "administrador" and not set(claves).intersection(
                identidad["permisos"]
            ):
                raise HTTPException(403, "No tiene permiso para esta operacion")
            return identidad

        return dependencia

    return actual, portal_operativo, administrativo, administrador, exigir_permiso, exigir_alguno
