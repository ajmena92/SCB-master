"""Adaptador HTTP del catálogo y operación del comedor."""

from collections.abc import Callable, Iterator
from datetime import date

from fastapi import APIRouter, Cookie, Depends, Header, HTTPException, Query, status

from aplicacion.modulos.identidad.servicio import AutenticacionFallida, ServicioIdentidad

from .esquemas import (
    CuentaTiquetesSalida,
    ConfiguracionOperacionSalida,
    EstadoOperacionSalida,
    IngresoEntrada,
    IngresoSalida,
    MovimientoTiquetesSalida,
    PersonaComedorSalida,
    ProfesorComedorEntrada,
    ReservaEntrada,
    ReservaSalida,
    TiquetesEntrada,
)
from .errores import ErrorOperacionComedor, IdempotenciaIncompatible
from .repositorio import RepositorioComedor
from .servicio import ServicioComedor


def crear_enrutador(
    obtener_repositorio: Callable[[], Iterator[RepositorioComedor]],
    exigir_permiso: Callable[..., Callable],
    exigir_csrf: Callable,
    obtener_identidad_estudiante: Callable[[], ServicioIdentidad],
) -> APIRouter:
    enrutador = APIRouter(prefix="/comedor", tags=["comedor"])

    def servicio(repo: RepositorioComedor = Depends(obtener_repositorio)) -> ServicioComedor:
        return ServicioComedor(repo)

    @enrutador.get(
        "/personas",
        response_model=list[PersonaComedorSalida],
        response_model_by_alias=True,
    )
    def personas(
        tipo_persona: str | None = Query(None, alias="tipoPersona"),
        incluir_inactivas: bool = Query(False, alias="incluirInactivas"),
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> list[PersonaComedorSalida]:
        try:
            return caso.personas(tipo_persona, incluir_inactivas)
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.post(
        "/personas/profesores",
        response_model=PersonaComedorSalida,
        response_model_by_alias=True,
    )
    def crear_profesor(
        datos: ProfesorComedorEntrada,
        _csrf: dict = Depends(exigir_csrf),
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> PersonaComedorSalida:
        try:
            return caso.crear_profesor(datos)
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.get(
        "/personas/{id_persona}/cuenta",
        response_model=CuentaTiquetesSalida,
        response_model_by_alias=True,
    )
    def cuenta(
        id_persona: int,
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> CuentaTiquetesSalida:
        try:
            return caso.cuenta(id_persona)
        except ValueError as exc:
            raise HTTPException(404, str(exc)) from exc

    @enrutador.get(
        "/personas/{id_persona}/movimientos",
        response_model=list[MovimientoTiquetesSalida],
        response_model_by_alias=True,
    )
    def movimientos(
        id_persona: int,
        limite: int = Query(50, ge=1, le=100),
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> list[MovimientoTiquetesSalida]:
        try:
            return caso.movimientos(id_persona, limite)
        except ValueError as exc:
            raise HTTPException(404, str(exc)) from exc

    @enrutador.post(
        "/personas/{id_persona}/tiquetes",
        response_model=MovimientoTiquetesSalida,
        response_model_by_alias=True,
        status_code=status.HTTP_201_CREATED,
    )
    def recargar(
        id_persona: int,
        datos: TiquetesEntrada,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> MovimientoTiquetesSalida:
        try:
            return caso.recargar(id_persona, datos, int(usuario["idUsuario"]))
        except IdempotenciaIncompatible as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc
        except ValueError as exc:
            raise HTTPException(400, str(exc)) from exc

    @enrutador.post("/reservas", response_model=ReservaSalida, response_model_by_alias=True)
    def reservar_administrativa(
        id_persona: int,
        datos: ReservaEntrada,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> ReservaSalida:
        try:
            return caso.reservar(id_persona, datos.fecha, int(usuario["idUsuario"]))
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    @enrutador.delete(
        "/reservas",
        response_model=ReservaSalida,
        response_model_by_alias=True,
    )
    def cancelar_administrativa(
        id_persona: int,
        fecha: date = Query(...),
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> ReservaSalida:
        try:
            return caso.cancelar(id_persona, fecha, int(usuario["idUsuario"]))
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    @enrutador.post(
        "/reservas/profesor",
        response_model=ReservaSalida,
        response_model_by_alias=True,
    )
    def reservar_profesor(
        datos: ReservaEntrada,
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
        repo: RepositorioComedor = Depends(obtener_repositorio),
    ) -> ReservaSalida:
        personas_registradas = repo.personas("profesor")
        persona = next(
            (
                fila
                for fila in personas_registradas
                if int(fila.get("id_usuario") or 0) == int(usuario["idUsuario"])
            ),
            None,
        )
        if persona is None:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                "El usuario no está registrado como profesor del comedor",
            )
        try:
            return caso.reservar(int(persona["id_persona"]), datos.fecha, int(usuario["idUsuario"]))
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    @enrutador.get("/operacion/configuracion", response_model=ConfiguracionOperacionSalida, response_model_by_alias=True)
    def configuracion_operacion(
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        repo: RepositorioComedor = Depends(obtener_repositorio),
    ) -> ConfiguracionOperacionSalida:
        return ConfiguracionOperacionSalida(**repo.configuracion_operacion())

    @enrutador.get("/operacion/estado", response_model=EstadoOperacionSalida, response_model_by_alias=True)
    def estado_operacion(
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        repo: RepositorioComedor = Depends(obtener_repositorio),
    ) -> EstadoOperacionSalida:
        return EstadoOperacionSalida(**repo.estado_operacion())

    @enrutador.get("/operacion/historial", response_model=list[IngresoSalida], response_model_by_alias=True)
    def historial_operacion(
        fecha: date = Query(...),
        limite: int = Query(25, ge=1, le=100),
        _usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        repo: RepositorioComedor = Depends(obtener_repositorio),
    ) -> list[IngresoSalida]:
        return [IngresoSalida(**fila) for fila in repo.historial_operacion(fecha, limite)]

    @enrutador.post("/operacion/ingresos", response_model=IngresoSalida, response_model_by_alias=True)
    def ingresar(
        datos: IngresoEntrada,
        terminal_id: str | None = Header(default=None, alias="X-Terminal-Id", max_length=100),
        _csrf: dict = Depends(exigir_csrf),
        usuario: dict = Depends(exigir_permiso("comedor.registrar")),
        caso: ServicioComedor = Depends(servicio),
    ) -> IngresoSalida:
        try:
            return caso.ingresar(
                datos.codigo_barras,
                datos.fecha,
                int(usuario["idUsuario"]),
                terminal_id,
            )
        except ErrorOperacionComedor as exc:
            raise HTTPException(
                status.HTTP_409_CONFLICT,
                detail={"codigo": exc.codigo, "mensaje": exc.mensaje},
            ) from exc
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    def validar_sesion_estudiante(
        identidad: ServicioIdentidad = Depends(obtener_identidad_estudiante),
        id_sesion: str | None = Cookie(default=None, alias="id_sesion"),
        secreto: str | None = Cookie(default=None, alias="secreto_sesion"),
        token: str | None = Header(default=None, alias="X-CSRF-Token"),
        cookie: str | None = Cookie(default=None, alias="csrf_token"),
    ) -> int:
        if not id_sesion or not secreto:
            raise HTTPException(401, "La sesión no es válida")
        try:
            sesion = identidad.validar_sesion(id_sesion, secreto)
        except AutenticacionFallida as exc:
            raise HTTPException(401, str(exc)) from exc
        if not token or token != cookie or not identidad.validar_csrf(sesion, token):
            raise HTTPException(403, "El token CSRF no es válido")
        return sesion.id_usuario

    @enrutador.post(
        "/reservas/estudiante",
        response_model=ReservaSalida,
        response_model_by_alias=True,
    )
    def reservar_estudiante(
        datos: ReservaEntrada,
        id_estudiante: int = Depends(validar_sesion_estudiante),
        caso: ServicioComedor = Depends(servicio),
    ) -> ReservaSalida:
        try:
            return caso.reservar_estudiante(id_estudiante, datos.fecha, None)
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    @enrutador.delete(
        "/reservas/estudiante",
        response_model=ReservaSalida,
        response_model_by_alias=True,
    )
    def cancelar_estudiante(
        fecha: date = Query(...),
        id_estudiante: int = Depends(validar_sesion_estudiante),
        caso: ServicioComedor = Depends(servicio),
    ) -> ReservaSalida:
        try:
            return caso.cancelar_estudiante(id_estudiante, fecha, None)
        except ValueError as exc:
            raise HTTPException(status.HTTP_409_CONFLICT, str(exc)) from exc

    return enrutador
