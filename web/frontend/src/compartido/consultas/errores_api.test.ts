import { describe, expect, it } from "vitest";
import { errMsg, formatApiError } from "./errores_api";

describe("errores de API", () => {
  it("normaliza detalles simples, listas y objetos", () => {
    expect(formatApiError("detalle")).toBe("detalle");
    expect(formatApiError([{ msg: "campo inválido" }])).toBe("campo inválido");
    expect(formatApiError({ msg: "conflicto" })).toBe("conflicto");
  });

  it("clasifica respuestas HTTP y errores de conexión", () => {
    expect(
      errMsg(
        { response: { status: 401, data: { detail: "no" } } },
        { showUnauthorizedDetail: true },
      ),
    ).toBe("no");
    expect(errMsg({ response: { status: 403 } })).toContain("permiso");
    expect(errMsg({ response: { status: 500 } })).toContain("servidor");
    expect(errMsg(new Error("network"))).toContain("comunicarse");
  });
});
