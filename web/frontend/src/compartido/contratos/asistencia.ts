/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface CorreccionEntrada {
  estado: "presente" | "ausente" | "tardanza" | "justificada";
  motivo: string;
}

export interface MarcaEntrada {
  estado: "presente" | "ausente" | "tardanza" | "justificada";
  fecha: string;
  idEstudiante: number;
  observacion?: string | null;
}

export interface MarcaSalida {
  corregida?: boolean;
  estado: "presente" | "ausente" | "tardanza" | "justificada";
  fecha: string;
  idEstudiante: number;
  idMarca: number;
  observacion?: string | null;
}
