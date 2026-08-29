import { describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { reservarComedorEstudiante, reservarComedorProfesor } from "./reservas";

describe("reservas de comedor", () => {
  it("reserva para un estudiante mediante su sesión", async () => {
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} } as never);

    await reservarComedorEstudiante("2026-08-27");

    expect(api.post).toHaveBeenCalledWith("/v1/comedor/reservas/estudiante", {
      fecha: "2026-08-27",
    });
  });

  it("reserva para un profesor mediante la sesión administrativa docente", async () => {
    vi.spyOn(api, "post").mockResolvedValueOnce({ data: {} } as never);

    await reservarComedorProfesor("2026-08-27");

    expect(api.post).toHaveBeenCalledWith("/v1/comedor/reservas/profesor", {
      fecha: "2026-08-27",
    });
  });
});
