import { describe, expect, it, vi } from "vitest";
import { api } from "@/lib/api";
import { actualizarRuta, crearRuta, normalizeRuta, obtenerDatosRutas, validarRuta } from "./rutas";

describe("consultas de rutas", () => {
  it("normaliza, valida y ejecuta operaciones canónicas", async () => {
    expect(normalizeRuta({ IdRuta: 2, Codigo: "N", Descripcion: "Norte", Activo: 1, EstudiantesAsignados: 3 }).idRuta).toBe(2);
    expect(validarRuta({ codigo: "", descripcion: "x" })).toContain("obligatorio"); expect(validarRuta({ codigo: "0", descripcion: "Norte" })).toContain("protegida"); expect(validarRuta({ codigo: "A", descripcion: "corta" })).toContain("más de 5"); expect(validarRuta({ codigo: "A", descripcion: "Ruta Norte" })).toBe("");
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: [] } as never).mockResolvedValueOnce({ data: [] } as never); vi.spyOn(api, "post").mockResolvedValue({} as never); vi.spyOn(api, "put").mockResolvedValue({} as never);
    await obtenerDatosRutas(); await crearRuta({ codigo: "A", descripcion: "Ruta Norte", colorHex: "#fff", activo: true }); await actualizarRuta(2, { codigo: "A", descripcion: "Ruta Norte", colorHex: "#fff", activo: true });
    expect(api.post).toHaveBeenCalledWith("/v1/transporte/rutas", expect.anything()); expect(api.put).toHaveBeenCalledWith("/v1/transporte/rutas/2", expect.anything());
  });
});
