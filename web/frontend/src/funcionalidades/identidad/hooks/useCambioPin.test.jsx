import { act } from "react";
import { createRoot } from "react-dom/client";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";
import { ProveedorAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { api } from "@/compartido/consultas/cliente_http";
import { useCambioPin } from "./useCambioPin";

vi.mock("@/compartido/consultas/cliente_http", () => ({ api: { post: vi.fn() } }));
vi.mock("@/compartido/consultas/errores_api", () => ({ errMsg: vi.fn(() => "Error de PIN") }));

function Montar({ recibir }) {
  recibir(useCambioPin());
  return null;
}

function preparar(recibir) {
  const contenedor = document.createElement("div");
  const raiz = createRoot(contenedor);
  act(() =>
    raiz.render(
      <MemoryRouter>
        <ProveedorAutenticacion>
          <Montar recibir={recibir} />
        </ProveedorAutenticacion>
      </MemoryRouter>,
    ),
  );
  return { contenedor, raiz };
}

describe("useCambioPin", () => {
  it("valida longitud y coincidencia antes de enviar", async () => {
    let hook;
    const { contenedor, raiz } = preparar((valor) => (hook = valor));
    await act(async () => {
      hook.cambiarNuevo("123");
      hook.cambiarConfirmar("124");
    });
    await act(async () => hook.enviar({ preventDefault: vi.fn() }));
    expect(hook.error).toBe("El nuevo PIN debe tener 6 dígitos");
    expect(api.post).not.toHaveBeenCalled();
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("envía un PIN válido y mantiene el contrato de la API", async () => {
    api.post.mockResolvedValueOnce({ data: {} });
    let hook;
    const { contenedor, raiz } = preparar((valor) => (hook = valor));
    await act(async () => {
      hook.cambiarActual("111111");
      hook.cambiarNuevo("222222");
      hook.cambiarConfirmar("222222");
    });
    await act(async () => hook.enviar({ preventDefault: vi.fn() }));
    expect(api.post).toHaveBeenCalledWith("/v1/estudiantes/pin", {
      pinActual: "111111",
      pinNuevo: "222222",
    });
    await act(async () => raiz.unmount());
    contenedor.remove();
  });

  it("expone el error de conexión o servidor sin ocultarlo en la página", async () => {
    api.post.mockRejectedValueOnce(new Error("offline"));
    let hook;
    const { contenedor, raiz } = preparar((valor) => (hook = valor));
    await act(async () => {
      hook.cambiarActual("111111");
      hook.cambiarNuevo("222222");
      hook.cambiarConfirmar("222222");
    });
    await act(async () => hook.enviar({ preventDefault: vi.fn() }));
    expect(hook.error).toBe("Error de PIN");
    expect(hook.cargando).toBe(false);
    await act(async () => raiz.unmount());
    contenedor.remove();
  });
});
