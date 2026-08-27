/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_SALUD: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/health",
    operacionId: "consultar_salud_api_health_get",
    dominio: "salud",
  },
  {
    metodo: "GET",
    ruta: "/api/ready",
    operacionId: "consultar_disponibilidad_api_ready_get",
    dominio: "salud",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/salud",
    operacionId: "consultar_salud_api_v1_salud_get",
    dominio: "salud",
  },
] as const;
