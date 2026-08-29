/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface ComponenteMenu {
  nombre: string;
  orden?: number;
  tipo?: string;
}

export interface PlantillaMenuEntrada {
  activo?: boolean;
  componentes?: Array<ComponenteMenu>;
  dia: number;
  observaciones?: string | null;
  semana: number;
  titulo: string;
}

export interface PlantillaMenuSalida {
  activo?: boolean;
  componentes?: Array<ComponenteMenu>;
  dia: number;
  idPlantilla: number;
  observaciones?: string | null;
  semana: number;
  titulo: string;
}

export interface SustitucionMenuEntrada {
  componentes?: Array<ComponenteMenu>;
  fecha: string;
  observaciones?: string | null;
  titulo: string;
}

export interface SustitucionMenuSalida {
  componentes?: Array<ComponenteMenu>;
  fecha: string;
  idSustitucion: number;
  observaciones?: string | null;
  titulo: string;
}
