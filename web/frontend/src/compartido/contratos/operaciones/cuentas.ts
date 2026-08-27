/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_CUENTAS: readonly OperacionApi[] = [
  {
    metodo: "POST",
    ruta: "/api/v1/cuentas/{id_estudiante}/movimientos",
    operacionId: "movimiento_api_v1_cuentas__id_estudiante__movimientos_post",
    dominio: "cuentas",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/cuentas/{id_estudiante}/saldo",
    operacionId: "saldo_api_v1_cuentas__id_estudiante__saldo_get",
    dominio: "cuentas",
  },
] as const;
