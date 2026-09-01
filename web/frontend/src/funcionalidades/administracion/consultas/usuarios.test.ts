import { beforeEach, describe, expect, it, vi } from "vitest";
import { api } from "@/compartido/consultas/cliente_http";
import { usuariosAdministrativosApi } from "./usuarios";

vi.mock("@/compartido/consultas/cliente_http", () => ({
  api: { get: vi.fn(), post: vi.fn(), put: vi.fn() },
}));

describe("usuariosAdministrativosApi", () => {
  beforeEach(() => vi.clearAllMocks());

  it("usa únicamente las rutas canónicas de cuentas y permisos", async () => {
    vi.mocked(api.get).mockResolvedValue({ data: [] });
    await usuariosAdministrativosApi.listar();
    await usuariosAdministrativosApi.permisos();
    await usuariosAdministrativosApi.profesores();
    expect(api.get).toHaveBeenNthCalledWith(1, "/v1/administracion/cuentas");
    expect(api.get).toHaveBeenNthCalledWith(2, "/v1/administracion/permisos");
    expect(api.get).toHaveBeenNthCalledWith(3, "/v1/administracion/profesores-disponibles");
  });

  it("envía sin adaptadores el alta y la vinculación inicial", async () => {
    const nueva = {
      usuario: "operador",
      rol: "operador" as const,
      permisos: ["comedor.operar"],
      personaId: 12,
    };
    vi.mocked(api.post).mockResolvedValue({ data: {} });
    await usuariosAdministrativosApi.crear(nueva);
    await usuariosAdministrativosApi.vincular({ personaId: 12 });
    expect(api.post).toHaveBeenNthCalledWith(1, "/v1/administracion/cuentas", nueva);
    expect(api.post).toHaveBeenNthCalledWith(2, "/v1/administracion/vinculacion-inicial", {
      personaId: 12,
    });
  });

  it("cambia la contraseña administrativa mediante su endpoint exclusivo", async () => {
    vi.mocked(api.post).mockResolvedValue({ data: {} });
    await usuariosAdministrativosApi.cambiarContrasena("temporal", "definitiva-segura");
    expect(api.post).toHaveBeenCalledWith("/v1/autenticacion/administracion/contrasena", {
      contrasenaActual: "temporal",
      contrasenaNueva: "definitiva-segura",
    });
  });
});
