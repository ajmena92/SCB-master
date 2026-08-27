/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface MovimientoEntrada {
  claveIdempotencia: string;
  concepto?: string | null;
  monto: number | string;
  tipo: "recarga" | "consumo" | "ajuste";
}

export interface MovimientoSalida {
  claveIdempotencia: string;
  concepto: string | null;
  creadoEn: string;
  idCuenta: number;
  idMovimiento: number;
  monto: string;
  saldoAnterior: string;
  saldoNuevo: string;
  tipo: "recarga" | "consumo" | "ajuste";
}

export interface SaldoSalida {
  actualizadoEn: string;
  idCuenta: number;
  idEstudiante: number;
  saldo: string;
}
