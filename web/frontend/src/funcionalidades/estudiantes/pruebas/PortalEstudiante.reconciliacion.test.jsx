import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { vi } from "vitest";
import PaginaPortalEstudiante from "../paginas/PaginaPortalEstudiante";
import { api } from "@/compartido/consultas/cliente_http";

vi.mock("@/compartido/consultas/cliente_http", () => ({
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

function diferida() {
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
    const menuObsoleto = diferida();
    const asistenciaObsoleta = diferida();
    const menuActual = diferida();
    const asistenciaActual = diferida();
    api.get
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaAbierta)
      .mockImplementationOnce(() => menuObsoleto.promise)
      .mockImplementationOnce(() => asistenciaObsoleta.promise)
      .mockImplementationOnce(() => menuActual.promise)
      .mockImplementationOnce(() => asistenciaActual.promise);
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
      menuObsoleto.resolve(respuestaMenu);
      asistenciaObsoleta.resolve(asistenciaAbierta);
      await Promise.resolve();
    });
    expect(api.get).toHaveBeenCalledTimes(6);

    await act(async () => {
      menuActual.resolve(respuestaMenu);
      asistenciaActual.resolve(asistenciaConfirmada);
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="confirmation-card"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(true);
  });

  it("applies an attendance response when the concurrent menu request fails", async () => {
    vi.useFakeTimers();
    const menuFallido = diferida();
    const actualizacionAsistenciaConfirmada = diferida();
    api.get
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaAbierta)
      .mockImplementationOnce(() => menuFallido.promise)
      .mockImplementationOnce(() => actualizacionAsistenciaConfirmada.promise);

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    await act(async () => {
      vi.advanceTimersByTime(60_000);
      menuFallido.reject(new Error("menú temporalmente no disponible"));
      actualizacionAsistenciaConfirmada.resolve(asistenciaConfirmada);
      await Promise.resolve();
    });

    expect(container.querySelector('[data-testid="student-error"]')).toBeNull();
    expect(container.querySelector('[data-testid="confirmation-card"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(true);
    expect(container.querySelector('[data-testid="decline-btn"]')).not.toBeNull();
  });
});
