import { describe, expect, it } from "vitest";
import { destinoSesion } from "./destinoSesion";

describe("destinoSesion", () => {
  it("resuelve destinos administrativos y estudiantiles", () => {
    expect(destinoSesion({ tipo: "administracion" })).toBe("/admin/panel");
    expect(destinoSesion({ tipo: "administracion", vinculacionPendiente: true })).toBe("/admin/vinculacion-inicial");
    expect(destinoSesion({ tipo: "administracion", cambioContrasenaObligatorio: true })).toBe("/admin/cambiar-contrasena");
    expect(destinoSesion({ tipo: "estudiante" })).toBe("/portal");
    expect(destinoSesion({ tipo: "profesor" }, true)).toBe("/cambiar-pin");
    expect(destinoSesion(null)).toBeNull();
  });
});
