import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { vi } from "vitest";

vi.mock("@/compartido/consultas/cliente_http", () => ({
  api: { get: vi.fn(), post: vi.fn() },
  errMsg: vi.fn(() => "No fue posible completar la operación."),
}));

vi.mock("sonner", () => ({
  toast: { error: vi.fn(), success: vi.fn() },
}));

import Plantillas from "./Plantillas";
import { api } from "@/compartido/consultas/cliente_http";

const obtenerPlantillas = vi.mocked(api.get);

async function flushAsyncWork() {
  await act(async () => {
    await new Promise((resolve) => setTimeout(resolve, 0));
  });
}

async function renderTab() {
  const container = document.createElement("div");
  document.body.appendChild(container);
  const root = createRoot(container);
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  await act(async () => {
    root.render(
      <QueryClientProvider client={queryClient}>
        <Plantillas />
      </QueryClientProvider>,
    );
  });
  await flushAsyncWork();
  return { container, root };
}

describe("plantillas de menú", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("mantiene contenidos largos dentro de la cuadrícula responsive", async () => {
    obtenerPlantillas.mockResolvedValueOnce({
      data: [
        {
          IdMenuPlantilla: 7,
          SemanaMes: 1,
          DiaSemana: 2,
          Titulo: "Pasta corta con vegetales, ensalada y carne de res en salsa de tomate",
          Activo: true,
          Componentes: [{ Nombre: "Pasta" }, { Nombre: "Carne" }],
        },
      ],
    });

    const { container, root } = await renderTab();
    await act(async () => {
      container
        .querySelector('[data-testid="semana-tab-1"]')
        ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    const card = container.querySelector('[data-testid="plantilla-1-2"]');
    const title = card?.querySelector("p[title]");

    expect(card?.className).toContain("min-w-0");
    expect(title?.className).toContain("break-words");
    expect(title?.textContent).toMatch(/Pasta corta/);
    expect(container.textContent).toMatch(/1 de 5 días/);
    expect(container.querySelector("table")?.parentElement?.className).toContain("md:block");
    expect(container.querySelector('[data-testid="plantilla-movil-1-2"]')).not.toBeNull();
    expect(container.querySelector("#selector-semana-movil")).not.toBeNull();

    await act(async () => {
      root.unmount();
    });
    container.remove();
  });

  it("muestra una previsualización breve de los componentes", async () => {
    obtenerPlantillas.mockResolvedValueOnce({
      data: [
        {
          IdMenuPlantilla: 8,
          SemanaMes: 1,
          DiaSemana: 1,
          Titulo: "Arroz con pollo",
          Activo: true,
          Componentes: [
            { Nombre: "Arroz", TipoComponente: "Principal", Orden: 1 },
            { Nombre: "Ensalada", TipoComponente: "Acompañamiento", Orden: 2 },
            { Nombre: "Fruta", TipoComponente: "Postre", Orden: 3 },
          ],
        },
      ],
    });

    const { container, root } = await renderTab();
    await act(async () => {
      container
        .querySelector('[data-testid="semana-tab-1"]')
        ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    expect(container.textContent).toMatch(/Arroz · Ensalada · \+1/);

    await act(async () => {
      root.unmount();
    });
    container.remove();
  });

  it("muestra un error visible y permite reintentar la carga", async () => {
    obtenerPlantillas
      .mockRejectedValueOnce(new Error("network"))
      .mockResolvedValueOnce({ data: [] });

    const { container, root } = await renderTab();
    expect(container.querySelector('[data-testid="plantillas-error"]')?.textContent).toMatch(
      /No pudimos cargar/,
    );

    await act(async () => {
      container
        .querySelector('[data-testid="plantillas-retry"]')
        ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    await flushAsyncWork();

    expect(obtenerPlantillas).toHaveBeenCalledTimes(2);
    expect(container.querySelector('[data-testid="plantillas-error"]')).toBeNull();

    await act(async () => {
      root.unmount();
    });
    container.remove();
  });

  it("muestra una sola semana activa y cambia de semana sin otra consulta", async () => {
    obtenerPlantillas.mockResolvedValueOnce({
      data: [
        {
          IdMenuPlantilla: 1,
          SemanaMes: 1,
          DiaSemana: 1,
          Titulo: "Lunes de la semana 1",
          Activo: true,
          Componentes: [],
        },
        {
          IdMenuPlantilla: 2,
          SemanaMes: 2,
          DiaSemana: 3,
          Titulo: "Miércoles de la semana 2",
          Activo: true,
          Componentes: [],
        },
      ],
    });

    const { container, root } = await renderTab();
    await act(async () => {
      container
        .querySelector('[data-testid="semana-tab-1"]')
        ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    expect(container.textContent).toMatch(/Lunes de la semana 1/);
    expect(container.textContent).not.toMatch(/Miércoles de la semana 2/);

    await act(async () => {
      container
        .querySelector('[data-testid="semana-tab-2"]')
        ?.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    expect(container.textContent).toMatch(/Miércoles de la semana 2/);
    expect(container.textContent).not.toMatch(/Lunes de la semana 1/);
    expect(obtenerPlantillas).toHaveBeenCalledTimes(1);

    await act(async () => {
      root.unmount();
    });
    container.remove();
  });
});
