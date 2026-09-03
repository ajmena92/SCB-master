export type RolAdministrativo = "administrador" | "operador";
export type TipoPersona = "estudiante" | "profesor";

export interface Persona {
  id: number;
  referenciaPublica: string;
  cedula: string | null;
  nombres: string;
  apellidos?: string;
  tipo: TipoPersona;
  activo: boolean;
  matriculaId?: number | null;
  seccion?: string | null;
  becado?: boolean;
  beneficioComedor?: "Beneficiario" | "No beneficiario";
  estadoMatricula?: "activo" | "retirado" | "graduado" | "trasladado" | null;
  rutaId?: number | null;
  descripcionRuta?: string | null;
  beneficioTransporte?: string;
  saldoTiquetes?: number;
}

export interface CredencialTemporal {
  cedula: string;
  nombre: string;
  pinTemporal: string;
}

export interface PersonaCreada extends Persona {
  pinTemporal: string;
}

export interface ResumenPersonas {
  estudiantesActivos: number;
  estudiantesInactivos: number;
}

export interface AnioLectivo {
  id: number;
  anio: number;
  vigente: boolean;
  cerrado: boolean;
}

export interface Matricula {
  id: number;
  personaId: number;
  anioLectivoId: number;
  seccion: string;
  becaComedor: boolean;
  estado: "activo" | "retirado" | "graduado" | "trasladado";
}

export interface PlantillaMenu {
  id: number;
  nombre: string;
  componentes: string[];
  activa: boolean;
}

export interface PublicacionMenu {
  id: number;
  fecha: string;
  plantillaId: number;
  nombre: string;
  componentes: string[];
}

export interface Tarifa {
  id: number;
  tipoPersona: TipoPersona;
  montoColones: number;
  vigenteDesde: string;
  vigenteHasta?: string | null;
}

export interface ResumenImportacion {
  token: string;
  filas: number;
  altas: number;
  cambios: number;
  desactivaciones: number;
  errores: number;
  detalle: Array<{ fila: number; estado: string; mensaje: string }>;
}

export interface ResultadoConfirmacionImportacion {
  credenciales: CredencialTemporal[];
  [campo: string]: unknown;
}

export interface ResultadoOperacion {
  estado: "aceptada" | "rechazada" | "pendiente";
  mensaje: string;
  persona?: Persona;
  saldo?: number;
  resultado?: string;
  modalidad?: string;
  consumioTiquete?: boolean;
  marcaTransporteExistente?: boolean;
  advertencia?: string | null;
}

export interface ReporteFila {
  [columna: string]: string | number | boolean | null;
}

export interface Pagina<T> {
  elementos: T[];
  total: number;
}
