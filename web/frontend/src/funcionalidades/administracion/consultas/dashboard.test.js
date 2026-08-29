import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { consultarDashboard } from "./dashboard";

describe("consulta del dashboard", () => {
  it("envía la vista separada de profesores al backend", async () => {
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: {} });

    await consultarDashboard("2026-08-27", { tipoPersona: "profesor", pagina: 1 });

    expect(api.get).toHaveBeenCalledWith("/v1/reportes/dashboard", {
      params: { fecha: "2026-08-27", porPagina: 25, tipoPersona: "profesor", pagina: 1 },
    });
  });

  it("envía el horario seleccionado sin alterar la consulta base", async () => {
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: {} });

    await consultarDashboard("2026-08-27", { horario: "nocturno", pagina: 1 });

    expect(api.get).toHaveBeenCalledWith("/v1/reportes/dashboard", {
      params: { fecha: "2026-08-27", porPagina: 25, horario: "nocturno", pagina: 1 },
    });
  });
});
