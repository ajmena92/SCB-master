import { api } from "@/compartido/consultas/cliente_http";

export function registrarConsumo(idEstudiante, fecha) {
  return api.post("/v1/comedor/registros", { idEstudiante: Number(idEstudiante), fecha });
}
