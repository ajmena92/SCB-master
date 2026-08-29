import type { EstudianteSalida } from "@/compartido/contratos/estudiantes";
import type { RutaSalida } from "@/compartido/contratos/transporte";

export type EstudianteAdministrativo = EstudianteSalida;
export type RutaEstudiante = RutaSalida;

export type SeccionEstudiante = {
  seccion?: string | null;
  etiqueta: string;
  total: number;
};

export type EstudianteReportePines = {
  idEstudiante: number;
  nombreCompleto: string;
  cedula: string;
  horario: string;
  pin: string;
};

export type ReportePinesEstudiantes = {
  turno: string | null;
  seccion: string;
  total: number;
  generadoEn: string;
  estudiantes: EstudianteReportePines[];
};

export type DatosPerfilEstudiante = {
  idEstadoComedor: 1 | 2;
  beneficioComedor: string;
  idRuta: number | null;
  rutaCodigo: string | null;
  rutaDescripcion: string | null;
  rutaColor: string | null;
};

export type PerfilEstudiante = {
  estudiante: DatosPerfilEstudiante;
  tieneFoto: boolean;
};
