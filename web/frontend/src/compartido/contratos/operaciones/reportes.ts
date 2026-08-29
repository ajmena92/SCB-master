/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_REPORTES: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/dashboard",
    operacionId: "consultar_api_v1_reportes_dashboard_get",
    dominio: "reportes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/dashboard/rutas/{ruta}",
    operacionId: "detalle_ruta_api_v1_reportes_dashboard_rutas__ruta__get",
    dominio: "reportes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/estudiantes",
    operacionId: "estudiantes_api_v1_reportes_estudiantes_get",
    dominio: "reportes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/estudiantes.csv",
    operacionId: "estudiantes_csv_api_v1_reportes_estudiantes_csv_get",
    dominio: "reportes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/transporte",
    operacionId: "transporte_api_v1_reportes_transporte_get",
    dominio: "reportes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/transporte.csv",
    operacionId: "transporte_csv_api_v1_reportes_transporte_csv_get",
    dominio: "reportes",
  },
] as const;
