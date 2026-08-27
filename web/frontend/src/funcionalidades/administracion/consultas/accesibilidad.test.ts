import { describe, expect, it } from "vitest";
import { DOMINIOS } from "./dominios";

/** Auditoría automatizable de contratos mínimos WCAG AA para las vistas administrativas. */
describe("contratos de accesibilidad administrativa", () => {
  it("define título, descripción y permiso para cada vista", () => {
    for (const dominio of Object.values(DOMINIOS)) {
      expect(dominio.titulo.trim()).not.toBe("");
      expect(dominio.descripcion.trim()).not.toBe("");
      expect(dominio.permiso).toMatch(/^[a-z]+\.[a-z]+$/);
      expect(dominio.columnas.every((columna) => columna.trim().length > 0)).toBe(true);
    }
  });
});
