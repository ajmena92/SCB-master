import { api } from "@/compartido/consultas/cliente_http";
import type { CuentaTiquetesSalida, TiquetesEntrada } from "@/compartido/contratos/comedor";

/** La API actual representa el saldo mediante cuentas de personas habilitadas. */
export async function consultarSaldoTiquetes(idPersona: number): Promise<CuentaTiquetesSalida> {
  return (await api.get<CuentaTiquetesSalida>(`/v1/comedor/personas/${idPersona}/cuenta`)).data;
}

export async function comprarTiquetes(
  idPersona: number,
  cantidad: number,
  concepto = "Compra de tiquetes",
): Promise<Record<string, unknown>> {
  const datos: TiquetesEntrada = {
    cantidad,
    concepto,
    claveIdempotencia: crypto.randomUUID(),
  };
  return (
    await api.post<Record<string, unknown>>(`/v1/comedor/personas/${idPersona}/tiquetes`, datos)
  ).data;
}
