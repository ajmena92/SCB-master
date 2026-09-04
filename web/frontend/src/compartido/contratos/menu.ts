/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface CalendarioMenuEntrada {
  fecha: string;
  habilitado: boolean;
  motivo?: string | null;
}

export interface CicloMenuEntrada {
  inicioCicloMenu: string;
}

export interface ComponenteMenuEntrada {
  nombre: string;
  orden: number;
  tipo?: string;
}

export interface PlantillaEntrada {
  activo?: boolean;
  componentes: Array<ComponenteMenuEntrada>;
  dia: number;
  observaciones?: string | null;
  semana: number;
  titulo: string;
}

export interface SustitucionMenuEntrada {
  componentes: Array<ComponenteMenuEntrada>;
  fecha: string;
  observaciones?: string | null;
  titulo: string;
}
