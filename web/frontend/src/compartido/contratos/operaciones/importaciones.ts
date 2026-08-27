/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_IMPORTACIONES: readonly OperacionApi[] = [
  {
    metodo: "POST",
    ruta: "/api/v1/importaciones/lotes",
    operacionId: "ejecutar_api_v1_importaciones_lotes_post",
    dominio: "importaciones",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/importaciones/lotes/{id_lote}",
    operacionId: "lote_api_v1_importaciones_lotes__id_lote__get",
    dominio: "importaciones",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/importaciones/lotes/{id_lote}/reversion",
    operacionId: "revertir_api_v1_importaciones_lotes__id_lote__reversion_post",
    dominio: "importaciones",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/importaciones/previsualizaciones",
    operacionId: "previsualizar_api_v1_importaciones_previsualizaciones_post",
    dominio: "importaciones",
  },
] as const;
