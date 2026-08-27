import { describe, expect, it } from "vitest";
import { filterNominal } from "../../funcionalidades/administracion/paginas/Dashboard";

describe("filtro del dashboard", () => {
  const filas = [{ NombreCompleto: "Ana Sol", Cedula: "1", Seccion: "7-1" }, { NombreCompleto: "Luis", Cedula: "2", Seccion: null }];
  it("filtra por nombre, cédula y sección y devuelve todo vacío", () => {
    expect(filterNominal(filas, "")).toEqual(filas); expect(filterNominal(filas, "ana")).toHaveLength(1); expect(filterNominal(filas, "7-1")).toHaveLength(1); expect(filterNominal(filas, "999")).toEqual([]);
  });
});
