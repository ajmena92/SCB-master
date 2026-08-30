export interface SesionPlataforma {
  tipo: "admin" | "estudiante" | "profesor";
  usuario?: { Nombre?: string; nombres?: string; Rol?: string; rol?: string };
  roles?: string[];
}

export interface AutenticacionPlataforma {
  session: SesionPlataforma | false | null;
  logout: () => Promise<void>;
}

export function esAdministrador(sesion: SesionPlataforma | false | null): boolean {
  if (!sesion) return false;
  const valores = [sesion?.usuario?.Rol, sesion?.usuario?.rol, ...(sesion?.roles ?? [])]
    .filter(Boolean)
    .map((valor) => String(valor).toLowerCase());
  return valores.some((rol) => rol === "administrador" || rol === "administrator");
}
