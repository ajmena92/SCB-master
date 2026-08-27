import { api } from "@/compartido/consultas/cliente_http";
import type { LoteSalida, Previsualizacion } from "@/compartido/contratos/importaciones";

function formulario(archivo: File) {
  const datos = new FormData();
  datos.append("archivo", archivo);
  return datos;
}

export async function previsualizarImportacion(archivo: File): Promise<Previsualizacion> {
  return (
    await api.post<Previsualizacion>("/v1/importaciones/previsualizaciones", formulario(archivo))
  ).data;
}

export async function ejecutarImportacion(archivo: File): Promise<LoteSalida> {
  return (await api.post<LoteSalida>("/v1/importaciones/lotes", formulario(archivo))).data;
}
