/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface ConfirmacionImportacion {
  anio: number;
  filas: Array<FilaImportacion>;
  huella: string;
}

export interface FilaImportacion {
  cedula?: string | null;
  nombres: string;
  seccion?: string | null;
  tipo: "estudiante" | "profesor";
}
