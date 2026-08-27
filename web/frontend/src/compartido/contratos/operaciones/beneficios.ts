/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_BENEFICIOS: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/beneficios",
    operacionId: "listar_api_v1_beneficios_get",
    dominio: "beneficios",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/beneficios",
    operacionId: "crear_api_v1_beneficios_post",
    dominio: "beneficios",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/beneficios/estudiantes/{id_estudiante}",
    operacionId: "obtener_asignacion_api_v1_beneficios_estudiantes__id_estudiante__get",
    dominio: "beneficios",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/beneficios/estudiantes/{id_estudiante}",
    operacionId: "asignar_api_v1_beneficios_estudiantes__id_estudiante__put",
    dominio: "beneficios",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/beneficios/{id_beneficio}",
    operacionId: "editar_api_v1_beneficios__id_beneficio__put",
    dominio: "beneficios",
  },
] as const;
