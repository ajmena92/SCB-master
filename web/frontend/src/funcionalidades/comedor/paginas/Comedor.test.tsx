import { act, createRef } from "react";
import { createRoot } from "react-dom/client";
import { beforeEach, describe, expect, it, vi } from "vitest";

const estado = vi.hoisted(() => ({ actual: null as Record<string, unknown> | null }));

vi.mock("@/funcionalidades/comedor/hooks/useMarcacionComedor", () => ({
  useMarcacionComedor: () => estado.actual,
}));

import Comedor from "./Comedor";

function crearEstado(overrides: Record<string, unknown> = {}) {
  const inputRef = createRef<HTMLInputElement>();
  return {
    codigoBarras: "E-10",
    fecha: "2026-08-28",
    horarios: [],
    configuracion: { isPending: false, isError: false },
    guardando: false,
    ultimoIngreso: null,
    errorOperacion: null,
    totalIngresos: 0,
    inputRef,
    setCodigoBarras: vi.fn(),
    setFecha: vi.fn(),
    registrar: vi.fn(),
    modoManual: false,
    setModoManual: vi.fn(),
    altoContraste: false,
    setAltoContraste: vi.fn(),
    historial: [],
    pequeno: false,
    horaServidor: "09:40:00",
    conexionDisponible: true,
    recargarHistorial: vi.fn(),
    ...overrides,
  };
}

function montar() {
  const contenedor = document.createElement("div");
  document.body.appendChild(contenedor);
  const raiz = createRoot(contenedor);
  act(() => raiz.render(<Comedor />));
  return { contenedor, raiz };
}

describe("pantalla kiosco de comedor", () => {
  beforeEach(() => {
    estado.actual = crearEstado();
    document.documentElement.requestFullscreen = vi.fn().mockResolvedValue(undefined);
    document.exitFullscreen = vi.fn().mockResolvedValue(undefined);
    Object.defineProperty(document, "fullscreenElement", { configurable: true, value: null });
  });

  it("bloquea el registro cuando la resolución es insuficiente", () => {
    estado.actual = crearEstado({ pequeno: true });
    const { contenedor, raiz } = montar();
    const pantalla = contenedor.querySelector('[data-testid="operacion-comedor"]');
    expect(pantalla).not.toBeNull();
    expect(pantalla?.querySelector('[role="alert"]')?.textContent).toContain("1280×720");
    expect([...pantalla?.querySelectorAll("button") ?? []].find((boton) => boton.textContent?.includes("Registrar ingreso"))?.disabled).toBe(true);
    act(() => raiz.unmount());
    contenedor.remove();
  });

  it("responde a las teclas operativas del kiosco", () => {
    const { contenedor, raiz } = montar();
    const foco = vi.spyOn(HTMLInputElement.prototype, "focus");
    act(() => window.dispatchEvent(new KeyboardEvent("keydown", { key: "F2" })));
    act(() => window.dispatchEvent(new KeyboardEvent("keydown", { key: "F3" })));
    act(() => window.dispatchEvent(new KeyboardEvent("keydown", { key: "F4" })));
    act(() => window.dispatchEvent(new KeyboardEvent("keydown", { key: "F7" })));
    expect(estado.actual?.setCodigoBarras as ReturnType<typeof vi.fn>).toHaveBeenCalledWith("");
    expect(foco).toHaveBeenCalled();
    expect(estado.actual?.recargarHistorial as ReturnType<typeof vi.fn>).toHaveBeenCalled();
    expect(estado.actual?.setAltoContraste as ReturnType<typeof vi.fn>).toHaveBeenCalled();
    act(() => raiz.unmount());
    contenedor.remove();
  });

  it("permite activar el modo manual y el alto contraste", () => {
    const { contenedor, raiz } = montar();
    const botones = [...contenedor.querySelectorAll("button")];
    act(() => botones.find((boton) => boton.textContent?.includes("entrada manual"))?.click());
    act(() => botones.find((boton) => boton.textContent?.includes("Alto contraste"))?.click());
    expect(estado.actual?.setModoManual as ReturnType<typeof vi.fn>).toHaveBeenCalled();
    expect(estado.actual?.setAltoContraste as ReturnType<typeof vi.fn>).toHaveBeenCalled();
    act(() => raiz.unmount());
    contenedor.remove();
  });

  it("muestra errores operativos estructurados y el estado de conexión", () => {
    estado.actual = crearEstado({
      conexionDisponible: false,
      errorOperacion: { codigo: "tiquete_agotado", mensaje: "No hay tiquetes disponibles." },
    });
    const { contenedor, raiz } = montar();
    expect(contenedor.querySelector('[data-testid="operacion-error-tiquete_agotado"]')).not.toBeNull();
    expect(contenedor.textContent).toContain("No hay tiquetes disponibles.");
    expect(contenedor.textContent).toContain("Sin conexión");
    act(() => raiz.unmount());
    contenedor.remove();
  });

  it("activa y desactiva pantalla completa mediante interacción explícita", async () => {
    const { contenedor, raiz } = montar();
    const boton = [...contenedor.querySelectorAll("button")].find((item) => item.textContent?.includes("Maximizar"));
    await act(async () => boton?.click());
    expect(document.documentElement.requestFullscreen).toHaveBeenCalled();
    act(() => raiz.unmount());
    contenedor.remove();
  });
});
