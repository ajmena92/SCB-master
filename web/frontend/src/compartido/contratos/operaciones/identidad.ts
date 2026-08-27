/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_IDENTIDAD: readonly OperacionApi[] = [
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion",
    operacionId: "autenticar_api_v1_autenticacion_post",
    dominio: "identidad",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/sesion",
    operacionId: "consultar_sesion_api_v1_sesion_get",
    dominio: "identidad",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/sesion/cerrar",
    operacionId: "cerrar_sesion_api_v1_sesion_cerrar_post",
    dominio: "identidad",
  },
] as const;
