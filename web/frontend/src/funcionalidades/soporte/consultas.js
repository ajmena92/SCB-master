import { api } from "@/compartido/consultas/cliente_http";

export function enviarSolicitud(asunto, detalle) {
  return api.post("/v1/soporte/solicitudes", { asunto, detalle });
}
