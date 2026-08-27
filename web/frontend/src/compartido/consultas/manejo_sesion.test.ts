import { describe, expect, it, vi } from "vitest";
import { manejarSesionExpirada } from "./manejo_sesion";

describe("manejo de sesión expirada", () => {
  it("notifica una respuesta 401 sin contrato de omisión", async () => {
    const escuchar = vi.fn();
    window.addEventListener("scsc:unauthenticated", escuchar);
    await expect(
      manejarSesionExpirada({ response: { status: 401 }, config: {} } as never),
    ).rejects.toBeTruthy();
    expect(escuchar).toHaveBeenCalledOnce();
    window.removeEventListener("scsc:unauthenticated", escuchar);
  });

  it("no notifica una petición marcada para omitir el manejo", async () => {
    const escuchar = vi.fn();
    window.addEventListener("scsc:unauthenticated", escuchar);
    await expect(
      manejarSesionExpirada({
        response: { status: 401 },
        config: { omitirManejoFalloAutenticacion: true },
      } as never),
    ).rejects.toBeTruthy();
    expect(escuchar).not.toHaveBeenCalled();
    window.removeEventListener("scsc:unauthenticated", escuchar);
  });
});
