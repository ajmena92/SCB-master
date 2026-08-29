/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface AsignacionEntrada {
  idBeneficio?: number | null;
}

export interface AsignacionSalida {
  idBeneficio: number | null;
  idEstudiante: number;
}

export interface BodyCargarApiV1EstudiantesIdEstudianteFotoPost {
  archivo: string;
}

export interface CambioAsignacion {
  idBeneficio?: number | null;
  idRuta?: number | null;
}

export interface CambioEstadoComedor {
  idEstadoComedor: 1 | 2;
}

export interface EstudianteEntrada {
  activo?: boolean;
  carne: string;
  cedula?: string | null;
  nombre: string;
  primerApellido: string;
  seccion?: string | null;
  segundoApellido?: string | null;
}

export interface EstudianteSalida {
  activo: boolean;
  beneficioComedor?: string;
  bloqueado?: boolean;
  carne: string;
  cedula: string | null;
  debeCambiarPin?: boolean;
  idBeneficio?: number | null;
  idEstadoComedor?: 1 | 2;
  idEstudiante: number;
  idRuta?: number | null;
  nombre: string;
  primerApellido: string;
  rutaCodigo?: string | null;
  rutaDescripcion?: string | null;
  seccion: string | null;
  segundoApellido: string | null;
  tieneFoto?: boolean;
  turno?: string | null;
}

export interface GeneracionPinesSeccion {
  seccion?: string | null;
  turno?: string | null;
}

export interface PaginaEstudiantes {
  elementos: Array<EstudianteSalida>;
  pagina: number;
  tamano: number;
  total: number;
}

export interface PinGenerado {
  idEstudiante: number;
  pin: string;
}
