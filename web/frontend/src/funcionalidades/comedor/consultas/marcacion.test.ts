import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { registrarMarcacionComedor } from "./marcacion";

describe("marcación de comedor", () => {
  it("registra el ingreso usando el código de barras", async () => {
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} } as never);

    await registrarMarcacionComedor({ cedula: "E-8", fecha: "2026-08-27" });

    expect(api.post).toHaveBeenCalledWith("/v1/comedor/operacion/ingresos", {
      cedula: "E-8",
      fecha: "2026-08-27",
    });
  });
});
