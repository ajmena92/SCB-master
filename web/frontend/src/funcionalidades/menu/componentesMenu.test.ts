import { componenteParaGuardar, prepararComponente, prepararComponentes } from "./componentesMenu";
import type { ComponenteMenu } from "./componentesMenu";

describe("componentes editables del menú", () => {
  it("asigna claves locales estables sin alterar los componentes recibidos", () => {
    const original = {
      Orden: 1,
      Nombre: "Arroz",
      TipoComponente: "Principal",
    } satisfies ComponenteMenu;

    const [editable] = prepararComponentes([original]);

    expect(editable).toMatchObject(original);
    expect(editable.claveEdicion).toEqual(expect.any(String));
    expect(original).not.toHaveProperty("claveEdicion");
  });

  it("retira la clave local antes de construir el contrato de guardado", () => {
    const editable = prepararComponente({
      Orden: 2,
      Nombre: "Ensalada",
      TipoComponente: "Acompañamiento",
    });

    expect(componenteParaGuardar(editable)).toEqual({
      Orden: 2,
      Nombre: "Ensalada",
      TipoComponente: "Acompañamiento",
    });
  });
});
