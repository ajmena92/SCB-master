import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { buscarEstudiantes, guardarCorreccion } from "./asistencia";

describe("consultas de asistencia", () => {
  it("busca estudiantes y guarda correcciones en las rutas canónicas", async () => {
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: { items: [] } });
    vi.spyOn(api, "put").mockResolvedValueOnce({ data: {} });
    await buscarEstudiantes("Ana Rojas");
    await guardarCorreccion(7, { estado: "presente", motivo: "Ajuste" });
    expect(api.get).toHaveBeenCalledWith("/v1/estudiantes?pagina=1&tamano=50&buscar=Ana%20Rojas");
    expect(api.put).toHaveBeenCalledWith("/v1/asistencia/marcas/7/correccion", {
      estado: "presente",
      motivo: "Ajuste",
    });
  });
});
