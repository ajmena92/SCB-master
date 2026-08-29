import { describe, expect, it } from "vitest";
import { clasificarErrorOperacion } from "./erroresOperacion";

describe("clasificarErrorOperacion", () => {
  it("conserva el código y mensaje estructurados de la API", () => {
    expect(
      clasificarErrorOperacion({
        response: { data: { detail: { codigo: "tiquete_agotado", mensaje: "Sin saldo" } } },
      }),
    ).toEqual({ codigo: "tiquete_agotado", mensaje: "Sin saldo" });
  });

  it("clasifica errores HTTP sin detalle y errores de conexión", () => {
    expect(clasificarErrorOperacion({ response: { data: {} } }).codigo).toBe("error_operacion");
    expect(clasificarErrorOperacion(new Error("offline")).codigo).toBe("error_conexion");
  });
});
