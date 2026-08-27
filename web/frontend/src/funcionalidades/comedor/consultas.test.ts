import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { registrarConsumo } from "./consultas";

describe("consultas de comedor", () => {
  it("registra consumo con identificador numérico", async () => {
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} } as never);
    await registrarConsumo("8", "2026-08-27");
    expect(api.post).toHaveBeenCalledWith("/v1/comedor/registros", {
      idEstudiante: 8,
      fecha: "2026-08-27",
    });
  });
});
