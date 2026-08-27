/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface HTTPValidationError {
  detail?: Array<ValidationError>;
}

export interface ValidationError {
  ctx?: Record<string, unknown>;
  input?: unknown;
  loc: Array<string | number>;
  msg: string;
  type: string;
}
