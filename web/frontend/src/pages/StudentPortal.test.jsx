import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { vi } from "vitest";
import StudentPortalBase from "./StudentPortal";
import { api } from "@/lib/api";

vi.mock("@/lib/api", () => ({
  api: { get: vi.fn(), post: vi.fn() },
  errMsg: vi.fn(() => "Error"),
}));

vi.mock("@/context/AuthContext", () => ({
  useAuth: () => ({ session: { usuario: { Nombre: "Ana Estudiante" } }, logout: vi.fn() }),
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

const declinedOpenAttendance = {
  data: {
    ...openAttendance.data,
    estado: "Cancelada",
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

const extendedOpenAttendance = {
  data: {
    ...openAttendance.data,
    horaLimite: "13:00",
    segundosParaCierre: 3_600,
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

function StudentPortal() {
  return (
    <QueryClientProvider client={queryClient}>
      <StudentPortalBase />
    </QueryClientProvider>
  );
}

describe("StudentPortal confirmation", () => {
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
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(openAttendance)
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(confirmedAttendance);
    api.post.mockResolvedValue({ data: { ok: true } });

    await act(async () => {
      root.render(<StudentPortal />);
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
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(confirmedAttendance)
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(declinedOpenAttendance);
    api.post.mockResolvedValue({ data: { ok: true } });

    await act(async () => {
      root.render(<StudentPortal />);
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

  it("advances the server clock and remaining time every second between server refreshes", async () => {
    vi.useFakeTimers();
    api.get.mockResolvedValueOnce(menuResponse).mockResolvedValueOnce(openAttendance);

    await act(async () => {
      root.render(<StudentPortal />);
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
      root.render(<StudentPortal />);
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
      root.render(<StudentPortal />);
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
      root.render(<StudentPortal />);
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
      root.render(<StudentPortal />);
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

  it("transitions from the final visible second to the closed, non-interactive result", async () => {
    vi.useFakeTimers();
    api.get
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(justBeforeCloseConfirmedAttendance)
      .mockResolvedValueOnce(menuResponse)
      .mockResolvedValueOnce(closedConfirmedAttendance);

    await act(async () => {
      root.render(<StudentPortal />);
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
      root.render(<StudentPortal />);
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
      root.render(<StudentPortal />);
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
        root.render(<StudentPortal />);
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
