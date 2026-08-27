import { act } from "react";
import { createRoot } from "react-dom/client";
import { vi } from "vitest";

import { api } from "@/lib/api";
import { AuthProvider, useAuth } from "./AuthContext";

vi.mock("@/lib/api", () => ({
  api: { get: vi.fn(), post: vi.fn() },
}));

function ObservadorSesion({ alCambiar }) {
  const autenticacion = useAuth();
  alCambiar(autenticacion);
  return null;
}

describe("AuthContext", () => {
  let contenedor;
  let raiz;

  beforeEach(() => {
    contenedor = document.createElement("div");
    document.body.appendChild(contenedor);
    raiz = createRoot(contenedor);
    vi.clearAllMocks();
  });

  afterEach(async () => {
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("conserva los roles y permisos explícitos devueltos por /auth/me", async () => {
    api.get.mockResolvedValueOnce({
      data: {
        tipo: "admin",
        usuario: {
          IdUsuario: 42,
          NombreCompleto: "Olga Operadora",
          EsAdministrador: 0,
          EsOperador: 1,
          roles: ["Operador"],
          permisos: ["rutas.administrar"],
        },
      },
    });
    const alCambiar = vi.fn();

    await act(async () => {
      raiz.render(
        <AuthProvider>
          <ObservadorSesion alCambiar={alCambiar} />
        </AuthProvider>,
      );
    });

    const sesion = alCambiar.mock.calls.at(-1)[0].session;
    expect(sesion).toMatchObject({
      tipo: "admin",
      roles: ["Operador"],
      permisos: ["rutas.administrar"],
      usuario: {
        Nombre: "Olga Operadora",
        Rol: "Operador",
      },
    });
  });

  it("no concede un rol ni permisos cuando /auth/me no los declara", async () => {
    api.get.mockResolvedValueOnce({
      data: {
        tipo: "admin",
        usuario: {
          IdUsuario: 43,
          NombreCompleto: "Cuenta sin asignación",
          EsAdministrador: 1,
          roles: [],
          permisos: [],
        },
      },
    });
    const alCambiar = vi.fn();

    await act(async () => {
      raiz.render(
        <AuthProvider>
          <ObservadorSesion alCambiar={alCambiar} />
        </AuthProvider>,
      );
    });

    const sesion = alCambiar.mock.calls.at(-1)[0].session;
    expect(sesion.roles).toEqual([]);
    expect(sesion.permisos).toEqual([]);
    expect(sesion.usuario.Rol).toBe("");
  });
});
