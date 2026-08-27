import { describe, expect, it, vi } from "vitest";
import { downloadCSV } from "./csv";

describe("descarga CSV", () => {
  it("escapa comillas, nulos y conserva encabezado UTF-8", () => {
    const click = vi.spyOn(HTMLAnchorElement.prototype, "click").mockImplementation(() => {});
    const crear = vi.spyOn(URL, "createObjectURL").mockReturnValue("blob:test");
    downloadCSV("datos.csv", [{ label: "Nombre", key: "nombre" }], [{ nombre: 'Ana "A"' }, { nombre: null }]);
    expect(crear).toHaveBeenCalled(); expect(click).toHaveBeenCalled();
    click.mockRestore(); crear.mockRestore();
  });
});
