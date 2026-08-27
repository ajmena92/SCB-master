import type { InternalAxiosRequestConfig } from "axios";

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

async function asegurarCookieCsrf(): Promise<void> {
  if (valorCookie(CSRF_COOKIE)) return;
  bootstrapCsrf ??= Promise.resolve().finally(() => {
    bootstrapCsrf = undefined;
  });
  await bootstrapCsrf;
}

export async function agregarCsrf(
  config: InternalAxiosRequestConfig,
): Promise<InternalAxiosRequestConfig> {
  const metodo = (config.method || "get").toLowerCase();
  if (!SAFE_METHODS.has(metodo) && !config.skipCsrf) {
    await asegurarCookieCsrf();
    const token = valorCookie(CSRF_COOKIE);
    if (token) config.headers[CSRF_HEADER] = decodeURIComponent(token);
  }
  return config;
}
