export type DiaCalendario = {
  fecha: string;
  habilitado: boolean;
  esLectivo: boolean;
  semana: number | null;
  dia: number;
  diaMes: number;
  motivo?: string | null;
  origen: "cerrado" | "no_lectivo" | "sin_menu" | "plantilla" | "sustitucion";
  titulo?: string | null;
  componentes: string[];
  tieneSustitucion: boolean;
};

export type ComponenteSustitucion = { clave: string; nombre: string };
export type SustitucionMenu = {
  fecha: string;
  titulo: string;
  observaciones: string;
  componentes: ComponenteSustitucion[];
};
export const MESES = [
  "Enero",
  "Febrero",
  "Marzo",
  "Abril",
  "Mayo",
  "Junio",
  "Julio",
  "Agosto",
  "Septiembre",
  "Octubre",
  "Noviembre",
  "Diciembre",
];
export const DIAS = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

function fechaIso(anio: number, mes: number, dia: number) {
  return `${anio}-${String(mes).padStart(2, "0")}-${String(dia).padStart(2, "0")}`;
}
export function rangoMes(anio: number, mes: number) {
  return {
    desde: fechaIso(anio, mes, 1),
    hasta: fechaIso(anio, mes, new Date(anio, mes, 0).getDate()),
  };
}
export function fechaCostaRica(): string {
  const partes = new Intl.DateTimeFormat("en-CA", {
    timeZone: "America/Costa_Rica",
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  }).formatToParts(new Date());
  const valor = (tipo: string) => partes.find((parte) => parte.type === tipo)?.value ?? "";
  return `${valor("year")}-${valor("month")}-${valor("day")}`;
}
export function semanasCalendario(dias: DiaCalendario[]): Array<Array<DiaCalendario | null>> {
  if (!dias.length) return [];
  const celdas: Array<DiaCalendario | null> = Array.from({ length: dias[0].dia - 1 }, () => null);
  celdas.push(...dias);
  while (celdas.length % 7) celdas.push(null);
  return Array.from({ length: celdas.length / 7 }, (_, indice) =>
    celdas.slice(indice * 7, indice * 7 + 7),
  );
}
export function fechaVisible(fecha: string): string {
  return new Intl.DateTimeFormat("es-CR", {
    timeZone: "America/Costa_Rica",
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(new Date(`${fecha}T12:00:00`));
}
export function componenteSustitucion(nombre = ""): ComponenteSustitucion {
  return { clave: `${Date.now()}-${Math.random()}`, nombre };
}
