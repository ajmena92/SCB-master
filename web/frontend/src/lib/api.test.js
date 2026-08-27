import { errMsg } from "./api";

describe("errMsg", () => {
  const unauthorized = { response: { status: 401, data: { detail: "Credenciales inválidas." } } };

  it("preserves the safe API detail for an authentication attempt", () => {
    expect(errMsg(unauthorized, { showUnauthorizedDetail: true })).toBe("Credenciales inválidas.");
  });

  it("keeps the expired-session message for protected requests", () => {
    expect(errMsg(unauthorized)).toBe("Su sesión no es válida o ha expirado. Ingrese nuevamente.");
  });

  it.each([
    [403, "No tiene permiso"],
    [429, "Demasiados intentos"],
    [500, "servidor"],
  ])("traduce errores HTTP %s", (status, esperado) => {
    expect(errMsg({ response: { status, data: { detail: "detalle" } } })).toContain(esperado);
  });

  it("traduce detalles de validación y errores sin respuesta", () => {
    expect(
      errMsg({ response: { status: 422, data: { detail: [{ msg: "campo inválido" }] } } }),
    ).toContain("campo inválido");
    expect(errMsg(new Error("network"))).toContain("comunicarse");
  });
});
