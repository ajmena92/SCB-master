import { api } from "@/compartido/consultas/cliente_http";
import type { ReservaEntrada, ReservaSalida } from "@/compartido/contratos/comedor";

export async function reservarComedorEstudiante(fecha: string): Promise<ReservaSalida> {
  const datos: ReservaEntrada = { fecha };
  return (await api.post<ReservaSalida>("/v1/comedor/reservas", datos)).data;
}

export async function reservarComedorProfesor(fecha: string): Promise<ReservaSalida> {
  const datos: ReservaEntrada = { fecha };
  return (await api.post<ReservaSalida>("/v1/comedor/reservas", datos)).data;
}

export async function cancelarComedorEstudiante(fecha: string): Promise<ReservaSalida> {
  return (
    await api.delete<ReservaSalida>("/v1/comedor/reservas", {
      data: { fecha },
    })
  ).data;
}
