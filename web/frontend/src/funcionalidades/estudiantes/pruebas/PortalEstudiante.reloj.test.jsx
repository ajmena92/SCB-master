import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { vi } from "vitest";
import PaginaPortalEstudiante from "../paginas/PaginaPortalEstudiante";
import { api } from "@/lib/api";

vi.mock("@/lib/api", () => ({
  api: { get: vi.fn(), post: vi.fn() },
  errMsg: vi.fn(() => "Error"),
}));

vi.mock("@/aplicacion/estado/ContextoAutenticacion", () => ({
  useAutenticacion: () => ({ session: { usuario: { Nombre: "Ana Estudiante" } }, logout: vi.fn() }),
}));

vi.mock("react-router-dom", () => ({ useNavigate: () => vi.fn() }), { virtual: true });
vi.mock("sonner", () => ({ toast: { success: vi.fn(), warning: vi.fn(), error: vi.fn() } }));

const menuResponse = {
  data: {
    menu: {
      Titulo: "Almuerzo",
      Componentes: [{ Orden: 1, Nombre: "Arroz", TipoComponente: "Principal" }],
    },
  },
};

const openAttendance = {
  data: {
    estado: null,
    descripcionHorario: "Diurno",
    horaLimite: "12:00",
    horaServidor: "10:00:00",
    periodoAbierto: true,
    periodoCerrado: false,
    segundosParaCierre: 3600,
  },
};

const beforeOpeningAttendance = {
  data: {
    ...openAttendance.data,
    periodoAbierto: false,
    segundosParaCierre: null,
  },
};

const justBeforeOpeningAttendance = {
  data: {
    ...beforeOpeningAttendance.data,
    segundosParaApertura: 1,
  },
};

function deferred() {
  let resolve;
  let reject;
  const promise = new Promise((resolvePromise, rejectPromise) => {
    resolve = resolvePromise;
    reject = rejectPromise;
  });
  return { promise, resolve, reject };
}

let queryClient;

function PortalEstudiantePrueba() {
  return (
    <QueryClientProvider client={queryClient}>
      <PaginaPortalEstudiante />
    </QueryClientProvider>
  );
}

describe("Portal del estudiante", () => {
  let container;
  let root;

  beforeEach(() => {
    global.IS_REACT_ACT_ENVIRONMENT = true;
    container = document.createElement("div");
    document.body.appendChild(container);
    root = createRoot(container);
    queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    api.get.mockReset();
    api.post.mockReset();
  });

  afterEach(async () => {
    await act(async () => root.unmount());
    container.remove();
    vi.useRealTimers();
  });

  it("advances the server clock and remaining time every second between server refreshes", async () => {
    vi.useFakeTimers();
    api.get.mockResolvedValueOnce(menuResponse).mockResolvedValueOnce(openAttendance);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    expect(container.querySelector('[data-testid="server-clock"]').textContent).toBe("10:00:00");
    expect(container.querySelector('[data-testid="countdown"]').textContent).toBe(
      "01 h 00 min 00 s",
    );
    expect(container.querySelector('[data-testid="countdown"]').hasAttribute("aria-live")).toBe(
      false,
    );
    expect(
      container.querySelector('[data-testid="countdown"]').getAttribute("aria-labelledby"),
    ).toBe("countdown-title");

    await act(async () => {
      vi.advanceTimersByTime(1_000);
    });

    expect(container.querySelector('[data-testid="server-clock"]').textContent).toBe("10:00:01");
    expect(container.querySelector('[data-testid="countdown"]').textContent).toBe(
      "00 h 59 min 59 s",
    );
  });

  it("keeps the server clock live before the confirmation window opens", async () => {
    vi.useFakeTimers();
    api.get.mockResolvedValueOnce(menuResponse).mockResolvedValueOnce(beforeOpeningAttendance);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    expect(container.querySelector('[data-testid="server-clock"]').textContent).toBe("10:00:00");
    expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();

    await act(async () => {
      vi.advanceTimersByTime(1_000);
    });

    expect(container.querySelector('[data-testid="server-clock"]').textContent).toBe("10:00:01");
  });

  it("refreshes from the server exactly at opening before enabling attendance controls", async () => {
    vi.useFakeTimers();
    const openingMenu = deferred();
    const openingAttendance = deferred();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(justBeforeOpeningAttendance)
      .mockImplementationOnce(() => openingMenu.promise)
      .mockImplementationOnce(() => openingAttendance.promise);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    expect(container.querySelector('[data-testid="confirm-btn"]')).toBeNull();
    expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();

    await act(async () => {
      await vi.advanceTimersByTimeAsync(1_000);
    });

    expect(api.get).toHaveBeenCalledTimes(4);
    expect(container.querySelector('[data-testid="confirm-btn"]')).toBeNull();
    expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();

    await act(async () => {
      openingMenu.resolve(menuResponse);
      openingAttendance.resolve(openAttendance);
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="confirm-btn"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="countdown-card"]')).not.toBeNull();
  });
});
