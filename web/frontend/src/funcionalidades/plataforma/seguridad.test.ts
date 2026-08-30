import { describe, expect, it } from "vitest";
import { esAdministrador } from "./seguridad";

describe("esAdministrador", () => {
  it("reconoce el rol administrativo sin depender de mayúsculas", () => {
    expect(esAdministrador({ tipo: "admin", usuario: { Rol: "Administrador" } })).toBe(true);
    expect(esAdministrador({ tipo: "admin", roles: ["administrador"] })).toBe(true);
  });

  it("mantiene al operador fuera de la configuración", () => {
    expect(esAdministrador({ tipo: "admin", usuario: { rol: "operador" } })).toBe(false);
    expect(esAdministrador(false)).toBe(false);
  });
});
