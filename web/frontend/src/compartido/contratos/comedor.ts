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
