/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_IMPORTACIONES: readonly OperacionApi[] = [
  {
    metodo: "POST",
    ruta: "/api/v1/importaciones/confirmar",
    operacionId: "confirmar_api_v1_importaciones_confirmar_post",
    dominio: "importaciones",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/importaciones/previsualizar",
    operacionId: "previsualizar_api_v1_importaciones_previsualizar_post",
    dominio: "importaciones",
  },
] as const;
