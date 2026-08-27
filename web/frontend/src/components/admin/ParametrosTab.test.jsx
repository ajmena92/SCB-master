import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { vi } from "vitest";

vi.mock("@/lib/api", () => ({
  api: { get: vi.fn(), put: vi.fn() },
  errMsg: vi.fn(() => "No fue posible completar la operación."),
}));

import ParametrosTabBase, { normalizeParametros, validateParametros } from "./ParametrosTab";
import { api, errMsg } from "@/lib/api";

const SETTINGS = {
  minutosAvisoPrevio: 15,
  horarios: [
    { idHorario: 1, descripcion: "Diurno", horaInicio: "06:00", horaLimite: "10:30", activo: true },
    {
      idHorario: 2,
      descripcion: "Nocturno",
      horaInicio: "17:00",
      horaLimite: "20:30",
      activo: true,
    },
  ],
};

async function renderTab() {
  const container = document.createElement("div");
  document.body.appendChild(container);
  const root = createRoot(container);
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  await act(async () => {
    root.render(
      <QueryClientProvider client={queryClient}>
        <ParametrosTabBase />
      </QueryClientProvider>,
    );
  });
  await flushAsyncWork();
  return { container, root };
}

async function flushAsyncWork() {
  await act(async () => {
    await new Promise((resolve) => setTimeout(resolve, 0));
  });
}

async function waitForSelector(container, selector, attempts = 5) {
  for (let attempt = 0; attempt < attempts; attempt += 1) {
    await flushAsyncWork();
    const element = container.querySelector(selector);
    if (element) return element;
  }
  return null;
}

async function setInput(input, value) {
  await act(async () => {
    const setter = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value").set;
    setter.call(input, value);
    input.dispatchEvent(new Event("input", { bubbles: true }));
  });
}

async function click(element) {
  await act(async () => {
    element.dispatchEvent(new MouseEvent("click", { bubbles: true }));
  });
}

describe("parámetros del portal", () => {
  beforeEach(() => {
    global.IS_REACT_ACT_ENVIRONMENT = true;
    vi.clearAllMocks();
    errMsg.mockReturnValue("No fue posible completar la operación.");
  });

  it("normaliza la respuesta de la API para que el formulario use sus campos", () => {
    expect(
      normalizeParametros({
        minutosAvisoPrevio: 20,
        Horarios: [
          { IdHorario: 2, Descripcion: "Nocturno", HoraInicio: "17:00", HoraLimite: "20:30" },
        ],
      }),
    ).toEqual({
      minutosAvisoPrevio: "20",
      horarios: [
        {
          idHorario: 2,
          descripcion: "Nocturno",
          horaApertura: "17:00",
          horaLimite: "20:30",
          activo: true,
        },
      ],
    });
  });

  it("prioriza el nombre público del contrato sobre el alias legado", () => {
    expect(
      normalizeParametros({ minutosAvisoPrevio: 30, minutosAviso: 15 }).minutosAvisoPrevio,
    ).toBe("30");
  });

  it("valida los límites editables antes de enviar", () => {
    expect(validateParametros({ minutosAvisoPrevio: "0", horarios: [] })).toMatch(/1 y 120/);
    expect(
      validateParametros({ minutosAvisoPrevio: "15", horarios: [{ horaLimite: "9:00" }] }),
    ).toMatch(/HH:mm/);
    expect(
      validateParametros({ minutosAvisoPrevio: "15", horarios: [{ horaLimite: "09:00" }] }),
    ).toBe("");
  });

  it("loads settings and saves the edited payload", async () => {
    api.get.mockResolvedValueOnce({ data: SETTINGS });
    api.put.mockResolvedValueOnce({ data: SETTINGS });
    const { container, root } = await renderTab();

    expect(api.get).toHaveBeenCalledWith("/v1/parametros");
    expect(container.querySelector('[data-testid="parametro-horario-1"]')).not.toBeNull();

    await setInput(container.querySelector('[data-testid="parametros-minutos-aviso"]'), "20");
    await setInput(container.querySelector('[data-testid="parametro-hora-limite-2"]'), "21:00");
    await click(container.querySelector('[data-testid="parametros-guardar"]'));

    expect(api.put).toHaveBeenCalledWith("/v1/parametros", {
      minutosAvisoPrevio: 20,
      horarios: [
        { idHorario: 1, horaLimite: "10:30" },
        { idHorario: 2, horaLimite: "21:00" },
      ],
    });
    expect(container.querySelector('[data-testid="parametros-success"]')?.textContent).toMatch(
      /Parámetros actualizados/,
    );
    await act(async () => {
      root.unmount();
    });
  });

  it("shows a visible error when loading parameters fails", async () => {
    api.get.mockRejectedValueOnce({ response: { status: 500 } });
    const { container, root } = await renderTab();

    const error = await waitForSelector(container, '[data-testid="parametros-error"]');
    expect(error?.textContent).toMatch(/No fue posible completar la operación/);
    await act(async () => {
      root.unmount();
    });
  });

  it("shows inactive schedules as read-only and excludes them from the save payload", async () => {
    api.get.mockResolvedValueOnce({
      data: {
        ...SETTINGS,
        horarios: [
          ...SETTINGS.horarios,
          {
            idHorario: 3,
            descripcion: "Especial",
            horaInicio: "06:00",
            horaLimite: "11:00",
            activo: false,
          },
        ],
      },
    });
    api.put.mockResolvedValueOnce({ data: SETTINGS });
    const { container, root } = await renderTab();

    const inactiveBadge = await waitForSelector(
      container,
      '[data-testid="parametro-horario-inactivo-3"]',
    );
    expect(inactiveBadge?.textContent).toBe("Inactivo");
    expect(container.querySelector('[data-testid="parametro-hora-limite-3"]')?.disabled).toBe(true);

    await click(container.querySelector('[data-testid="parametros-guardar"]'));

    expect(api.put).toHaveBeenCalledWith("/v1/parametros", {
      minutosAvisoPrevio: 15,
      horarios: [
        { idHorario: 1, horaLimite: "10:30" },
        { idHorario: 2, horaLimite: "20:30" },
      ],
    });
    await act(async () => {
      root.unmount();
    });
  });
});
