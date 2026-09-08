import { act, type ReactNode } from "react";
import { createRoot } from "react-dom/client";
import { describe, expect, it, vi } from "vitest";

const profesor = {
  id: 41,
  referenciaPublica: "profesor-41",
  cedula: "1-2345-6789",
  nombres: "María Profesora",
  tipo: "profesor" as const,
  activo: true,
};

vi.mock("@tanstack/react-query", () => ({
  useMutation: () => ({ mutate: vi.fn(), isPending: false, isSuccess: false, error: undefined }),
  useQuery: () => ({ data: undefined, isLoading: false, error: undefined }),
  useQueryClient: () => ({ invalidateQueries: vi.fn() }),
}));

vi.mock("react-router-dom", () => ({
  useLocation: () => ({ state: { persona: profesor } }),
  useNavigate: () => vi.fn(),
  useParams: () => ({ referencia: profesor.referenciaPublica }),
}));

vi.mock("../componentes/FotoEstudiante", () => ({
  default: ({ personaId }: { personaId: number }) => (
    <div data-testid="foto" data-persona={personaId} />
  ),
}));
vi.mock("../componentes/DialogoCredencialTemporal", () => ({ default: () => null }));
vi.mock("../componentes/ConfirmacionExpediente", () => ({ ConfirmacionExpediente: () => null }));
vi.mock("../componentes/ResumenMatriculaEstudiante", () => ({
  ResumenMatriculaEstudiante: () => <div data-testid="resumen-matricula" />,
}));
vi.mock("../componentes/ElementosComunes", () => ({
  Aviso: ({ children }: { children: ReactNode }) => <div>{children}</div>,
  Campo: ({ etiqueta, children }: { etiqueta: string; children: ReactNode }) => (
    <label>
      {etiqueta}
      {children}
    </label>
  ),
  EstadoCarga: () => <div>Cargando</div>,
}));
vi.mock("../componentes/EstadoExpedienteEstudiante", () => ({
  CargandoExpedienteEstudiante: () => <div>Cargando expediente</div>,
  ExpedienteEstudianteNoEncontrado: () => <div>Expediente no encontrado</div>,
}));

import EditarEstudiante from "./EditarEstudiante";

describe("expediente de profesor", () => {
  it("permite acceder a la fotografía sin exponer matrícula, ruta o beneficio", async () => {
    const contenedor = document.createElement("div");
    const raiz = createRoot(contenedor);

    await act(async () => raiz.render(<EditarEstudiante />));

    expect(contenedor.textContent).toContain("Expediente del profesor");
    expect(contenedor.textContent).toContain("Profesor");
    expect(contenedor.querySelector('[data-testid="foto"]')?.getAttribute("data-persona")).toBe(
      "41",
    );
    expect(contenedor.textContent).not.toContain("Ruta de transporte");
    expect(contenedor.textContent).not.toContain("Beca de comedor");
    expect(contenedor.querySelector('[data-testid="resumen-matricula"]')).toBeNull();
    expect(
      [...contenedor.querySelectorAll("button")].some((boton) =>
        boton.textContent?.includes("Guardar cambios"),
      ),
    ).toBe(false);

    await act(async () => raiz.unmount());
  });
});
