import { api } from "@/compartido/consultas/cliente_http";

export async function consultarEventosAuditoria() {
  return (await api.get("/v1/auditoria/eventos")).data;
}
