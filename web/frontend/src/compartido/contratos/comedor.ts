/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface ConfiguracionOperacionSalida {
  fechaServidor: string;
  horaServidor: string;
  horarios?: Array<HorarioOperacionSalida>;
  minutosAvisoPrevio: number;
  permitirMarcaTardia: boolean;
  permitirSinMarcaTransporte: boolean;
}

export interface CuentaTiquetesSalida {
  actualizadoEn: string;
  disponibles: number;
  idCuenta: number;
  idPersona: number;
  reservados: number;
  saldo: number;
}

export interface EstadoOperacionSalida {
  fechaServidor: string;
  ingresosHoy: number;
}

export interface EstadoPortalProfesorSalida {
  descripcionHorario?: string;
  estado?: "Confirmada" | "Cancelada" | null;
  periodoAbierto?: boolean;
  periodoCerrado?: boolean;
}

export interface HorarioOperacionSalida {
  activo: boolean;
  codigo: string;
  descripcion: string;
  horaLimite: string;
}

export interface IngresoEntrada {
  codigoBarras: string;
  fecha: string;
}

export interface IngresoSalida {
  advertencias?: Array<string>;
  codigoHorario?: string | null;
  fecha: string;
  horaLimite?: string | null;
  horaMarca?: string | null;
  idIngreso: number;
  idPersona: number;
  marcaTransporteExistente?: boolean;
  modalidad: "beca" | "tiquete";
  nombreCompleto?: string;
  registradoPor?: number | null;
  resultado?: "registrado" | "tardio";
}

export interface MovimientoTiquetesSalida {
  cantidad: number;
  claveIdempotencia: string;
  concepto?: string | null;
  creadoEn: string;
  creadoPor?: number | null;
  idCuenta: number;
  idMovimiento: number;
  reservadosAnterior: number;
  reservadosNuevo: number;
  saldoAnterior: number;
  saldoNuevo: number;
  tipo: "recarga" | "consumo" | "reserva" | "liberacion" | "ajuste";
}

export interface PersonaComedorSalida {
  activo: boolean;
  beneficioComedor: string;
  codigoBarras: string;
  colegio?: string | null;
  idEstadoComedor: 1 | 2;
  idEstudiante?: number | null;
  idPersona: number;
  idUsuario?: number | null;
  nombreCompleto: string;
  tipoPersona: "estudiante" | "profesor";
}

export interface ProfesorComedorEntrada {
  colegio?: string | null;
  idUsuario: number;
  nombreCompleto: string;
}

export interface ProfesorPortalSalida {
  activo: boolean;
  barcode: string;
  beneficioComedor: string;
  colegio?: string | null;
  idEstadoComedor: 1 | 2;
  idPersona: number;
  idUsuario: number;
  nombre: string;
  tipoPersona: "profesor";
}

export interface ReservaEntrada {
  fecha: string;
}

export interface ReservaSalida {
  estado: "reservada" | "cancelada" | "consumida";
  fecha: string;
  idPersona: number;
  idReserva: number;
  modalidad: "beca" | "tiquete";
  requiereTiquete: boolean;
}

export interface TiquetesEntrada {
  cantidad: number;
  claveIdempotencia: string;
  concepto?: string | null;
}
