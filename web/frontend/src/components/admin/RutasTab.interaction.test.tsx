import { act } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi } from "vitest";
import RutasTab from "./RutasTab";
import { api } from "@/lib/api";

vi.mock("sonner", () => ({ toast: { success: vi.fn(), error: vi.fn() } }));
describe("interacción de rutas", () => {
  it("muestra catálogo vacío y permite abrir creación", async () => {
    vi.spyOn(api, "get").mockResolvedValue({ data: [] } as never);
    const contenedor = document.createElement("div"); const root = createRoot(contenedor); const cliente = new QueryClient({ defaultOptions: { queries: { retry: false } } });
    await act(async () => root.render(<QueryClientProvider client={cliente}><RutasTab /></QueryClientProvider>)); await act(async () => new Promise((r) => setTimeout(r, 0)));
    expect(contenedor.textContent).toContain("No hay rutas");
    const nuevo = contenedor.querySelector('[data-testid="ruta-nueva"]'); expect(nuevo).not.toBeNull(); await act(async () => nuevo?.dispatchEvent(new MouseEvent("click", { bubbles: true })));
    expect(document.body.textContent).toContain("Nueva ruta"); await act(async () => root.unmount());
  });
});
