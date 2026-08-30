/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import { OPERACIONES_IDENTIDAD } from "./operaciones/identidad";
import { OPERACIONES_ESTUDIANTES } from "./operaciones/estudiantes";
import { OPERACIONES_TRANSPORTE } from "./operaciones/transporte";
import { OPERACIONES_ASISTENCIA } from "./operaciones/asistencia";
import { OPERACIONES_CUENTAS } from "./operaciones/cuentas";
import { OPERACIONES_REPORTES } from "./operaciones/reportes";
import { OPERACIONES_IMPORTACIONES } from "./operaciones/importaciones";
import { OPERACIONES_AUDITORIA } from "./operaciones/auditoria";
import { OPERACIONES_MENU } from "./operaciones/menu";
import { OPERACIONES_COMEDOR } from "./operaciones/comedor";
import { OPERACIONES_SOPORTE } from "./operaciones/soporte";
import { OPERACIONES_ADMINISTRACION } from "./operaciones/administracion";
import { OPERACIONES_PARAMETROS } from "./operaciones/parametros";
import { OPERACIONES_SALUD } from "./operaciones/salud";
import { OPERACIONES_COMUNES } from "./operaciones/comunes";

export type MetodoHttp = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

export interface OperacionApi {
  metodo: MetodoHttp;
  ruta: string;
  operacionId: string;
  dominio: string;
}

export const OPERACIONES_API: readonly OperacionApi[] = [
  ...OPERACIONES_IDENTIDAD,
  ...OPERACIONES_ESTUDIANTES,
  ...OPERACIONES_TRANSPORTE,
  ...OPERACIONES_ASISTENCIA,
  ...OPERACIONES_CUENTAS,
  ...OPERACIONES_REPORTES,
  ...OPERACIONES_IMPORTACIONES,
  ...OPERACIONES_AUDITORIA,
  ...OPERACIONES_MENU,
  ...OPERACIONES_COMEDOR,
  ...OPERACIONES_SOPORTE,
  ...OPERACIONES_ADMINISTRACION,
  ...OPERACIONES_PARAMETROS,
  ...OPERACIONES_SALUD,
  ...OPERACIONES_COMUNES,
] as const;
