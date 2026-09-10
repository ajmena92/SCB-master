import { api } from "@/compartido/consultas/cliente_http";
import type { DiaCalendario, SustitucionMenu } from "../calendario";

export interface RangoCalendarioMenu {
  desde: string;
  hasta: string;
}

interface EstadoServicioMenu {
  fecha: string;
  habilitado: boolean;
}

interface ComponenteSustitucionApi {
  nombre: string;
  tipo: "Principal";
  orden: number;
}

interface SustitucionMenuApi {
  fecha: string;
  titulo: string;
  observaciones: string | null;
  componentes: ComponenteSustitucionApi[];
}

export async function consultarCalendarioMenu(
  rango: RangoCalendarioMenu,
): Promise<DiaCalendario[]> {
  return (await api.get<DiaCalendario[]>("/v1/menu/calendario", { params: rango })).data;
}

export async function actualizarServicioMenu(estado: EstadoServicioMenu): Promise<void> {
  await api.put("/v1/menu/calendario", estado);
}

export async function guardarSustitucionMenu(datos: SustitucionMenu): Promise<void> {
  const payload: SustitucionMenuApi = {
    fecha: datos.fecha,
    titulo: datos.titulo,
    observaciones: datos.observaciones || null,
    componentes: datos.componentes
      .filter((item) => item.nombre.trim())
      .map((item, indice) => ({
        nombre: item.nombre.trim(),
        tipo: "Principal",
        orden: indice + 1,
      })),
  };
  await api.put(`/v1/menu/sustituciones/${datos.fecha}`, payload);
}
