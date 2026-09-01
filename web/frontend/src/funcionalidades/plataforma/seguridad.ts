export interface SesionPlataforma {
  tipo: "administracion" | "estudiante" | "profesor";
  cuentaId?: number;
  personaId?: number | null;
  usuario?: Record<string, unknown> | string;
  nombres?: string | null;
  rol?: "administrador" | "operador";
  permisos?: string[];
  cambioContrasenaObligatorio?: boolean;
  vinculacionPendiente?: boolean;
}

export interface AutenticacionPlataforma {
  session: SesionPlataforma | false | null;
  logout: () => Promise<void>;
  loadMe: () => Promise<{ session: SesionPlataforma | false; debeCambiarPin: boolean }>;
  limpiarSesion: () => void;
}

export function esAdministrador(sesion: SesionPlataforma | false | null): boolean {
  if (!sesion) return false;
  return sesion.tipo === "administracion" && sesion.rol === "administrador";
}
