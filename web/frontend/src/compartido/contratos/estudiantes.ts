/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface AnioEntrada {
  anio: number;
  vigente?: boolean;
}

export interface AsignacionRutaEntrada {
  fechaFin?: string | null;
  fechaInicio: string;
  matriculaId: number;
}

export interface BodyCargarFotoApiV1PersonasPersonaIdFotoPost {
  archivo: string;
}

export interface CambioRutaMatriculaEntrada {
  rutaId?: number | null;
}

export interface GeneracionPinesSeccionEntrada {
  anioLectivoId: number;
  seccion: string;
}

export interface MatriculaBeneficioEntrada {
  becado?: boolean;
}

export interface MatriculaBeneficiosEntrada {
  becado?: boolean;
  rutaId?: number | null;
}

export interface MatriculaEntrada {
  anioLectivoId: number;
  becado?: boolean;
  estado?: string;
  personaId: number;
  seccion: string;
}

export interface PersonaActualizacionEntrada {
  cedula?: string | null;
  nombres: string;
}

export interface PersonaEntrada {
  activo?: boolean;
  cedula?: string | null;
  nombres: string;
  tipo: "estudiante" | "profesor";
}

export interface PersonaSalida {
  activo: boolean;
  cedula: string | null;
  id: number;
  nombres: string;
  pinTemporal?: string | null;
  referenciaPublica: string;
  tipo: string;
}

export interface ResumenPersonasSalida {
  estudiantesActivos: number;
  estudiantesInactivos: number;
}
