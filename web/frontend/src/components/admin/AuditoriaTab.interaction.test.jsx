import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi } from "vitest";
import AuditoriaTab from "../../funcionalidades/administracion/paginas/AuditoriaEventos";
import { api } from "@/lib/api";

vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));
describe("interacción de auditoría", () => {
  it("muestra vacío tras cargar eventos canónicos", async () => {
    vi.spyOn(api, "get").mockResolvedValueOnce({ data: [] });
    const contenedor = document.createElement("div"); const root = createRoot(contenedor); const cliente = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    await act(async () => root.render(<QueryClientProvider client={cliente}><AuditoriaTab /></QueryClientProvider>)); await act(async () => new Promise((r) => setTimeout(r, 0)));
    expect(contenedor.textContent).toContain("Sin eventos"); expect(api.get).toHaveBeenCalledWith("/v1/auditoria/eventos"); await act(async () => root.unmount());
  });
});
