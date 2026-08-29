import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { consultarSustituciones, guardarSustitucion } from "./menu";

describe("consultas de menú", () => {
  it("consulta y guarda sustituciones por sus rutas canónicas", async () => {
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: [] });
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} });
    await consultarSustituciones();
    const datos = {
      fecha: "2026-01-01",
      titulo: "Menú especial",
      observaciones: "",
      componentes: [],
    };
    await guardarSustitucion(datos);
    expect(api.get).toHaveBeenCalledWith("/v1/menu/sustituciones");
    expect(api.post).toHaveBeenCalledWith("/v1/menu/sustitucion", datos);
  });
});
