import { normalizeRuta, validateRuta } from "../../funcionalidades/rutas/paginas/Rutas";

describe("catálogo de rutas", () => {
  it("normaliza la respuesta SQL y conserva el color de identidad", () => {
    expect(
      normalizeRuta({
        IdRuta: 7,
        Codigo: "5369",
        Descripcion: "Barrio Los Ángeles",
        Activo: 1,
        ColorCarnetHex: "#EF4444",
        EstudiantesAsignados: 12,
      }),
    ).toMatchObject({
      idRuta: 7,
      codigo: "5369",
      activo: true,
      colorCarnetHex: "#EF4444",
      estudiantesAsignados: 12,
    });
  });
  it("valida el código protegido y las descripciones cortas", () => {
    expect(validateRuta({ codigo: "0", descripcion: "Recorrido válido" })).toMatch(/protegida/);
    expect(validateRuta({ codigo: "9000", descripcion: "Corto" })).toMatch(/más de 5/);
    expect(validateRuta({ codigo: "9000", descripcion: "Barrio y centro educativo" })).toBe("");
  });
});
