import { api } from "@/compartido/consultas/cliente_http";

export async function consultarSustituciones() {
  return (await api.get("/v1/menu/sustituciones")).data;
}

export async function guardarSustitucion(datos) {
  return api.post("/v1/menu/sustitucion", datos);
}
