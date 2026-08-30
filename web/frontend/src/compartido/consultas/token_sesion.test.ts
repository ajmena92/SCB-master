import { beforeEach, describe, expect, it } from "vitest";
import { borrarTokenSesion, guardarTokenSesion, obtenerTokenSesion } from "./token_sesion";

describe("token de sesión", () => {
  beforeEach(() => sessionStorage.clear());

  it("se conserva solo durante la sesión del navegador", () => {
    guardarTokenSesion("token-opaco");
    expect(obtenerTokenSesion()).toBe("token-opaco");
    borrarTokenSesion();
    expect(obtenerTokenSesion()).toBeNull();
  });
});
