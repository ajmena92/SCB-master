/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface ReporteEstudiante {
  activo: boolean;
  carne: string;
  idEstudiante: number;
  nombreCompleto: string;
  seccion: string | null;
}

export interface ReporteEstudiantes {
  elementos: Array<ReporteEstudiante>;
  total: number;
}

export interface ReporteRuta {
  activo: boolean;
  codigo: string;
  descripcion: string;
  estudiantesAsignados: number;
  idRuta: number;
}

export interface ReporteTransporte {
  elementos: Array<ReporteRuta>;
  total: number;
}
