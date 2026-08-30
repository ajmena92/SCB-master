from datetime import date, datetime
from typing import Literal

from pydantic import BaseModel, ConfigDict, Field

TipoPersona = Literal["estudiante", "profesor"]
EstadoComedor = Literal[1, 2]
EstadoReserva = Literal["reservada", "cancelada", "consumida"]


class PersonaComedorSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_persona: int = Field(serialization_alias="idPersona")
    tipo_persona: TipoPersona = Field(serialization_alias="tipoPersona")
    id_estudiante: int | None = Field(default=None, serialization_alias="idEstudiante")
    id_usuario: int | None = Field(default=None, serialization_alias="idUsuario")
    codigo_barras: str = Field(serialization_alias="codigoBarras")
    nombre_completo: str = Field(serialization_alias="nombreCompleto")
    colegio: str | None = None
    id_estado_comedor: EstadoComedor = Field(serialization_alias="idEstadoComedor")
    beneficio_comedor: str = Field(serialization_alias="beneficioComedor")
    activo: bool


class ProfesorComedorEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_usuario: int = Field(alias="idUsuario", ge=1)
    nombre_completo: str = Field(alias="nombreCompleto", min_length=1, max_length=220)
    colegio: str | None = Field(default=None, max_length=200)


class TiquetesEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    cantidad: int = Field(ge=1, le=1000)
    concepto: str | None = Field(default=None, max_length=250)
    clave_idempotencia: str = Field(alias="claveIdempotencia", min_length=8, max_length=100)


class CuentaTiquetesSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_cuenta: int = Field(serialization_alias="idCuenta")
    id_persona: int = Field(serialization_alias="idPersona")
    saldo: int
    reservados: int
    disponibles: int
    actualizado_en: datetime = Field(serialization_alias="actualizadoEn")


class MovimientoTiquetesSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_movimiento: int = Field(serialization_alias="idMovimiento")
    id_cuenta: int = Field(serialization_alias="idCuenta")
    tipo: Literal["recarga", "consumo", "reserva", "liberacion", "ajuste"]
    cantidad: int
    saldo_anterior: int = Field(serialization_alias="saldoAnterior")
    saldo_nuevo: int = Field(serialization_alias="saldoNuevo")
    reservados_anterior: int = Field(serialization_alias="reservadosAnterior")
    reservados_nuevo: int = Field(serialization_alias="reservadosNuevo")
    clave_idempotencia: str = Field(serialization_alias="claveIdempotencia")
    concepto: str | None = None
    creado_por: int | None = Field(default=None, serialization_alias="creadoPor")
    creado_en: datetime = Field(serialization_alias="creadoEn")


class ReservaEntrada(BaseModel):
    fecha: date


class ReservaSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_reserva: int = Field(serialization_alias="idReserva")
    id_persona: int = Field(serialization_alias="idPersona")
    fecha: date
    estado: EstadoReserva
    requiere_tiquete: bool = Field(serialization_alias="requiereTiquete")
    modalidad: Literal["beca", "tiquete"]


class IngresoEntrada(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    codigo_barras: str = Field(alias="codigoBarras", min_length=1, max_length=80)
    fecha: date


class IngresoSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    id_ingreso: int = Field(serialization_alias="idIngreso")
    id_persona: int = Field(serialization_alias="idPersona")
    fecha: date
    modalidad: Literal["beca", "tiquete"]
    codigo_horario: str | None = Field(default=None, serialization_alias="codigoHorario")
    hora_marca: datetime | None = Field(default=None, serialization_alias="horaMarca")
    marca_transporte_existente: bool = Field(
        default=False, serialization_alias="marcaTransporteExistente"
    )
    registrado_por: int | None = Field(default=None, serialization_alias="registradoPor")
    resultado: Literal["registrado", "tardio"] = "registrado"
    nombre_completo: str = Field(default="", serialization_alias="nombreCompleto")
    hora_limite: str | None = Field(default=None, serialization_alias="horaLimite")
    advertencias: list[str] = Field(default_factory=list)


class ConfiguracionOperacionSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    fecha_servidor: date = Field(serialization_alias="fechaServidor")
    hora_servidor: str = Field(serialization_alias="horaServidor")
    minutos_aviso_previo: int = Field(serialization_alias="minutosAvisoPrevio")
    permitir_marca_tardia: bool = Field(serialization_alias="permitirMarcaTardia")
    permitir_sin_marca_transporte: bool = Field(serialization_alias="permitirSinMarcaTransporte")
    horarios: list["HorarioOperacionSalida"] = Field(default_factory=list)


class HorarioOperacionSalida(BaseModel):
    model_config = ConfigDict(populate_by_name=True)

    codigo: str
    descripcion: str
    hora_limite: str = Field(serialization_alias="horaLimite")
    activo: bool


class EstadoOperacionSalida(BaseModel):
    ingresos_hoy: int = Field(serialization_alias="ingresosHoy")
    fecha_servidor: date = Field(serialization_alias="fechaServidor")


class ProfesorPortalSalida(BaseModel):
    """Proyección mínima que consume el portal autenticado del profesor."""

    model_config = ConfigDict(populate_by_name=True)

    tipo_persona: Literal["profesor"] = Field(serialization_alias="tipoPersona")
    id_persona: int = Field(serialization_alias="idPersona")
    id_usuario: int = Field(serialization_alias="idUsuario")
    nombre: str
    colegio: str | None = None
    id_estado_comedor: EstadoComedor = Field(serialization_alias="idEstadoComedor")
    beneficio_comedor: str = Field(serialization_alias="beneficioComedor")
    activo: bool
    barcode: str


class EstadoPortalProfesorSalida(BaseModel):
    """Estado de la reserva diaria, con la forma esperada por el portal."""

    model_config = ConfigDict(populate_by_name=True)

    estado: Literal["Confirmada", "Cancelada"] | None = None
    periodo_abierto: bool = Field(default=True, serialization_alias="periodoAbierto")
    periodo_cerrado: bool = Field(default=False, serialization_alias="periodoCerrado")
    descripcion_horario: str = Field(
        default="Comedor para profesores", serialization_alias="descripcionHorario"
    )
