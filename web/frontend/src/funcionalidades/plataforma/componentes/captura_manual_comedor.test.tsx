import { act, createRef } from "react";
import { createRoot } from "react-dom/client";
import { expect, it, vi } from "vitest";
import { CapturaManualComedor } from "./captura_manual_comedor";

it("conserva captura, foco y cierre y bloquea el envío mientras valida", async () => {
  const contenedor = document.createElement("div");
  const raiz = createRoot(contenedor);
  const referencia = createRef<HTMLInputElement>();
  const registrar = vi.fn((evento) => evento.preventDefault());
  const ocultar = vi.fn();
  const mostrar = (pendiente: boolean) =>
    raiz.render(
      <CapturaManualComedor
        referenciaEntrada={referencia}
        pendiente={pendiente}
        alRegistrar={registrar}
        alOcultar={ocultar}
      />,
    );
  try {
    await act(async () => mostrar(false));
    expect(referencia.current).toBe(contenedor.querySelector("input"));
    expect(referencia.current?.name).toBe("codigo");
    await act(async () => {
      contenedor
        .querySelector("form")!
        .dispatchEvent(new Event("submit", { bubbles: true, cancelable: true }));
      contenedor.querySelector<HTMLButtonElement>('[aria-label="Ocultar respaldo"]')!.click();
    });
    expect(registrar).toHaveBeenCalledTimes(1);
    expect(ocultar).toHaveBeenCalledTimes(1);
    await act(async () => mostrar(true));
    const enviar = contenedor.querySelector<HTMLButtonElement>('button:not([type="button"])');
    expect(enviar?.disabled).toBe(true);
    expect(enviar?.textContent).toBe("Validando…");
  } finally {
    await act(async () => raiz.unmount());
  }
});
