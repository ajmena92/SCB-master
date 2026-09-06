/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface AutorizacionEntrada {
  cedula: string;
  decision: "aprobada" | "rechazada";
  fecha: string;
  motivo?: string | null;
}

export interface CancelacionReservaEntrada {
  cedula?: string | null;
  fecha: string;
}

export interface HorarioReservaEntrada {
  horaLimite: string;
  turno: string;
}

export interface IngresoEntrada {
  cedula: string;
  fecha: string;
}

export interface ReservaEntrada {
  cedula?: string | null;
  fecha: string;
}

export interface TarifaEntrada {
  fechaFin?: string | null;
  fechaInicio: string;
  monto: number | string;
  tipoPersona: "estudiante" | "profesor";
}

export interface VentaEntrada {
  cantidad: number;
  cedula: string;
  medioPago?: string;
}

export type IngresoSalida = { idIngreso: number; nombreCompleto: string; horaMarca?: string; resultado?: string; modalidad?: string; advertencias?: string[] };
export type ReservaSalida = Record<string, unknown>;
export type CuentaTiquetesSalida = { idCuenta: number; saldo: number; disponibles: number; reservados?: number };
export type TiquetesEntrada = { cantidad: number; concepto: string; claveIdempotencia: string };
export type ConfiguracionOperacionSalida = { horarios: Array<Record<string, unknown>>; horaServidor?: string };