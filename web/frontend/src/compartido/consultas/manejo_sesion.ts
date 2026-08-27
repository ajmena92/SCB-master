import type { AxiosError } from "axios";

export function manejarSesionExpirada(error: AxiosError): Promise<never> {
  if (error.response?.status === 401 && !error.config?.omitirManejoFalloAutenticacion) {
    window.dispatchEvent(new CustomEvent("scsc:unauthenticated"));
  }
  return Promise.reject(error);
}
