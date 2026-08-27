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

const asistenciaCanceladaAbierta = {
  data: {
    ...asistenciaAbierta.data,
    estado: "Cancelada",
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

  it("moves focus to the confirmation card after a successful confirmation", async () => {
    api.get
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaAbierta)
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaConfirmada);
    api.post.mockResolvedValue({ data: { ok: true } });

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    const confirmButton = container.querySelector('[data-testid="confirm-btn"]');
    expect(confirmButton).not.toBeNull();

    await act(async () => {
      confirmButton.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    const confirmationCard = container.querySelector('[data-testid="confirmation-card"]');
    expect(confirmationCard).not.toBeNull();
    expect(confirmationCard.getAttribute("aria-live")).toBe("polite");
    expect(document.activeElement).toBe(confirmationCard);
    expect(container.querySelector('[data-testid="confirm-btn"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(true);

    const declineButton = container.querySelector('[data-testid="decline-btn"]');
    expect(declineButton).not.toBeNull();
    expect(declineButton.textContent).toContain("No asistiré");
    expect(declineButton.className).toContain("bg-destructive");
    expect(container.querySelector('[data-testid="countdown"]')).toBeNull();
    expect(container.querySelector('[data-testid="server-clock"]')).toBeNull();
    expect(container.querySelector('[data-testid="marca-hora-servidor"]').textContent).toContain(
      "2026-08-13 10:00:01",
    );
  });

  it("keeps confirmation disabled and uses No asistiré to remove a confirmed attendance", async () => {
    api.get
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaConfirmada)
      .mockResolvedValueOnce(respuestaMenu)
      .mockResolvedValueOnce(asistenciaCanceladaAbierta);
    api.post.mockResolvedValue({ data: { ok: true } });

    await act(async () => {
      root.render(<PortalEstudiantePrueba />);
    });

    const confirmButton = container.querySelector('[data-testid="confirm-btn"]');
    const declineButton = container.querySelector('[data-testid="decline-btn"]');
    expect(confirmButton.disabled).toBe(true);
    expect(declineButton.disabled).toBe(false);

    await act(async () => {
      declineButton.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });

    expect(api.post).toHaveBeenCalledWith("/v1/estudiantes/asistencia/decline");
    expect(container.querySelector('[data-testid="estado-rechazado"]')).not.toBeNull();
    expect(container.querySelector('[data-testid="confirm-btn"]').disabled).toBe(false);
    expect(container.querySelector('[data-testid="decline-btn"]').disabled).toBe(true);
  });
});
