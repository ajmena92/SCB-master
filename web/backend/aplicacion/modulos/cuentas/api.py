"""Adaptador HTTP canónico del dominio de cuentas."""

from collections.abc import Callable, Iterator
from typing import Any

from fastapi import APIRouter, Depends, HTTPException, Request

from .esquemas import MovimientoEntrada, MovimientoSalida, SaldoSalida
from .repositorio import RepositorioCuentas
from .servicio import ServicioCuentas


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioCuentas]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
    obtener_ip: Callable[[Request], str],
) -> APIRouter:
    enrutador = APIRouter(prefix="/cuentas", tags=["cuentas"])

    def servicio(repo: RepositorioCuentas = Depends(obtener_repositorio)) -> ServicioCuentas:
        return ServicioCuentas(repo)

    @enrutador.get(
        "/{id_estudiante}/saldo", response_model=SaldoSalida, response_model_by_alias=True
    )
    def saldo(
        id_estudiante: int,
        _u: dict = Depends(exigir_permiso("cuentas.leer")),
        caso: ServicioCuentas = Depends(servicio),
    ) -> SaldoSalida:
        try:
            return caso.saldo(id_estudiante)
        except ValueError as exc:
            raise HTTPException(404, str(exc)) from exc

    @enrutador.post(
        "/{id_estudiante}/movimientos",
        response_model=MovimientoSalida,
        response_model_by_alias=True,
    )
    def movimiento(
        id_estudiante: int,
        datos: MovimientoEntrada,
        request: Request,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("cuentas.editar")),
        caso: ServicioCuentas = Depends(servicio),
    ) -> MovimientoSalida:
        try:
            return caso.registrar_movimiento(
                id_estudiante, datos, int(usuario["idUsuario"]), obtener_ip(request)
            )
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    return enrutador
