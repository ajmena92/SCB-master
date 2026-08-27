import { filterNominal } from "../../funcionalidades/administracion/paginas/Dashboard";

const rows = [
  { NombreCompleto: "Ana López", Cedula: "101230456", Seccion: "10-1" },
  { NombreCompleto: "Bruno Mora", Cedula: "202340567", Seccion: "11-2" },
];

describe("filterNominal", () => {
  it("filters by name, cédula or section without changing the source rows", () => {
    expect(filterNominal(rows, "ana")).toEqual([rows[0]]);
    expect(filterNominal(rows, "202340567")).toEqual([rows[1]]);
    expect(filterNominal(rows, "10-1")).toEqual([rows[0]]);
    expect(filterNominal(rows, "  ")).toBe(rows);
  });

  it("returns no matches when the student is not in the nominal list", () => {
    expect(filterNominal(rows, "inexistente")).toEqual([]);
  });
});
