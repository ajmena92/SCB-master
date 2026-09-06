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
  const rgb = (color.slice(1).match(/../g) || []).map((parte) => parseInt(parte, 16) / 255);
  const luminancia = rgb
    .map((componente) =>
      componente <= 0.03928 ? componente / 12.92 : ((componente + 0.055) / 1.055) ** 2.4,
    )
    .reduce(
      (total, componente, indice) => total + componente * [0.2126, 0.7152, 0.0722][indice],
      0,
    );
  const contraste = (texto: number) =>
    (Math.max(luminancia, texto) + 0.05) / (Math.min(luminancia, texto) + 0.05);
  const luminanciaTextoOscuro = 0.009;
  const luminanciaTextoClaro = 1;

  return contraste(luminanciaTextoOscuro) >= contraste(luminanciaTextoClaro)
    ? "#111827"
    : "#FFFFFF";
}

export function obtenerNombreCompleto(estudiante: DatosCarnet = {}): string {
  return [estudiante.nombre, estudiante.primerApellido, estudiante.segundoApellido]
    .filter(Boolean)
    .join(" ");
}

export function obtenerAnioCarnet(estudiante: DatosCarnet = {}): number {
  return estudiante.anio ?? estudiante.anioLectivo ?? estudiante.ano ?? new Date().getFullYear();
}
