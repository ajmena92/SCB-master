import { act } from "react";
import { createRoot } from "react-dom/client";
import { beforeEach, describe, expect, it, vi } from "vitest";

/* El arnés inspecciona el valor del hook para verificar el temporizador. */
/* eslint-disable react-hooks/refs */

const registrarMarcacion = vi.fn();
const consultarHistorial = vi.fn();

vi.mock("@tanstack/react-query", () => ({
  useQuery: () => ({
    data: { horarios: [], horaServidor: "09:00:00" },
    isPending: false,
    isError: false,
  }),
}));
vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));
vi.mock("@/funcionalidades/comedor/consultas/marcacion", () => ({
  consultarConfiguracionComedor: vi.fn(),
  registrarMarcacionComedor: (...args: unknown[]) => registrarMarcacion(...args),
  consultarHistorialComedor: (...args: unknown[]) => consultarHistorial(...args),
}));

import { useMarcacionComedor } from "./useMarcacionComedor";

function PruebaHook() {
  const estado = useMarcacionComedor();
  return (
    <>
      <input
        ref={estado.inputRef}
        value={estado.codigoBarras}
        onChange={(evento) => estado.setCodigoBarras(evento.target.value)}
      />
      <button type="button" onClick={() => void estado.registrar()}>
        registrar
      </button>
      <button type="button" onClick={() => estado.setCodigoBarras("E-10")}>
        preparar
      </button>
      <output>{estado.ultimoIngreso?.nombreCompleto ?? "vacio"}</output>
    </>
  );
}

describe("useMarcacionComedor", () => {
  beforeEach(() => {
    vi.useFakeTimers();
    registrarMarcacion.mockResolvedValue({
      idIngreso: 1,
      nombreCompleto: "Estudiante",
      modalidad: "beca",
      resultado: "registrado",
    });
  });

  it("limpia el último resultado después de 60 segundos", async () => {
    const contenedor = document.createElement("div");
    document.body.appendChild(contenedor);
    const raiz = createRoot(contenedor);
    act(() => raiz.render(<PruebaHook />));
    const boton = contenedor.querySelector("button") as HTMLButtonElement;
    const preparar = contenedor.querySelectorAll("button")[1] as HTMLButtonElement;
    act(() => preparar.click());
    await act(async () => boton.click());
    expect(contenedor.querySelector("output")?.textContent).toBe("Estudiante");
    act(() => vi.advanceTimersByTime(60_000));
    expect(contenedor.querySelector("output")?.textContent).toBe("vacio");
    act(() => raiz.unmount());
    contenedor.remove();
    vi.useRealTimers();
  });
});
