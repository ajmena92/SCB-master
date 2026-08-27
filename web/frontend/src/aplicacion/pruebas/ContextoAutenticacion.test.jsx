import { act } from "react";
import { createRoot } from "react-dom/client";
import { vi } from "vitest";

import { api } from "@/lib/api";
import { ProveedorAutenticacion, useAutenticacion } from "../estado/ContextoAutenticacion";

vi.mock("@/lib/api", () => ({
  api: { get: vi.fn(), post: vi.fn() },
}));

function ObservadorSesion({ alCambiar }) {
  const autenticacion = useAutenticacion();
  alCambiar(autenticacion);
  return null;
}

describe("ProveedorAutenticacion", () => {
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

  it("conserva los roles y permisos explícitos devueltos por la sesión canónica", async () => {
    api.get.mockResolvedValueOnce({
      data: {
        tipo: "admin",
        usuario: {
          IdUsuario: 42,
          NombreCompleto: "Docente de prueba",
          roles: ["Profesor"],
          permisos: ["rutas.administrar"],
        },
      },
    });
    const alCambiar = vi.fn();

    await act(async () => {
      raiz.render(
        <ProveedorAutenticacion>
          <ObservadorSesion alCambiar={alCambiar} />
        </ProveedorAutenticacion>,
      );
    });

    const sesion = alCambiar.mock.calls.at(-1)[0].session;
    expect(sesion).toMatchObject({
      tipo: "admin",
      roles: ["Profesor"],
      permisos: ["rutas.administrar"],
      usuario: {
        Nombre: "Docente de prueba",
        Rol: "Profesor",
      },
    });
  });

  it("no concede un rol ni permisos cuando la sesión no los declara", async () => {
    api.get.mockResolvedValueOnce({
      data: {
        tipo: "admin",
        usuario: {
          IdUsuario: 43,
          NombreCompleto: "Cuenta sin asignación",
          roles: [],
          permisos: [],
        },
      },
    });
    const alCambiar = vi.fn();

    await act(async () => {
      raiz.render(
        <ProveedorAutenticacion>
          <ObservadorSesion alCambiar={alCambiar} />
        </ProveedorAutenticacion>,
      );
    });

    const sesion = alCambiar.mock.calls.at(-1)[0].session;
    expect(sesion.roles).toEqual([]);
    expect(sesion.permisos).toEqual([]);
    expect(sesion.usuario.Rol).toBe("");
  });
});
