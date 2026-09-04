/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_COMEDOR: readonly OperacionApi[] = [
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/autorizaciones",
    operacionId: "autorizar_api_v1_comedor_autorizaciones_post",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/operacion",
    operacionId: "ingresar_api_v1_comedor_operacion_post",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/comedor/operacion/estado",
    operacionId: "estado_operacion_api_v1_comedor_operacion_estado_get",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/comedor/personas/{persona_id}/foto",
    operacionId: "foto_persona_comedor_api_v1_comedor_personas__persona_id__foto_get",
    dominio: "comedor",
  },
  {
    metodo: "DELETE",
    ruta: "/api/v1/comedor/reservas",
    operacionId: "cancelar_api_v1_comedor_reservas_delete",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/reservas",
    operacionId: "reserva_api_v1_comedor_reservas_post",
    dominio: "comedor",
  },
] as const;
