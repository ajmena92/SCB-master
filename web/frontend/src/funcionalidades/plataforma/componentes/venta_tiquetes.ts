import type { Persona } from "@/compartido/contratos/plataforma";

export type PersonaVenta = Persona & { becado: boolean; saldoTiquetes: number };

export const monedaColones = new Intl.NumberFormat("es-CR", {
  style: "currency",
  currency: "CRC",
  maximumFractionDigits: 0,
});
