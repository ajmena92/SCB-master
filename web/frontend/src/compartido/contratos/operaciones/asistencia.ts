/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_ASISTENCIA: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/asistencia/marcas",
    operacionId: "listar_api_v1_asistencia_marcas_get",
    dominio: "asistencia",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/asistencia/marcas",
    operacionId: "registrar_api_v1_asistencia_marcas_post",
    dominio: "asistencia",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/asistencia/marcas/{id_marca}/correccion",
    operacionId: "corregir_api_v1_asistencia_marcas__id_marca__correccion_put",
    dominio: "asistencia",
  },
] as const;
