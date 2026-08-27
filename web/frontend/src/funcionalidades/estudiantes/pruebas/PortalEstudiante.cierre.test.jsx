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

const confirmedAttendance = {
  data: {
    ...openAttendance.data,
    estado: "Confirmada",
    fechaHoraConfirmacionServidor: "2026-08-13 10:00:01",
  },
};

const closedConfirmedAttendance = {
  data: {
    ...confirmedAttendance.data,
    periodoCerrado: true,
    segundosParaCierre: 0,
  },
};

const closedUnconfirmedAttendance = {
  data: {
    ...openAttendance.data,
    estado: "Cancelada",
    periodoCerrado: true,
    segundosParaCierre: 0,
  },
};

const justBeforeCloseConfirmedAttendance = {
  data: {
    ...confirmedAttendance.data,
    segundosParaCierre: 1,
  },
};

const justBeforeCloseAttendance = {
  data: {
    ...openAttendance.data,
    segundosParaCierre: 1,
  },
};

const extendedOpenAttendance = {
  data: {
    ...openAttendance.data,
    horaLimite: "13:00",
    segundosParaCierre: 3_600,
  },
};

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

  it("transitions from the final visible second to the closed, non-interactive result", async () => {
    vi.useFakeTimers();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(justBeforeCloseConfirmedAttendance)
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(closedConfirmedAttendance);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    await act(async () => {
      await Promise.resolve();
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="countdown"]')).toBeNull();
    expect(container.querySelector('[data-testid="server-clock"]')).toBeNull();

    await act(async () => {
      await vi.advanceTimersByTimeAsync(1_000);
    });

    await vi.waitFor(() => {
      expect(container.querySelector('[data-testid="final-attendance-status"]')).not.toBeNull();
    });

    expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();
    expect(container.querySelector('[data-testid="server-clock"]')).toBeNull();
    expect(
      container.querySelector('[data-testid="final-attendance-status"]').textContent,
    ).toContain("Marcó asistencia al comedor");

    await act(async () => {
      vi.advanceTimersByTime(5_000);
    });

    expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();
  });

  it("reopens local closure controls when the server extends the cutoff dynamically", async () => {
    vi.useFakeTimers();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(justBeforeCloseAttendance)
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(extendedOpenAttendance);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    await act(async () => {
      vi.advanceTimersByTime(1_000);
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="periodo-cerrado"]')).toBeNull();
    expect(container.querySelector('[data-testid="countdown-card"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="server-clock"]')).not.toBeNull();
  });

  it("keeps cancellation available immediately before closing and removes it at closing", async () => {
    vi.useFakeTimers();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(justBeforeCloseConfirmedAttendance)
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(closedConfirmedAttendance);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    await act(async () => {
      await Promise.resolve();
      await Promise.resolve();
    });

    const declineButton = container.querySelector('[data-testid="decline-btn"]');
    expect(declineButton).not.toBeNull();
    expect(declineButton.disabled).toBe(false);
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(true);

    await act(async () => {
      await vi.advanceTimersByTimeAsync(1_000);
    });

    await vi.waitFor(() => {
      expect(container.querySelector('[data-testid="decline-btn"]')).toBeNull();
    });

    expect(container.querySelector('[data-testid="decline-btn"]')).toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]')).toBeNull();
  });

  it.each([
    ["confirmada", closedConfirmedAttendance, "Marcó asistencia al comedor"],
    ["cancelada", closedUnconfirmedAttendance, "No marcó asistencia al comedor"],
  ])(
    "shows only the final state and no actions after closure for an attendance %s",
    async (_label, attendance, finalText) => {
      api.get.mockResolvedValueOnce(menuResponse).mockResolvedValueOnce(attendance);

      await act(async () => {
        root.render(<PortalEstudiantePrueba />);
      });

      expect(container.querySelector('[data-testid="server-clock"]')).toBeNull();
      expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();
      expect(container.querySelector('[data-testid="confirm-btn"]')).toBeNull();
      expect(container.querySelector('[data-testid="decline-btn"]')).toBeNull();
      expect(
        container.querySelector('[data-testid="final-attendance-status"]').textContent,
      ).toContain(finalText);
      expect(container.querySelector('[data-testid="menu-card"]')).not.toBeNull();
    },
  );
});
