/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface AccesoEstudiante {
  carne: string;
  pin: string;
}

export interface AutenticacionSalida {
  csrfToken: string;
  expiraEn: string;
  idUsuario: number;
  nombreUsuario: string;
  permisos: Array<string>;
}

export interface CambioPinEstudiante {
  pinActual: string;
  pinNuevo: string;
}

export interface CredencialesEntrada {
  contrasena: string;
  nombreUsuario: string;
}

export interface SesionActualSalida {
  expiraEn: string;
  idUsuario: number;
  tipo?: string;
  usuario?: Record<string, unknown>;
}
