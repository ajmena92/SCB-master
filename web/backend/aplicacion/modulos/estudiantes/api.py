"""Adaptador HTTP canónico del dominio de estudiantes."""

from collections.abc import Callable, Iterator
from typing import Any

from fastapi import APIRouter, Depends, HTTPException, Query, Request

from .esquemas import EstudianteEntrada, EstudianteSalida, PaginaEstudiantes
from .repositorio import RepositorioEstudiantes
from .servicio import ServicioEstudiantes


def crear_enrutador(obtener_repositorio: Callable[[], Iterator[RepositorioEstudiantes]], exigir_permiso: Callable[..., Callable], exigir_csrf: Callable, obtener_ip: Callable[[Request], str]) -> APIRouter:
    enrutador = APIRouter(prefix="/estudiantes", tags=["estudiantes"])

    def servicio(repositorio: RepositorioEstudiantes = Depends(obtener_repositorio)) -> ServicioEstudiantes:
        return ServicioEstudiantes(repositorio)

    @enrutador.get("", response_model=PaginaEstudiantes, response_model_by_alias=True)
    def listar(pagina: int = Query(1, ge=1), tamano: int = Query(25, ge=1, le=100), buscar: str = "", _u: dict = Depends(exigir_permiso("estudiantes.leer")), caso: ServicioEstudiantes = Depends(servicio)) -> PaginaEstudiantes:
        return caso.listar(pagina, tamano, buscar)

    @enrutador.get("/{id_estudiante}", response_model=EstudianteSalida, response_model_by_alias=True)
    def obtener(id_estudiante: int, _u: dict = Depends(exigir_permiso("estudiantes.leer")), caso: ServicioEstudiantes = Depends(servicio)) -> EstudianteSalida:
        try:
            return caso.obtener(id_estudiante)
        except ValueError as exc:
            raise HTTPException(404, str(exc)) from exc

    @enrutador.post("", response_model=EstudianteSalida, response_model_by_alias=True)
    def crear(datos: EstudianteEntrada, request: Request, _csrf: dict = Depends(exigir_csrf), usuario: dict[str, Any] = Depends(exigir_permiso("estudiantes.editar")), caso: ServicioEstudiantes = Depends(servicio)) -> EstudianteSalida:
        try:
            return caso.crear(datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.put("/{id_estudiante}", response_model=EstudianteSalida, response_model_by_alias=True)
    def editar(id_estudiante: int, datos: EstudianteEntrada, request: Request, _csrf: dict = Depends(exigir_csrf), usuario: dict[str, Any] = Depends(exigir_permiso("estudiantes.editar")), caso: ServicioEstudiantes = Depends(servicio)) -> EstudianteSalida:
        try:
            return caso.editar(id_estudiante, datos, int(usuario["idUsuario"]), obtener_ip(request))
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    return enrutador
