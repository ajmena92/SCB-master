/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_PARAMETROS: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/calendario",
    operacionId: "calendario_api_v1_calendario_get",
    dominio: "parametros",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/parametros",
    operacionId: "obtener_api_v1_parametros_get",
    dominio: "parametros",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/parametros",
    operacionId: "guardar_api_v1_parametros_put",
    dominio: "parametros",
  },
] as const;
