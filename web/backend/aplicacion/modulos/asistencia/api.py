"""Adaptador HTTP canónico del dominio de asistencia."""

from collections.abc import Callable, Iterator
from datetime import date
from typing import Any

from fastapi import APIRouter, Depends, HTTPException, Query, Request

from .esquemas import CorreccionEntrada, MarcaEntrada, MarcaSalida
from .repositorio import RepositorioAsistencia
from .servicio import ServicioAsistencia


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioAsistencia]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
    obtener_ip: Callable[[Request], str],
) -> APIRouter:
    enrutador = APIRouter(prefix="/asistencia", tags=["asistencia"])

    def servicio(
        repositorio: RepositorioAsistencia = Depends(obtener_repositorio),
    ) -> ServicioAsistencia:
        return ServicioAsistencia(repositorio)

    @enrutador.get("/marcas", response_model=list[MarcaSalida], response_model_by_alias=True)
    def listar(
        fecha: date = Query(...),
        _usuario: dict = Depends(exigir_permiso("asistencia.leer")),
        caso: ServicioAsistencia = Depends(servicio),
    ) -> list[MarcaSalida]:
        return caso.listar(fecha)

    @enrutador.post("/marcas", response_model=MarcaSalida, response_model_by_alias=True)
    def registrar(
        datos: MarcaEntrada,
        request: Request,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("asistencia.editar")),
        caso: ServicioAsistencia = Depends(servicio),
    ) -> MarcaSalida:
        try:
            return caso.registrar(datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.put(
        "/marcas/{id_marca}/correccion", response_model=MarcaSalida, response_model_by_alias=True
    )
    def corregir(
        id_marca: int,
        datos: CorreccionEntrada,
        request: Request,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("asistencia.editar")),
        caso: ServicioAsistencia = Depends(servicio),
    ) -> MarcaSalida:
        try:
            return caso.corregir(id_marca, datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    return enrutador
