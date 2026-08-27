"""Adaptador HTTP canónico del dominio de beneficios."""

from collections.abc import Callable, Iterator
from typing import Any

from fastapi import APIRouter, Depends, HTTPException, Request

from .esquemas import AsignacionEntrada, AsignacionSalida, BeneficioEntrada, BeneficioSalida
from .repositorio import RepositorioBeneficios
from .servicio import ServicioBeneficios


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioBeneficios]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
    obtener_ip: Callable[[Request], str],
) -> APIRouter:
    enrutador = APIRouter(prefix="/beneficios", tags=["beneficios"])

    def servicio(repo: RepositorioBeneficios = Depends(obtener_repositorio)) -> ServicioBeneficios:
        return ServicioBeneficios(repo)

    @enrutador.get("", response_model=list[BeneficioSalida], response_model_by_alias=True)
    def listar(
        _u: dict = Depends(exigir_permiso("beneficios.leer")),
        caso: ServicioBeneficios = Depends(servicio),
    ) -> list[BeneficioSalida]:
        return caso.listar()

    @enrutador.post("", response_model=BeneficioSalida, response_model_by_alias=True)
    def crear(
        datos: BeneficioEntrada,
        request: Request,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("beneficios.editar")),
        caso: ServicioBeneficios = Depends(servicio),
    ) -> BeneficioSalida:
        try:
            return caso.crear(datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.put("/{id_beneficio}", response_model=BeneficioSalida, response_model_by_alias=True)
    def editar(
        id_beneficio: int,
        datos: BeneficioEntrada,
        request: Request,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("beneficios.editar")),
        caso: ServicioBeneficios = Depends(servicio),
    ) -> BeneficioSalida:
        try:
            return caso.editar(id_beneficio, datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.get(
        "/estudiantes/{id_estudiante}",
        response_model=AsignacionSalida,
        response_model_by_alias=True,
    )
    def obtener_asignacion(
        id_estudiante: int,
        _u: dict = Depends(exigir_permiso("beneficios.leer")),
        caso: ServicioBeneficios = Depends(servicio),
    ) -> AsignacionSalida:
        return caso.obtener_asignacion(id_estudiante)

    @enrutador.put(
        "/estudiantes/{id_estudiante}",
        response_model=AsignacionSalida,
        response_model_by_alias=True,
    )
    def asignar(
        id_estudiante: int,
        datos: AsignacionEntrada,
        request: Request,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict[str, Any] = Depends(exigir_permiso("beneficios.editar")),
        caso: ServicioBeneficios = Depends(servicio),
    ) -> AsignacionSalida:
        try:
            return caso.asignar(
                id_estudiante, datos, int(usuario["idUsuario"]), obtener_ip(request)
            )
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    return enrutador
