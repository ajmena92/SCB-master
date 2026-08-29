"""Portal autenticado de profesores para la operación del comedor."""

from __future__ import annotations

from collections.abc import Callable, Iterator
from datetime import date

from fastapi import APIRouter, Cookie, Depends, HTTPException, status

from aplicacion.modulos.identidad.servicio import AutenticacionFallida, ServicioIdentidad

from .esquemas import EstadoPortalProfesorSalida, ProfesorPortalSalida, ReservaSalida
from .repositorio import RepositorioComedor
from .servicio import ServicioComedor


def crear_enrutador_profesores(
    obtener_repositorio: Callable[[], Iterator[RepositorioComedor]],
    obtener_identidad: Callable[[], ServicioIdentidad],
    obtener_menu: Callable[..., Iterator],
    exigir_csrf: Callable[..., object],
    obtener_fecha_local: Callable[[], date],
) -> APIRouter:
    """Expone únicamente operaciones propias del profesor autenticado."""

    enrutador = APIRouter(prefix="/profesores", tags=["comedor-profesores"])

    def servicio(
        repositorio: RepositorioComedor = Depends(obtener_repositorio),
    ) -> ServicioComedor:
        return ServicioComedor(repositorio)

    def profesor_actual(
        repositorio: RepositorioComedor = Depends(obtener_repositorio),
        identidad: ServicioIdentidad = Depends(obtener_identidad),
        id_sesion: str | None = Cookie(default=None, alias="id_sesion"),
        secreto: str | None = Cookie(default=None, alias="secreto_sesion"),
    ) -> dict:
        if not id_sesion or not secreto:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, "La sesión no es válida")
        try:
            sesion = identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(status.HTTP_401_UNAUTHORIZED, str(exc)) from exc
        try:
            return repositorio.persona_por_usuario(sesion.id_usuario)
        except ValueError as exc:
            raise HTTPException(status.HTTP_403_FORBIDDEN, str(exc)) from exc

    @enrutador.get("/menu")
    def menu(
        _profesor: dict = Depends(profesor_actual),
        repositorio_menu=Depends(obtener_menu),
    ) -> dict[str, list[dict]]:
        return {"menu": repositorio_menu.listar()}

    @enrutador.get(
        "/carnet",
        response_model=ProfesorPortalSalida,
        response_model_by_alias=True,
    )
    def carnet(profesor: dict = Depends(profesor_actual)) -> ProfesorPortalSalida:
        return ProfesorPortalSalida(
            tipo_persona="profesor",
            id_persona=int(profesor["id_persona"]),
            id_usuario=int(profesor["id_usuario"]),
            nombre=str(profesor["nombre_completo"]),
            colegio=profesor.get("colegio"),
            id_estado_comedor=int(profesor["id_estado_comedor"]),
            beneficio_comedor=str(profesor["beneficio_comedor"]),
            activo=bool(profesor["activo"]),
            barcode=str(profesor["codigo_barras"]),
        )

    @enrutador.get(
        "/asistencia/hoy",
        response_model=EstadoPortalProfesorSalida,
        response_model_by_alias=True,
    )
    def asistencia_hoy(
        profesor: dict = Depends(profesor_actual),
        caso: ServicioComedor = Depends(servicio),
    ) -> EstadoPortalProfesorSalida:
        reserva = caso.estado_reserva(int(profesor["id_persona"]), obtener_fecha_local())
        estado = None
        if reserva is not None:
            estado = "Cancelada" if reserva.estado == "cancelada" else "Confirmada"
        return EstadoPortalProfesorSalida(estado=estado)

    @enrutador.post(
        "/asistencia/{accion}",
        response_model=EstadoPortalProfesorSalida,
        response_model_by_alias=True,
    )
    def registrar_asistencia(
        accion: str,
        profesor: dict = Depends(profesor_actual),
        _csrf: dict = Depends(exigir_csrf),
        caso: ServicioComedor = Depends(servicio),
    ) -> EstadoPortalProfesorSalida:
        fecha = obtener_fecha_local()
        try:
            if accion == "confirm":
                reserva: ReservaSalida = caso.reservar(
                    int(profesor["id_persona"]), fecha, int(profesor["id_usuario"])
                )
                estado = "Confirmada" if reserva.estado != "cancelada" else "Cancelada"
            elif accion == "decline":
                reserva = caso.cancelar(
                    int(profesor["id_persona"]), fecha, int(profesor["id_usuario"])
                )
                estado = "Cancelada" if reserva.estado == "cancelada" else "Confirmada"
            else:
                raise HTTPException(400, "Acción de asistencia no válida")
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc
        return EstadoPortalProfesorSalida(estado=estado)

    return enrutador
