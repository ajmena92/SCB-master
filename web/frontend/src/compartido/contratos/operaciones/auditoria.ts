/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_AUDITORIA: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/auditoria/eventos",
    operacionId: "eventos_api_v1_auditoria_eventos_get",
    dominio: "auditoria",
  },
] as const;
