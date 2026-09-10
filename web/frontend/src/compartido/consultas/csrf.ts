import type { InternalAxiosRequestConfig } from "axios";
import { API } from "./configuracion_api";

const CSRF_HEADER = "X-CSRF-Token";
const CSRF_COOKIE = "csrf_token";
const SAFE_METHODS = new Set(["get", "head", "options"]);

function valorCookie(nombre: string): string | undefined {
  const prefijo = `${encodeURIComponent(nombre)}=`;
  return document.cookie
    .split(";")
    .map((item) => item.trim())
    .find((item) => item.startsWith(prefijo))
    ?.slice(prefijo.length);
}

let bootstrapCsrf: Promise<void> | undefined;

async function asegurarCookieCsrf(renovar = false): Promise<void> {
  if (!renovar && valorCookie(CSRF_COOKIE)) return;
  bootstrapCsrf ??= fetch(`${API}/v1/autenticacion/csrf`, {
    credentials: "include",
    headers: { Accept: "application/json" },
  })
    .then((respuesta) => {
      if (!respuesta.ok) throw new Error("No se pudo iniciar la protección CSRF.");
    })
    .finally(() => {
      bootstrapCsrf = undefined;
    });
  await bootstrapCsrf;
}

export async function agregarCsrf(
  config: InternalAxiosRequestConfig,
): Promise<InternalAxiosRequestConfig> {
  const metodo = (config.method || "get").toLowerCase();
  if (!SAFE_METHODS.has(metodo) && !config.omitirCsrf) {
    const esLogin = /\/v1\/autenticacion\/(administracion|portal)$/.test(config.url || "");
    await asegurarCookieCsrf(esLogin);
    const token = valorCookie(CSRF_COOKIE);
    if (token) config.headers[CSRF_HEADER] = decodeURIComponent(token);
  }
  return config;
}
