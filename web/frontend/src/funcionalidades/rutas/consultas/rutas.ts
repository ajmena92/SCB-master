import { api } from "@/lib/api";

export interface Ruta {
  idRuta: number;
  codigo: string;
  descripcion: string;
  activo: boolean;
  estudiantesAsignados: number;
  colorCarnetHex?: string | null;
}

export interface FormularioRuta {
  idRuta: number | null;
  codigo: string;
  descripcion: string;
  activo: boolean;
  colorHex: string;
}

export interface ColorRuta {
  clave: string;
  nombre: string;
  hex: string;
}

export interface DatosRutas {
  rows: Ruta[];
  palette: ColorRuta[];
}

type FilaRuta = Record<string, unknown>;

function valor(fila: FilaRuta, camel: string, pascal: string): unknown {
  return fila[camel] ?? fila[pascal];
}

export function normalizeRuta(fila: FilaRuta): Ruta {
  const activo = valor(fila, "activo", "Activo");
  return {
    idRuta: Number(valor(fila, "idRuta", "IdRuta")),
    codigo: String(valor(fila, "codigo", "Codigo") ?? ""),
    descripcion: String(valor(fila, "descripcion", "Descripcion") ?? ""),
    activo: activo !== false && activo !== 0,
    estudiantesAsignados: Number(valor(fila, "estudiantesAsignados", "EstudiantesAsignados") ?? 0),
    colorCarnetHex: valor(fila, "colorCarnetHex", "ColorCarnetHex") as string | null | undefined,
  };
}

export function validarRuta(formulario: Pick<FormularioRuta, "codigo" | "descripcion">): string {
  if (!formulario.codigo.trim()) return "El código de ruta es obligatorio.";
  if (formulario.codigo.trim() === "0") return "La ruta 0 está protegida.";
  if (formulario.descripcion.trim().length <= 5)
    return "La descripción debe tener más de 5 caracteres.";
  return "";
}

export async function obtenerRutas(): Promise<Ruta[]> {
  const { data } = await api.get<FilaRuta[]>("/v1/transporte/rutas");
  return (data || []).map(normalizeRuta);
}

export async function obtenerPaleta(): Promise<ColorRuta[]> {
  const { data } = await api.get<ColorRuta[]>("/v1/transporte/rutas/paleta");
  return data || [];
}

export async function obtenerDatosRutas(): Promise<DatosRutas> {
  const [rows, palette] = await Promise.all([obtenerRutas(), obtenerPaleta()]);
  return { rows, palette };
}

export interface RutaParaGuardar {
  codigo: string;
  descripcion: string;
  colorHex: string;
  activo: boolean;
}

export async function crearRuta(datos: RutaParaGuardar): Promise<void> {
  await api.post("/v1/transporte/rutas", datos);
}

export async function actualizarRuta(idRuta: number, datos: RutaParaGuardar): Promise<void> {
  await api.put(`/v1/transporte/rutas/${idRuta}`, datos);
}
