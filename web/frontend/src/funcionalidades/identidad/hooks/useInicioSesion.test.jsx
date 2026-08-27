import { act } from "react";
import { createRoot } from "react-dom/client";
import { MemoryRouter } from "react-router-dom";
import { vi } from "vitest";

import { ProveedorAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { api } from "@/lib/api";
import {
  clasificarErrorAutenticacion,
  useInicioSesionAdministrativo,
  useInicioSesionEstudiantil,
} from "./useInicioSesion";

vi.mock("@/lib/api", () => ({
  api: { get: vi.fn(), post: vi.fn() },
  errMsg: vi.fn(() => "Error de autenticación"),
}));

function MontarHook({ tipo, alCambiar }) {
  const hook = tipo === "admin" ? useInicioSesionAdministrativo() : useInicioSesionEstudiantil();
  alCambiar(hook);
  return null;
}

function preparar(tipo, alCambiar) {
  const contenedor = document.createElement("div");
  const raiz = createRoot(contenedor);
  document.body.appendChild(contenedor);
  api.get.mockResolvedValue({ data: { tipo: "admin", usuario: { roles: [], permisos: [] } } });
  act(() => {
    raiz.render(
      <MemoryRouter>
        <ProveedorAutenticacion>
          <MontarHook tipo={tipo} alCambiar={alCambiar} />
        </ProveedorAutenticacion>
      </MemoryRouter>,
    );
  });
  return { contenedor, raiz };
}

describe("hooks de inicio de sesión", () => {
  beforeEach(() => vi.clearAllMocks());

  it("clasifica credenciales, servidor y conexión sin depender de la vista", () => {
    expect(clasificarErrorAutenticacion({ response: { status: 401 } })).toBe("credenciales");
    expect(clasificarErrorAutenticacion({ response: { status: 503 } })).toBe("servidor");
    expect(clasificarErrorAutenticacion(new Error("offline"))).toBe("conexion");
  });

  it("autentica administradores, carga la sesión y delega la navegación", async () => {
    api.post.mockResolvedValueOnce({ data: {} });
    api.get.mockResolvedValue({ data: { tipo: "admin", usuario: { roles: [], permisos: [] } } });
    let actual;
    const { contenedor, raiz } = preparar("admin", (valor) => (actual = valor));
    await act(async () => {
      actual.cambiarNombreUsuario("operador");
      actual.cambiarContrasena("secreto");
    });
    await act(async () => actual.enviar({ preventDefault: vi.fn() }));
    expect(api.post).toHaveBeenCalledWith(
      "/v1/autenticacion",
      { nombreUsuario: "operador", contrasena: "secreto" },
      expect.any(Object),
    );
    expect(api.get).toHaveBeenCalled();
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("rechaza un PIN incompleto antes de llamar a la API", async () => {
    let actual;
    const { contenedor, raiz } = preparar("estudiante", (valor) => (actual = valor));
    await act(async () => actual.cambiarPin("123"));
    await act(async () => actual.enviar({ preventDefault: vi.fn() }));
    expect(api.post).not.toHaveBeenCalled();
    expect(actual.error).toBe("El PIN debe tener 6 dígitos.");
    expect(actual.tipoError).toBe("validacion");
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("clasifica un error HTTP estudiantil y limpia el PIN", async () => {
    api.post.mockRejectedValueOnce({ response: { status: 503 } });
    let actual;
    const { contenedor, raiz } = preparar("estudiante", (valor) => (actual = valor));
    await act(async () => actual.cambiarPin("123456"));
    await act(async () => actual.enviar({ preventDefault: vi.fn() }));
    expect(actual.tipoError).toBe("servidor");
    expect(actual.pin).toBe("");
    await act(async () => raiz.unmount());
    contenedor.remove();
  });
});
