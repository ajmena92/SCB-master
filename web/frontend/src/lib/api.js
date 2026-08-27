import axios from "axios";

// The browser only calls the API through its own origin. Do not use a database
// address or a cross-origin URL here.
const configuredBaseUrl = import.meta.env.VITE_API_BASE_URL || "/api";
if (!configuredBaseUrl.startsWith("/")) {
  throw new Error("VITE_API_BASE_URL debe ser una ruta relativa, por ejemplo /api.");
}
export const API = configuredBaseUrl.replace(/\/$/, "");

const CSRF_HEADER = "X-CSRF-Token";
const CSRF_COOKIE = "csrf_token";
const SAFE_METHODS = new Set(["get", "head", "options"]);

function cookieValue(name) {
  const prefix = `${encodeURIComponent(name)}=`;
  return document.cookie
    .split(";")
    .map((item) => item.trim())
    .find((item) => item.startsWith(prefix))
    ?.slice(prefix.length);
}

let csrfBootstrap;
async function ensureCsrfCookie() {
  if (cookieValue(CSRF_COOKIE)) return;
  if (!csrfBootstrap) {
    csrfBootstrap = Promise.resolve().finally(() => {
      csrfBootstrap = undefined;
    });
  }
  await csrfBootstrap;
}

export const api = axios.create({
  baseURL: API,
  withCredentials: true,
  headers: { Accept: "application/json" },
});

api.interceptors.request.use(async (config) => {
  const method = (config.method || "get").toLowerCase();
  if (!SAFE_METHODS.has(method) && !config.skipCsrf) {
    await ensureCsrfCookie();
    const token = cookieValue(CSRF_COOKIE);
    if (!token) return config;
    config.headers = config.headers || {};
    config.headers[CSRF_HEADER] = decodeURIComponent(token);
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401 && !error.config?.omitirManejoFalloAutenticacion) {
      window.dispatchEvent(new CustomEvent("scsc:unauthenticated"));
    }
    return Promise.reject(error);
  },
);

export function formatApiError(detail) {
  if (detail == null) return "Ocurrió un error. Intente de nuevo.";
  if (typeof detail === "string") return detail;
  if (Array.isArray(detail))
    return detail
      .map((e) => (e && typeof e.msg === "string" ? e.msg : JSON.stringify(e)))
      .join(" ");
  if (detail && typeof detail.msg === "string") return detail.msg;
  return String(detail);
}

export function errMsg(e, { showUnauthorizedDetail = false } = {}) {
  if (!e.response)
    return "No fue posible comunicarse con el servidor. Verifique su conexión e intente de nuevo.";
  if (e.response.status === 401) {
    if (showUnauthorizedDetail) {
      const detail = e.response?.data?.detail;
      return detail == null ? "Credenciales inválidas." : formatApiError(detail);
    }
    return "Su sesión no es válida o ha expirado. Ingrese nuevamente.";
  }
  if (e.response.status === 403) return "No tiene permiso para realizar esta acción.";
  if (e.response.status === 429)
    return "Demasiados intentos. Espere un momento antes de continuar.";
  if (e.response.status >= 500)
    return "El servidor no pudo completar la solicitud. Intente de nuevo más tarde.";
  return formatApiError(e.response?.data?.detail) || "No fue posible completar la solicitud.";
}
