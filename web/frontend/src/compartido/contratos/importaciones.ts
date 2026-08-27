/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface BodyEjecutarApiV1ImportacionesLotesPost {
  archivo: string;
}

export interface BodyPrevisualizarApiV1ImportacionesPrevisualizacionesPost {
  archivo: string;
}

export interface ErrorFila {
  fila: number;
  mensaje: string;
}

export interface LoteSalida {
  creadoEn: string;
  errores: Array<ErrorFila>;
  estado: string;
  idLote: number;
  nombreArchivo: string;
  revertidoEn?: string | null;
  totalFilas: number;
}

export interface Previsualizacion {
  cabeceras: Array<string>;
  errores: Array<ErrorFila>;
  filas: Array<Record<string, unknown>>;
  totalFilas: number;
  valida: boolean;
}
