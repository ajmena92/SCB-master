export type RolAdministrativo = "administrador" | "operador";

export interface PersonaCuentaAdministrativa {
  id: number;
  cedula: string;
  nombres: string;
}

export interface CuentaAdministrativa {
  id: number;
  persona: PersonaCuentaAdministrativa | null;
  usuario: string;
  rol: RolAdministrativo;
  activo: boolean;
  permisos: string[];
  cambioContrasenaObligatorio: boolean;
  vinculacionPendiente: boolean;
}

export interface ProfesorDisponible {
  id: number;
  cedula: string;
  nombres: string;
}

export interface PermisoAdministrativo {
  clave: string;
  descripcion: string;
  modulo: string;
}

export interface ProfesorNuevo {
  cedula: string;
  nombres: string;
}

export interface CuentaCrearEntrada {
  usuario: string;
  rol: RolAdministrativo;
  permisos: string[];
  personaId?: number;
  profesorNuevo?: ProfesorNuevo;
}

export interface CuentaEditarEntrada {
  usuario?: string;
  rol?: RolAdministrativo;
  activo?: boolean;
  permisos?: string[];
  personaId?: number;
}

export interface CredencialesTemporales {
  contrasena: string;
  pin?: string;
}

export interface RespuestaCuentaCreada {
  cuenta: CuentaAdministrativa;
  credencialesTemporales: CredencialesTemporales;
}

export interface RespuestaRestablecimiento {
  contrasenaTemporal: string;
  cambioContrasenaObligatorio: true;
  sesionesRevocadas: true;
}

export interface VinculacionInicialEntrada {
  personaId?: number;
  profesorNuevo?: ProfesorNuevo;
}

export interface RespuestaVinculacionInicial {
  cuenta: CuentaAdministrativa;
  pinTemporal?: string;
}

export interface CredencialesParaMostrar {
  nombres: string;
  usuario: string;
  contrasena: string;
  pin?: string;
}
