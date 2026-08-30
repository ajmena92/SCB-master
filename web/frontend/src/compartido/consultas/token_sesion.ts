const CLAVE_TOKEN = "scb_token_sesion";

export function guardarTokenSesion(token: string): void {
  sessionStorage.setItem(CLAVE_TOKEN, token);
}

export function obtenerTokenSesion(): string | null {
  return sessionStorage.getItem(CLAVE_TOKEN);
}

export function borrarTokenSesion(): void {
  sessionStorage.removeItem(CLAVE_TOKEN);
}
