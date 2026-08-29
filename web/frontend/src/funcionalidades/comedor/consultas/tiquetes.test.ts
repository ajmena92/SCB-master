import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { comprarTiquetes, consultarSaldoTiquetes } from "./tiquetes";

describe("consultas de tiquetes", () => {
  it("consulta el saldo de la cuenta estudiantil", async () => {
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: { saldo: "4" } } as never);

    await consultarSaldoTiquetes(8);

    expect(api.get).toHaveBeenCalledWith("/v1/comedor/personas/8/cuenta");
  });

  it("registra una compra idempotente como recarga", async () => {
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} } as never);
    vi.stubGlobal("crypto", { randomUUID: () => "compra-8-2026" });

    await comprarTiquetes(8, 3);

    expect(api.post).toHaveBeenCalledWith("/v1/comedor/personas/8/tiquetes", {
      cantidad: 3,
      concepto: "Compra de tiquetes",
      claveIdempotencia: "compra-8-2026",
    });
    vi.unstubAllGlobals();
  });
});
