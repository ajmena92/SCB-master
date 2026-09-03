export const NOMBRE_COLEGIO = "Colegio Técnico Profesional de Platanares";
export const LOGO_COLEGIO = "/images/escudo-ctp-platanares.png";

export type DatosCarnet = {
  tipoPersona?: "estudiante" | "profesor";
  idEstudiante?: number;
  nombre?: string;
  primerApellido?: string;
  segundoApellido?: string;
  cedula?: string;
  seccion?: string;
  rutaColor?: string;
  rutaDescripcion?: string;
  idEstadoComedor?: 1 | 2;
  beneficioComedor?: string;
  colegio?: string;
  codigoQr?: string;
  tieneFoto?: boolean;
  anio?: number;
  anioLectivo?: number;
  ano?: number;
};
export function obtenerColorRutaSeguro(valor?: unknown): string {
  return typeof valor === "string" && /^#[0-9a-f]{6}$/i.test(valor) ? valor : "#CBD5E1";
}

export function obtenerColorTextoRuta(color: string): string {
  const rgb = (color.slice(1).match(/../g) || []).map((parte) => parseInt(parte, 16));
  return (rgb[0] * 299 + rgb[1] * 587 + rgb[2] * 114) / 1000 > 160 ? "#252653" : "#FFFFFF";
}

export function obtenerNombreCompleto(estudiante: DatosCarnet = {}): string {
  return [estudiante.nombre, estudiante.primerApellido, estudiante.segundoApellido]
    .filter(Boolean)
    .join(" ");
}

export function obtenerAnioCarnet(estudiante: DatosCarnet = {}): number {
  return estudiante.anio ?? estudiante.anioLectivo ?? estudiante.ano ?? new Date().getFullYear();
}
