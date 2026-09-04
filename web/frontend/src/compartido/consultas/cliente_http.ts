import axios from "axios";
import { agregarCsrf } from "./csrf";
import { manejarSesionExpirada } from "./manejo_sesion";
import { API } from "./configuracion_api";

export { API } from "./configuracion_api";

declare module "axios" {
  interface AxiosRequestConfig {
    omitirCsrf?: boolean;
    omitirManejoFalloAutenticacion?: boolean;
  }
}

export const api = axios.create({
  baseURL: API,
  withCredentials: true,
  headers: { Accept: "application/json" },
});
api.interceptors.request.use((configuracion) => {
  return agregarCsrf(configuracion);
});
api.interceptors.response.use((response) => response, manejarSesionExpirada);
