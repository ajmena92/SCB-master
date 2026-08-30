import type { AxiosError } from "axios";
import { borrarTokenSesion } from "./token_sesion";

export function manejarSesionExpirada(error: AxiosError): Promise<never> {
  if (error.response?.status === 401 && !error.config?.omitirManejoFalloAutenticacion) {
    borrarTokenSesion();
    window.dispatchEvent(new CustomEvent("scsc:unauthenticated"));
  }
  return Promise.reject(error);
}
