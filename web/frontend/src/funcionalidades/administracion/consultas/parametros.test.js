import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import {
  consultarCalendario,
  consultarParametros,
  guardarParametros,
  normalizeParametros,
  validateParametros,
} from "./parametros";

describe("consultas y reglas de parámetros", () => {
  it("normaliza formatos del contrato y valida horarios", () => {
    const datos = normalizeParametros({
      MinutosAviso: 15,
      Horarios: [
        { IdHorario: 1, Descripcion: "Almuerzo", HoraInicio: "11:00", HoraLimite: "12:00" },
      ],
    });
    expect(datos.horarios[0].descripcion).toBe("Almuerzo");
    expect(validateParametros(datos)).toBe("");
  });

  it("usa las rutas canónicas de consulta y guardado", async () => {
    vi.spyOn(api, "get")
      .mockResolvedValueOnce({ data: { dias: [] } })
      .mockResolvedValueOnce({ data: {} });
    vi.spyOn(api, "put").mockResolvedValueOnce({ data: {} });
    await consultarCalendario(2026, 8);
    await consultarParametros();
    await guardarParametros({ horarios: [] });
    expect(api.get).toHaveBeenCalledWith("/v1/parametros/calendario?anio=2026&mes=8");
    expect(api.put).toHaveBeenCalledWith("/v1/parametros", { horarios: [] });
  });
});
