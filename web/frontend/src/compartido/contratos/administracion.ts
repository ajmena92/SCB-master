/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface CuentaAdministrativaActualizacion {
  activo?: boolean | null;
  permisos?: Array<string> | null;
  personaId?: number | null;
  rol?: "administrador" | "operador" | null;
  usuario?: string | null;
}

export interface CuentaAdministrativaEntrada {
  permisos?: Array<string>;
  personaId?: number | null;
  profesorNuevo?: ProfesorNuevoAdministrativo | null;
  rol: "administrador" | "operador";
  usuario: string;
}

export interface ProfesorNuevoAdministrativo {
  cedula: string;
  nombres: string;
}

export interface VinculacionCuentaEntrada {
  personaId?: number | null;
  profesorNuevo?: ProfesorNuevoAdministrativo | null;
}
