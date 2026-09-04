import type { RefObject } from "react";

export type EstadoPortalApi = {
  segundosParaApertura?: number;
  segundosParaCierre?: number;
  horaServidor?: string;
  periodoAbierto?: boolean;
  periodoCerrado?: boolean;
  estado?: string;
  minutosAvisoPrevio?: number;
  descripcionHorario?: string;
  horaLimite?: string;
  horaInicio?: string;
  fechaHoraConfirmacionServidor?: string;
  sinTiquete?: boolean;
};

export type TipoPersonaComedor = "estudiante" | "profesor";

export type MenuEstudiante = {
  Titulo: string;
  Componentes: Array<{ Orden: number; Nombre: string; TipoComponente: string }>;
  Observaciones?: string;
  origen?: string;
};

export type SincronizacionPortal = {
  segundosParaApertura: number | null;
  segundosParaCierre: number | null;
  horaServidorSegundos: number | null;
  sincronizadoEn: number;
};

export type EstadoPortal = {
  menu: MenuEstudiante | null;
  vistaActiva: "menu" | "carnet";
  setVistaActiva: (vista: "menu" | "carnet") => void;
  estado: EstadoPortalApi | null;
  cargando: boolean;
  error: string;
  ejecutando: boolean;
  cerrado: boolean;
  abierto: boolean;
  asistenciaConfirmada: boolean;
  rechazada: boolean;
  servicioDisponible: boolean;
  cuentaRegresiva: string | null;
  horaServidor: string | null;
  cierreProximo: boolean;
  vistaAsistencia: string;
  tarjetaConfirmacion: RefObject<HTMLElement | null>;
  registrarAsistencia: (tipo: "confirm" | "decline") => Promise<void>;
  cargar: () => Promise<unknown>;
  carnet: {
    datos: Record<string, unknown> | null;
    error: string;
    cargando: boolean;
    recargar: () => unknown;
  };
};
