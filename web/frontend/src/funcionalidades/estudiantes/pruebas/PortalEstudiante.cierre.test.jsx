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

const respuestaMenu = {
  data: {
    menu: {
      Titulo: "Almuerzo",
      Componentes: [{ Orden: 1, Nombre: "Arroz", TipoComponente: "Principal" }],
    },
  },
};

const asistenciaAbierta = {
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

const asistenciaConfirmada = {
  data: {
    ...asistenciaAbierta.data,
    estado: "Confirmada",
    fechaHoraConfirmacionServidor: "2026-08-13 10:00:01",
  },
};

const asistenciaConfirmadaCerrada = {
  data: {
    ...asistenciaConfirmada.data,
    periodoCerrado: true,
    segundosParaCierre: 0,
  },
};

const asistenciaNoMarcadaCerrada = {
  data: {
    ...asistenciaAbierta.data,
    estado: "Cancelada",
    periodoCerrado: true,
    segundosParaCierre: 0,
  },
};

const asistenciaConfirmadaJustoAntesDelCierre = {
  data: {
    ...asistenciaConfirmada.data,
    segundosParaCierre: 1,
  },
};

const asistenciaJustoAntesDelCierre = {
  data: {
    ...asistenciaAbierta.data,
    segundosParaCierre: 1,
  },
};

const asistenciaAbiertaExtendida = {
  data: {
    ...asistenciaAbierta.data,
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
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaConfirmadaJustoAntesDelCierre)
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaConfirmadaCerrada);

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
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaJustoAntesDelCierre)
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaAbiertaExtendida);

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
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaConfirmadaJustoAntesDelCierre)
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaConfirmadaCerrada);

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
    ["confirmada", asistenciaConfirmadaCerrada, "Marcó asistencia al comedor"],
    ["cancelada", asistenciaNoMarcadaCerrada, "No marcó asistencia al comedor"],
  ])(
    "shows only the final state and no actions after closure for an attendance %s",
    async (_label, asistencia, textoFinal) => {
      api.get.mockResolvedValueOnce(respuestaMenu).mockResolvedValueOnce(asistencia);

      await act(async () => {
        root.render(<PortalEstudiantePrueba />);
      });

      expect(container.querySelector('[data-testid="server-clock"]')).toBeNull();
      expect(container.querySelector('[data-testid="countdown-card"]')).toBeNull();
      expect(container.querySelector('[data-testid="confirm-btn"]')).toBeNull();
      expect(container.querySelector('[data-testid="decline-btn"]')).toBeNull();
      expect(
        container.querySelector('[data-testid="final-attendance-status"]').textContent,
      ).toContain(textoFinal);
      expect(container.querySelector('[data-testid="menu-card"]')).not.toBeNull();
    },
  );
});
