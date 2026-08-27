import { describe, expect, it } from "vitest";
import type { InternalAxiosRequestConfig } from "axios";
import { agregarCsrf } from "./csrf";

describe("protección CSRF", () => {
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
});
