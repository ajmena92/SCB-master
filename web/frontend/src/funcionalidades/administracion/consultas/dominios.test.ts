import { describe, expect, it, vi } from "vitest";
import { DOMINIOS } from "./dominios";
import { api } from "@/compartido/consultas/cliente_http";

describe("catálogo de dominios administrativos", () => {
  it("expone los dominios web canónicos", () => {
    expect(Object.keys(DOMINIOS)).toEqual([
      "estudiantes",
      "asistencia",
      "beneficios",
      "comedor",
      "reportes",
      "importaciones",
      "auditoria",
    ]);
  });

  it("declara permiso y ruta para cada dominio", () => {
    for (const definicion of Object.values(DOMINIOS)) {
      expect(definicion.ruta).toMatch(/^\/api\/v1\//);
      expect(definicion.permiso).toContain(".");
      expect(definicion.columnas.length).toBeGreaterThan(0);
    }
  });

  it("normaliza respuestas en lista y objeto", async () => {
    vi.spyOn(api, "get")
      .mockResolvedValueOnce({ data: { items: [{ idEstudiante: 1 }] } } as never)
      .mockResolvedValueOnce({ data: { eventos: [{ idEvento: 2 }] } } as never);
    expect(await DOMINIOS.estudiantes.cargar()).toEqual([{ idEstudiante: 1 }]);
    expect(await DOMINIOS.auditoria.cargar()).toEqual([{ idEvento: 2 }]);
  });
});
