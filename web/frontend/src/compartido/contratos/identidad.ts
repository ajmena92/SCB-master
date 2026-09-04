/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface AdministracionEntrada {
  contrasena: string;
  usuario: string;
}

export interface CambioContrasenaAdministrativaEntrada {
  contrasenaActual: string;
  contrasenaNueva: string;
}

export interface CambioPinEntrada {
  pinActual: string;
  pinNuevo: string;
}

export interface PortalEntrada {
  cedula: string;
  pin: string;
}

export interface SesionSalida {
  cambioContrasenaObligatorio?: boolean;
  cambioObligatorio?: boolean;
  cuentaId?: number | null;
  expiraEn: string;
  nombres?: string | null;
  permisos?: Array<string>;
  personaId?: number | null;
  rol?: string | null;
  tipo: string;
  usuario?: string | null;
  vinculacionPendiente?: boolean;
}
