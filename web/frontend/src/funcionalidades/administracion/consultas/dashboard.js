import { api } from "@/compartido/consultas/cliente_http";

export async function consultarDashboard() {
  return (await api.get("/v1/dashboard")).data;
}
