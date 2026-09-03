import {
  ADMIN_NAVIGATION,
  obtenerGrupoAdministrativoActivo,
  obtenerRutaAdministrativaPredeterminada,
  obtenerModulosVisibles,
} from "@/config/adminNavigation";

describe("navegación administrativa", () => {
  it("expone todos los módulos PostgreSQL al administrador", () => {
    const modulos = obtenerModulosVisibles({
      tipo: "administracion",
      rol: "administrador",
      permisos: [],
    });
    expect(modulos).toEqual(ADMIN_NAVIGATION);
    expect(modulos.find((modulo) => modulo.id === "comedor").path).toBe("/admin/panel/comedor");
  });

  it("no concede módulos cuando el backend no declara rol ni permisos", () => {
    expect(obtenerModulosVisibles({ tipo: "administracion", permisos: [] })).toEqual([]);
    expect(
      obtenerRutaAdministrativaPredeterminada({ tipo: "administracion", permisos: [] }),
    ).toBeNull();
  });

  it("resuelve los tres grupos compactos desde sus rutas", () => {
    expect(obtenerGrupoAdministrativoActivo("/admin/panel/inicio")).toBe("principal");
    expect(obtenerGrupoAdministrativoActivo("/admin/panel/rutas")).toBe("administracion");
    expect(obtenerGrupoAdministrativoActivo("/admin/panel/personas/detalle")).toBe(
      "operacion",
    );
    expect(obtenerGrupoAdministrativoActivo("/unknown")).toBeNull();
  });

  it("usa exclusivamente las claves canónicas del catálogo RBAC", () => {
    expect(ADMIN_NAVIGATION.every((modulo) => Array.isArray(modulo.requiredPermissions))).toBe(
      true,
    );
    expect(
      ADMIN_NAVIGATION.find((modulo) => modulo.id === "dashboard").requiredPermissions,
    ).toEqual(["dashboard.leer"]);
    expect(ADMIN_NAVIGATION.find((modulo) => modulo.id === "comedor").requiredPermissions).toEqual([
      "comedor.operar",
    ]);
  });

  it("filtra al operador por permisos explícitos y nunca muestra usuarios", () => {
    const modulos = obtenerModulosVisibles({
      tipo: "administracion",
      rol: "operador",
      permisos: ["comedor.operar", "rutas.administrar"],
    });
    expect(modulos.map((modulo) => modulo.id)).toEqual(["comedor", "rutas"]);
    expect(modulos.map((modulo) => modulo.id)).not.toContain("usuarios");
  });

  it("elige como destino inicial el primer módulo realmente autorizado", () => {
    expect(
      obtenerRutaAdministrativaPredeterminada({
        tipo: "administracion",
        rol: "operador",
        permisos: ["reportes.leer"],
      }),
    ).toBe("/admin/panel/reportes");
  });
});
