/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface RutaEntrada {
  activo?: boolean;
  codigo: string;
  colorHex: string;
  descripcion: string;
}

export interface RutaSalida {
  activo: boolean;
  codigo: string;
  colorCarnetHex: string;
  descripcion: string;
  estudiantesAsignados?: number;
  idRuta: number;
}
