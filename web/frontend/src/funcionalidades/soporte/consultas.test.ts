import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { enviarSolicitud } from "./consultas";

describe("consultas de soporte", () => {
  it("envía solicitudes con asunto y detalle", async () => {
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} } as never);
    await enviarSolicitud("Acceso", "No puedo ingresar");
    expect(api.post).toHaveBeenCalledWith("/v1/soporte/solicitudes", {
      asunto: "Acceso",
      detalle: "No puedo ingresar",
    });
  });
});
