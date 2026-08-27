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

  it("waits for a fresh attendance reload after confirming behind an in-flight periodic reload", async () => {
    vi.useFakeTimers();
    const staleMenu = deferred();
    const staleAttendance = deferred();
    const freshMenu = deferred();
    const freshAttendance = deferred();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(openAttendance)
      .mockImplementationOnce(() => staleMenu.promise)
      .mockImplementationOnce(() => staleAttendance.promise)
      .mockImplementationOnce(() => freshMenu.promise)
      .mockImplementationOnce(() => freshAttendance.promise);
    api.post.mockResolvedValue({ data: { ok: true } });

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    await act(async () => {
      vi.advanceTimersByTime(60_000);
    });
    expect(api.get).toHaveBeenCalledTimes(4);

    await act(async () => {
      container
        .querySelector('[data-testid="confirm-btn"]')
        .dispatchEvent(new MouseEvent("click", { bubbles: true }));
      await Promise.resolve();
    });
    expect(api.post).toHaveBeenCalledWith("/v1/estudiantes/asistencia/confirm");
    expect(api.get).toHaveBeenCalledTimes(4);

    await act(async () => {
      staleMenu.resolve(menuResponse);
      staleAttendance.resolve(openAttendance);
      await Promise.resolve();
    });
    expect(api.get).toHaveBeenCalledTimes(6);

    await act(async () => {
      freshMenu.resolve(menuResponse);
      freshAttendance.resolve(confirmedAttendance);
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="confirmation-card"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(true);
  });

  it("applies an attendance response when the concurrent menu request fails", async () => {
    vi.useFakeTimers();
    const failedMenu = deferred();
    const confirmedAttendanceRefresh = deferred();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(openAttendance)
      .mockImplementationOnce(() => failedMenu.promise)
      .mockImplementationOnce(() => confirmedAttendanceRefresh.promise);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    await act(async () => {
      vi.advanceTimersByTime(60_000);
      failedMenu.reject(new Error("menú temporalmente no disponible"));
      confirmedAttendanceRefresh.resolve(confirmedAttendance);
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="student-error"]')).toBeNull();
    expect(container.querySelector('[data-testid="confirmation-card"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(true);
    expect(container.querySelector('[data-testid="decline-btn"]')).not.toBeNull();
  });
});
