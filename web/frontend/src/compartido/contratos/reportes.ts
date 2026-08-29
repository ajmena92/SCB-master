/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface AlertaDashboard {
  cantidad: number;
  tipo: string;
  titulo: string;
}

export interface DashboardSalida {
  alertas?: Array<AlertaDashboard>;
  asistencia: MetricaAsistencia;
  becadosComedor: number;
  consumoComedor: number;
  fecha: string;
  horarios?: Array<"diurno" | "nocturno">;
  ingresosHistoricos?: number;
  noBecados: number;
  nominal: NominalPaginado;
  porEstadoComedor: Array<GrupoDashboard>;
  porHorario: Array<GrupoDashboard>;
  porRuta: Array<RutaDashboard>;
  porSeccion: Array<GrupoDashboard>;
  saldoTiquetes?: number;
  semana: Array<TendenciaDia>;
  tipoPersona: "estudiante" | "profesor";
  tiquetesConsumidos?: number;
  tiquetesReservados?: number;
  ultimosCincoDias: Array<TendenciaDia>;
}

export interface GrupoDashboard {
  ausentes?: number;
  consumo?: number;
  nombre: string;
  porcentaje?: number;
  presentes?: number;
  sinRegistro?: number;
  total: number;
}

export interface MetricaAsistencia {
  ausentes: number;
  coberturaRegistro: number;
  justificadas: number;
  porcentaje: number;
  presentes: number;
  sinRegistro: number;
  tardanzas: number;
  total: number;
}

export interface NominalPaginado {
  elementos: Array<RegistroNominal>;
  pagina: number;
  porPagina: number;
  total: number;
}

export interface RegistroNominal {
  beneficioComedor: string;
  cedula: string | null;
  estado: string;
  historico?: boolean;
  horario: string;
  idEstadoComedor: 1 | 2;
  idEstudiante?: number | null;
  idPersona: number;
  nombreCompleto: string;
  origen: string;
  ruta?: string;
  seccion: string;
  tipoPersona: string;
}

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

export interface RutaDashboard {
  ausentes?: number;
  consumo?: number;
  idRuta?: number | null;
  nombre: string;
  porcentaje?: number;
  presentes?: number;
  sinRegistro?: number;
  total: number;
}

export interface TendenciaDia {
  ausentes: number;
  dia: string;
  fecha: string;
  porcentaje: number;
  presentes: number;
  sinRegistro: number;
  total: number;
}
