import axios from "axios";
import { agregarCsrf } from "./csrf";
import { manejarSesionExpirada } from "./manejo_sesion";

const urlConfigurada = import.meta.env.VITE_API_BASE_URL || "/api";
if (!urlConfigurada.startsWith("/"))
  throw new Error("VITE_API_BASE_URL debe ser una ruta relativa, por ejemplo /api.");
export const API = urlConfigurada.replace(/\/$/, "");

declare module "axios" {
  interface AxiosRequestConfig {
    skipCsrf?: boolean;
    omitirManejoFalloAutenticacion?: boolean;
  }
}

export const api = axios.create({
  baseURL: API,
  withCredentials: true,
  headers: { Accept: "application/json" },
});
api.interceptors.request.use(agregarCsrf);
api.interceptors.response.use((response) => response, manejarSesionExpirada);
