import { describe, expect, it, vi } from "vitest";
import type { InternalAxiosRequestConfig } from "axios";
import { agregarCsrf } from "./csrf";

describe("protección CSRF", () => {
  it("renueva el token anterior antes de autenticar", async () => {
    Object.defineProperty(document, "cookie", { configurable: true, value: "csrf_token=viejo" });
    const original = globalThis.fetch;
    const fetchNuevo = vi.fn().mockImplementation(async () => {
      Object.defineProperty(document, "cookie", { configurable: true, value: "csrf_token=nuevo" });
      return { ok: true };
    });
    globalThis.fetch = fetchNuevo;
    try {
      const config = {
        method: "post",
        url: "/v1/autenticacion/administracion",
        headers: {},
      } as InternalAxiosRequestConfig;
      await agregarCsrf(config);
      expect(fetchNuevo).toHaveBeenCalledTimes(1);
      expect(config.headers["X-CSRF-Token"]).toBe("nuevo");
    } finally {
      globalThis.fetch = original;
    }
  });
  it("agrega el token a métodos no seguros cuando existe la cookie", async () => {
    Object.defineProperty(document, "cookie", {
      configurable: true,
      value: "csrf_token=token%2Bseguro",
    });
    const config = { method: "post", headers: {} } as {
      method: string;
      headers: Record<string, string>;
    };
    await agregarCsrf(config as unknown as InternalAxiosRequestConfig);
    expect(config.headers["X-CSRF-Token"]).toBe("token+seguro");
  });

  it("no agrega token a métodos seguros", async () => {
    const config = { method: "get", headers: {} } as {
      method: string;
      headers: Record<string, string>;
    };
    await agregarCsrf(config as unknown as InternalAxiosRequestConfig);
    expect(config.headers["X-CSRF-Token"]).toBeUndefined();
  });

  it("solicita un CSRF anónimo antes de un login cuando todavía no existe cookie", async () => {
    Object.defineProperty(document, "cookie", { configurable: true, value: "" });
    const fetchOriginal = globalThis.fetch;
    const fetchSimulado = vi.fn().mockResolvedValue({ ok: true });
    globalThis.fetch = fetchSimulado;
    const config = { method: "post", headers: {} } as {
      method: string;
      headers: Record<string, string>;
    };

    await agregarCsrf(config as unknown as InternalAxiosRequestConfig);

    expect(fetchSimulado).toHaveBeenCalledWith("/api/v1/autenticacion/csrf", {
      credentials: "include",
      headers: { Accept: "application/json" },
    });
    globalThis.fetch = fetchOriginal;
  });
});
