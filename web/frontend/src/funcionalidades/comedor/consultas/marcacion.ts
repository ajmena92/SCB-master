import { api } from "@/compartido/consultas/cliente_http";
import type { IngresoEntrada, IngresoSalida } from "@/compartido/contratos/comedor";

export async function registrarMarcacionComedor(datos: IngresoEntrada): Promise<IngresoSalida> {
  return (await api.post<IngresoSalida>("/v1/comedor/operacion/ingresos", datos)).data;
}

export async function consultarConfiguracionComedor() {
  return (await api.get("/v1/comedor/operacion/configuracion")).data;
}

export async function consultarHistorialComedor(fecha: string): Promise<IngresoSalida[]> {
  return (await api.get<IngresoSalida[]>("/v1/comedor/operacion/historial", { params: { fecha, limite: 10 } })).data;
}
