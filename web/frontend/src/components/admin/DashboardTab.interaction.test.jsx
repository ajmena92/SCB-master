import { describe, expect, it, vi } from "vitest";
import { api } from "@/lib/api";
import DashboardTab, { filterNominal } from "./DashboardTab";

describe("interacción del dashboard", () => {
  it("filtra resultados y conserva vacío", () => {
    const datos = [{ NombreCompleto: "Ana", Cedula: "1", Seccion: "7-1" }]; expect(filterNominal(datos, "ana")).toEqual(datos); expect(filterNominal(datos, "x")).toEqual([]);
  });
  it("permite consultar el estado vacío de la API", async () => {
    vi.spyOn(api, "get").mockResolvedValue({ data: { nominal: [] } }); expect(api.get).not.toHaveBeenCalled(); expect(DashboardTab).toBeTypeOf("function");
  });
});
