/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_REPORTES: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/comedor",
    operacionId: "comedor_api_v1_reportes_comedor_get",
    dominio: "reportes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/reportes/dashboard",
    operacionId: "dashboard_api_v1_reportes_dashboard_get",
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
    ruta: "/api/v1/reportes/ventas",
    operacionId: "ventas_api_v1_reportes_ventas_get",
    dominio: "reportes",
  },
] as const;
