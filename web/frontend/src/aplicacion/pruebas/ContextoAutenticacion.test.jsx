import { act } from "react";
import { createRoot } from "react-dom/client";
import { vi } from "vitest";

import { api } from "@/compartido/consultas/cliente_http";
import { guardarTokenSesion } from "@/compartido/consultas/token_sesion";
import { ProveedorAutenticacion, useAutenticacion } from "../estado/ContextoAutenticacion";

vi.mock("@/compartido/consultas/cliente_http", () => ({
  api: { get: vi.fn(), post: vi.fn() },
}));

function ObservadorSesion({ alCambiar }) {
  const autenticacion = useAutenticacion();
  alCambiar(autenticacion);
  return null;
}

function Consumidor() {
  const { debeCambiarPin } = useAutenticacion();
  return <span data-testid="debe-cambiar-pin">{String(debeCambiarPin)}</span>;
}

describe("ProveedorAutenticacion", () => {
  let contenedor;
  let raiz;

  beforeEach(() => {
    contenedor = document.createElement("div");
    document.body.appendChild(contenedor);
    raiz = createRoot(contenedor);
    vi.clearAllMocks();
    sessionStorage.clear();
  });

  afterEach(async () => {
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("conserva el rol y los permisos explícitos de la sesión canónica", async () => {
    api.get.mockResolvedValueOnce({
      status: 200,
      data: {
        tipo: "administracion",
        cuentaId: 42,
        personaId: 7,
        usuario: "docente.prueba",
        nombres: "Docente de prueba",
        rol: "operador",
        permisos: ["rutas.administrar"],
        cambioContrasenaObligatorio: false,
        vinculacionPendiente: false,
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
      tipo: "administracion",
      cuentaId: 42,
      rol: "operador",
      permisos: ["rutas.administrar"],
      usuario: "docente.prueba",
      nombres: "Docente de prueba",
    });
  });

  it("no concede un rol ni permisos cuando la sesión no los declara", async () => {
    api.get.mockResolvedValueOnce({
      status: 200,
      data: {
        tipo: "administracion",
        cuentaId: 43,
        personaId: 8,
        usuario: "sin.permisos",
        nombres: "Cuenta sin asignación",
        rol: "operador",
        permisos: [],
        cambioContrasenaObligatorio: false,
        vinculacionPendiente: false,
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
    expect(sesion.permisos).toEqual([]);
    expect(sesion.rol).toBe("operador");
    expect(sesion.usuario).toBe("sin.permisos");
  });

  it("trata 204 como ausencia normal de sesión", async () => {
    api.get.mockResolvedValueOnce({ status: 204, data: undefined });
    const alCambiar = vi.fn();

    await act(async () => {
      raiz.render(
        <ProveedorAutenticacion>
          <ObservadorSesion alCambiar={alCambiar} />
        </ProveedorAutenticacion>,
      );
    });

    expect(alCambiar.mock.calls.at(-1)[0].session).toBe(false);
  });

  it("conserva la obligación de cambiar PIN al restaurar la sesión", async () => {
    api.get.mockResolvedValueOnce({
      status: 200,
      data: {
        tipo: "portal",
        rol: "estudiante",
        codigo: "E-00000018",
        cambioObligatorio: true,
      },
    });

    await act(async () => {
      raiz.render(
        <ProveedorAutenticacion>
          <Consumidor />
        </ProveedorAutenticacion>,
      );
    });

    expect(contenedor.querySelector('[data-testid="debe-cambiar-pin"]')?.textContent).toBe("true");
  });

  it("cierra la sesión en el backend y limpia el estado local incluso si el cierre falla", async () => {
    api.get.mockResolvedValueOnce({
      status: 200,
      data: { tipo: "portal", rol: "estudiante", codigo: "E-00000018" },
    });
    api.post.mockRejectedValueOnce(new Error("sin conexión"));
    guardarTokenSesion("token-activo");
    let autenticacion;

    await act(async () => {
      raiz.render(
        <ProveedorAutenticacion>
          <ObservadorSesion alCambiar={(valor) => (autenticacion = valor)} />
        </ProveedorAutenticacion>,
      );
    });
    await act(async () => {
      await autenticacion.logout();
    });

    expect(api.post).toHaveBeenCalledWith("/v1/autenticacion/logout");
    expect(sessionStorage.getItem("scb_token_sesion")).toBeNull();
    expect(autenticacion.session).toBe(false);
  });
});
