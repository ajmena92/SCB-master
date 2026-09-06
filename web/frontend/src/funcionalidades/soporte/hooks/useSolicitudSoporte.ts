import { useState } from "react";
import { mensajeError } from "@/compartido/consultas/errores";
import { enviarSolicitud } from "@/funcionalidades/soporte/consultas";
import { notificar } from "@/compartido/notificaciones/notificaciones";

export function useSolicitudSoporte() {
  const [asunto, setAsunto] = useState("");
  const [detalle, setDetalle] = useState("");
  const [enviando, setEnviando] = useState(false);
  async function enviar() {
    setEnviando(true);
    try {
      await enviarSolicitud(asunto, detalle);
      notificar.exito("Solicitud enviada");
      setAsunto("");
      setDetalle("");
    } catch (error) {
      notificar.error(mensajeError(error));
    } finally {
      setEnviando(false);
    }
  }
  return { asunto, detalle, enviando, setAsunto, setDetalle, enviar };
}
