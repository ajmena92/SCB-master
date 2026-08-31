import {
  ADMIN_NAVIGATION,
  obtenerGrupoAdministrativoActivo,
  getDefaultAdminRoute,
  obtenerModulosVisibles,
} from "@/config/adminNavigation";

describe("administrative tabs", () => {
  it("expone todos los módulos PostgreSQL al Administrador", () => {
    const adminModules = obtenerModulosVisibles({ usuario: { Rol: "Administrador" } });

    expect(adminModules).toHaveLength(ADMIN_NAVIGATION.length);
    expect(adminModules.map((module) => module.v)).toContain("dashboard");
    expect(adminModules.find((module) => module.id === "comedor").path).toBe(
      "/admin/panel/comedor",
    );
    expect(adminModules).toEqual(ADMIN_NAVIGATION);
  });

  it("reconoce una sesión administrativa aunque el backend aún no haya cargado el rol", () => {
    const adminModules = obtenerModulosVisibles({ tipo: "admin", usuario: { idUsuario: 1 } });

    expect(adminModules).toEqual(ADMIN_NAVIGATION);
  });

  it("does not expose modules to an Usuario sin permisos explícitos", () => {
    const operatorModules = obtenerModulosVisibles({ usuario: { Rol: "Profesor" } });

    expect(operatorModules).toHaveLength(0);
    expect(operatorModules.map((module) => module.v)).not.toContain("correcciones");
    expect(getDefaultAdminRoute({ usuario: { Rol: "Profesor" } })).toBe("/admin/panel/inicio");
  });

  it("resolves the navigation group from a target route", () => {
    expect(obtenerGrupoAdministrativoActivo("/admin/panel/rutas")).toBe("operacion");
    expect(obtenerGrupoAdministrativoActivo("/admin/panel/personas/details")).toBe("personas");
    expect(obtenerGrupoAdministrativoActivo("/unknown")).toBeNull();
  });

  it("keeps permission metadata in the central catalog for later RBAC enforcement", () => {
    expect(ADMIN_NAVIGATION.every((module) => Array.isArray(module.requiredPermissions))).toBe(
      true,
    );
    expect(ADMIN_NAVIGATION.find((module) => module.v === "dashboard").requiredPermissions).toEqual(
      ["reportes.dashboard.leer"],
    );
  });

  it("does not resolve a route that is not part of the catalog", () => {
    expect(obtenerGrupoAdministrativoActivo("/admin/panel/operacion/no-existe")).toBeNull();
  });

  it("filters an operator by the permissions returned by the API", () => {
    const modules = obtenerModulosVisibles({
      usuario: { Rol: "Profesor" },
      permisos: ["comedor.registrar"],
    });
    expect(modules.map((module) => module.id)).toEqual(["comedor"]);
  });

  it("muestra Inicio a un usuario con el permiso canónico del dashboard", () => {
    const modules = obtenerModulosVisibles({
      usuario: { Rol: "Profe" },
      permisos: ["reportes.dashboard.leer"],
    });

    expect(modules.map((module) => module.id)).toEqual(["dashboard"]);
  });
});
