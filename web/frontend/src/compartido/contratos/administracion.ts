/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface PermisoSalida {
  activo?: boolean;
  clave: string;
  descripcion?: string | null;
}

export interface RolEntrada {
  descripcion?: string | null;
  nombre: string;
  permisos?: Array<string>;
}

export interface RolSalida {
  descripcion?: string | null;
  idRol: number;
  nombre: string;
  permisos?: Array<string>;
}

export interface UsuarioEntrada {
  activo?: boolean;
  contrasena?: string | null;
  nombreUsuario: string;
  permisos?: Array<string>;
  roles?: Array<string>;
}

export interface UsuarioSalida {
  activo: boolean;
  idUsuario: number;
  nombreUsuario: string;
  permisos?: Array<string>;
  roles?: Array<string>;
}
