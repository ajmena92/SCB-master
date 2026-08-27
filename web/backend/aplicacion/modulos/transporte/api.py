"""Adaptador HTTP del modulo de transporte."""

from collections.abc import Callable, Iterator
from typing import Any

from fastapi import APIRouter, Depends, HTTPException, Request

from aplicacion.modulos.transporte.esquemas import RutaEntrada, RutaSalida
from aplicacion.modulos.transporte.repositorio import RepositorioRutas
from aplicacion.modulos.transporte.servicio import ServicioRutas


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioRutas]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
    obtener_ip: Callable[[Request], str],
) -> APIRouter:
    enrutador = APIRouter(prefix="/transporte", tags=["transporte"])
    permiso = exigir_permiso("rutas.administrar")

    def servicio(repositorio: RepositorioRutas = Depends(obtener_repositorio)) -> ServicioRutas:
        return ServicioRutas(repositorio)

    @enrutador.get("/rutas", response_model=list[RutaSalida])
    def listar_rutas(
        _usuario: dict = Depends(permiso), caso_uso: ServicioRutas = Depends(servicio)
    ) -> list[RutaSalida]:
        return caso_uso.listar()

    @enrutador.get("/rutas/paleta")
    def listar_paleta(_usuario: dict = Depends(permiso), caso_uso: ServicioRutas = Depends(servicio)) -> list[dict]:
        return caso_uso.listar_paleta()

    @enrutador.post("/rutas", response_model=RutaSalida)
    def crear_ruta(
        datos: RutaEntrada, request: Request, _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(permiso), caso_uso: ServicioRutas = Depends(servicio),
    ) -> RutaSalida:
        try:
            return caso_uso.crear(datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.put("/rutas/{id_ruta}", response_model=RutaSalida)
    def editar_ruta(
        id_ruta: int, datos: RutaEntrada, request: Request, _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(permiso), caso_uso: ServicioRutas = Depends(servicio),
    ) -> RutaSalida:
        try:
            return caso_uso.editar(id_ruta, datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    return enrutador
