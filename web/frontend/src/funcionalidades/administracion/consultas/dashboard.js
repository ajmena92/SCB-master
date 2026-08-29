import { api } from "@/compartido/consultas/cliente_http";

export async function consultarDashboard(fecha, filtros = {}) {
  return (
    await api.get("/v1/reportes/dashboard", {
      params: { fecha, porPagina: 25, ...filtros },
    })
  ).data;
}
