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
/** Días operativos del comedor; sábado y domingo no tienen menú lectivo. */
export const DIAS = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes"];

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
  const semanas = new Map<string, Array<DiaCalendario | null>>();
  for (const dia of dias) {
    if (dia.dia > 5) continue;
    const fecha = new Date(`${dia.fecha}T12:00:00`);
    fecha.setDate(fecha.getDate() - (dia.dia - 1));
    const clave = fecha.toISOString().slice(0, 10);
    const semana = semanas.get(clave) ?? Array.from({ length: 5 }, () => null);
    semana[dia.dia - 1] = dia;
    semanas.set(clave, semana);
  }
  return [...semanas.entries()].sort(([a], [b]) => a.localeCompare(b)).map(([, semana]) => semana);
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
