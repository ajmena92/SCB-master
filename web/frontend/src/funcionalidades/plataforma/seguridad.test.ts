import { describe, expect, it } from "vitest";
import { esAdministrador } from "./seguridad";

describe("esAdministrador", () => {
  it("reconoce el rol administrativo sin depender de mayúsculas", () => {
    expect(esAdministrador({ tipo: "administracion", rol: "administrador" })).toBe(true);
    expect(esAdministrador({ tipo: "administracion", rol: "administrador" })).toBe(true);
  });

  it("mantiene al operador fuera de la configuración", () => {
    expect(esAdministrador({ tipo: "administracion", rol: "operador" })).toBe(false);
    expect(esAdministrador(false)).toBe(false);
  });
});
