/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

export interface DiaCalendario {
  fecha: string;
  habilitado: boolean;
}

export interface HorarioEntrada {
  horaLimite: string;
  idHorario: number;
}

export interface HorarioSalida {
  activo: boolean;
  codigo: string;
  descripcion: string;
  horaLimite: string;
  idHorario: number;
}

export interface ParametrosEntrada {
  horarios?: Array<HorarioEntrada>;
  minutosAvisoPrevio: number;
  permitirMarcaTardia?: boolean;
  permitirSinMarcaTransporte?: boolean;
}

export interface ParametrosSalida {
  horarios?: Array<HorarioSalida>;
  minutosAvisoPrevio: number;
  permitirMarcaTardia?: boolean;
  permitirSinMarcaTransporte?: boolean;
}
