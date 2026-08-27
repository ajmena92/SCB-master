import type { BeneficioSalida, EstudianteSalida, RutaSalida } from "@/compartido/contratos/api";

export type EstudianteAdministrativo = EstudianteSalida;
export type BeneficioEstudiante = BeneficioSalida;
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
  tipoBeca: number | null;
  idRuta: number | null;
  rutaCodigo: string | null;
  rutaDescripcion: string | null;
  rutaColor: string | null;
};

export type PerfilEstudiante = {
  estudiante: DatosPerfilEstudiante;
  tieneFoto: boolean;
};
