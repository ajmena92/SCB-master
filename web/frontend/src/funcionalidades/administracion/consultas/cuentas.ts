import { api } from "@/compartido/consultas/cliente_http";
import type {
  MovimientoEntrada,
  MovimientoSalida,
  SaldoSalida,
} from "@/compartido/contratos/cuentas";

export async function consultarSaldo(idEstudiante: number): Promise<SaldoSalida> {
  return (await api.get<SaldoSalida>(`/v1/cuentas/${idEstudiante}/saldo`)).data;
}

export async function registrarMovimiento(
  idEstudiante: number,
  datos: MovimientoEntrada,
): Promise<MovimientoSalida> {
  return (await api.post<MovimientoSalida>(`/v1/cuentas/${idEstudiante}/movimientos`, datos)).data;
}
