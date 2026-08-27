/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface BeneficioEntrada {
  activo?: boolean;
  descripcion?: string | null;
  diasPermitidos?: number;
  nombre: string;
}

export interface BeneficioSalida {
  activo: boolean;
  descripcion: string | null;
  diasPermitidos: number;
  idBeneficio: number;
  nombre: string;
}
