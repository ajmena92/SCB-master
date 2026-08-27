/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_TRANSPORTE: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/transporte/rutas",
    operacionId: "listar_rutas_api_v1_transporte_rutas_get",
    dominio: "transporte",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/transporte/rutas",
    operacionId: "crear_ruta_api_v1_transporte_rutas_post",
    dominio: "transporte",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/transporte/rutas/paleta",
    operacionId: "listar_paleta_api_v1_transporte_rutas_paleta_get",
    dominio: "transporte",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/transporte/rutas/{id_ruta}",
    operacionId: "editar_ruta_api_v1_transporte_rutas__id_ruta__put",
    dominio: "transporte",
  },
] as const;
