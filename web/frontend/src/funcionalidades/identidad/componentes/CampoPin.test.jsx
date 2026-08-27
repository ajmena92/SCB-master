import { act } from "react";
import { createRoot } from "react-dom/client";
import { describe, expect, it } from "vitest";
import CampoPin from "./CampoPin";

describe("CampoPin", () => {
  it("limita la entrada a dígitos y permite pegar un PIN completo", async () => {
    const contenedor = document.createElement("div");
    const raiz = createRoot(contenedor);
    document.body.appendChild(contenedor);
    let valor = "";
    await act(async () =>
      raiz.render(
        <CampoPin label="PIN" value={valor} onChange={(nuevo) => (valor = nuevo)} testid="pin" />,
      ),
    );
    const entrada = contenedor.querySelector('[data-testid="pin"] input');
    const evento = new Event("paste", { bubbles: true, cancelable: true });
    Object.defineProperty(evento, "clipboardData", { value: { getData: () => "12a3456" } });
    await act(async () => entrada.dispatchEvent(evento));
    expect(valor).toBe("123456");
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("mueve el foco con las flechas entre dígitos", async () => {
    const contenedor = document.createElement("div");
    const raiz = createRoot(contenedor);
    document.body.appendChild(contenedor);
    await act(async () =>
      raiz.render(<CampoPin label="PIN" value="12" onChange={() => {}} testid="pin" />),
    );
    const entradas = contenedor.querySelectorAll('[data-testid="pin"] input');
    entradas[0].focus();
    await act(async () =>
      entradas[0].dispatchEvent(new KeyboardEvent("keydown", { key: "ArrowRight", bubbles: true })),
    );
    expect(document.activeElement).toBe(entradas[1]);
    await act(async () => raiz.unmount());
    contenedor.remove();
  });
});
