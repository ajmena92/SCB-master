import { createRoot } from "react-dom/client";
import { act } from "react";
import { describe, expect, it, vi } from "vitest";
import { EditorRuta } from "./EditorRuta";

describe("editor de rutas", () => {
  it("renderiza formulario, paleta, error y estado bloqueado", async () => {
    const contenedor = document.createElement("div");
    const root = createRoot(contenedor);
    const form = {
      idRuta: 2,
      codigo: "0",
      descripcion: "Ruta protegida",
      activo: true,
      colorHex: "#FF0000",
    };
    await act(async () =>
      root.render(
        <EditorRuta
          open
          onOpenChange={vi.fn()}
          form={form}
          setForm={vi.fn()}
          palette={[{ clave: "rojo", nombre: "Rojo", hex: "#FF0000" }]}
          saving={false}
          error="Error de prueba"
          onGuardar={vi.fn()}
        />,
      ),
    );
    expect(document.body.textContent).toContain("Editar ruta");
    expect(document.body.textContent).toContain("Error de prueba");
    expect(document.body.querySelector('[data-testid="ruta-paleta-rojo"]')).not.toBeNull();
    expect(
      document.body.querySelector('[data-testid="ruta-guardar"]')?.hasAttribute("disabled"),
    ).toBe(true);
    await act(async () => root.unmount());
  });
});
