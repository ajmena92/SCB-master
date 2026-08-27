import { describe, expect, it, vi } from "vitest";
import { consultarUsuarios, consultarRoles, consultarPermisos, crearUsuario, crearRol, editarUsuario } from "./administracion";
import { api } from "@/lib/api";
describe("consultas de administración", () => { it("consulta usuarios por la API canónica", async () => { vi.spyOn(api, "get").mockResolvedValueOnce({ data: [] } as never); await consultarUsuarios(); expect(api.get).toHaveBeenCalledWith("/v1/administracion/usuarios"); }); });
describe("operaciones de administración", () => {
  it("consulta catálogos y ejecuta mutaciones tipadas", async () => {
    vi.spyOn(api, "get").mockResolvedValue({ data: [] } as never);
    vi.spyOn(api, "post").mockResolvedValue({ data: {} } as never);
    vi.spyOn(api, "put").mockResolvedValue({ data: {} } as never);
    await consultarRoles(); await consultarPermisos();
    await crearUsuario({ nombreUsuario: "ana", contrasena: "12345678", activo: true });
    await crearRol({ nombre: "Profesor", descripcion: "Acceso docente" });
    await editarUsuario(4, { activo: false, permisos: ["auditoria.leer"] });
    expect(api.get).toHaveBeenCalledWith("/v1/administracion/roles");
    expect(api.get).toHaveBeenCalledWith("/v1/administracion/permisos");
    expect(api.post).toHaveBeenCalledTimes(2); expect(api.put).toHaveBeenCalledWith("/v1/administracion/usuarios/4", { activo: false, permisos: ["auditoria.leer"] });
  });
});
