/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_ESTUDIANTES: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes",
    operacionId: "listar_api_v1_estudiantes_get",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes",
    operacionId: "crear_api_v1_estudiantes_post",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/asistencia/hoy",
    operacionId: "asistencia_hoy_api_v1_estudiantes_asistencia_hoy_get",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes/asistencia/{accion}",
    operacionId: "registrar_asistencia_api_v1_estudiantes_asistencia__accion__post",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes/autenticacion",
    operacionId: "autenticar_api_v1_estudiantes_autenticacion_post",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/carnet",
    operacionId: "carnet_api_v1_estudiantes_carnet_get",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/carnet/foto",
    operacionId: "foto_carnet_api_v1_estudiantes_carnet_foto_get",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/menu",
    operacionId: "menu_api_v1_estudiantes_menu_get",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes/pin",
    operacionId: "cambiar_pin_api_v1_estudiantes_pin_post",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes/pines/seccion",
    operacionId: "generar_pines_seccion_api_v1_estudiantes_pines_seccion_post",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/secciones",
    operacionId: "secciones_api_v1_estudiantes_secciones_get",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/{id_estudiante}",
    operacionId: "obtener_api_v1_estudiantes__id_estudiante__get",
    dominio: "estudiantes",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/estudiantes/{id_estudiante}",
    operacionId: "editar_api_v1_estudiantes__id_estudiante__put",
    dominio: "estudiantes",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/estudiantes/{id_estudiante}/beneficio",
    operacionId: "beneficio_api_v1_estudiantes__id_estudiante__beneficio_put",
    dominio: "estudiantes",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/estudiantes/{id_estudiante}/estado-comedor",
    operacionId: "estado_comedor_api_v1_estudiantes__id_estudiante__estado_comedor_put",
    dominio: "estudiantes",
  },
  {
    metodo: "DELETE",
    ruta: "/api/v1/estudiantes/{id_estudiante}/foto",
    operacionId: "eliminar_api_v1_estudiantes__id_estudiante__foto_delete",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/{id_estudiante}/foto",
    operacionId: "consultar_api_v1_estudiantes__id_estudiante__foto_get",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes/{id_estudiante}/foto",
    operacionId: "cargar_api_v1_estudiantes__id_estudiante__foto_post",
    dominio: "estudiantes",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/estudiantes/{id_estudiante}/perfil",
    operacionId: "perfil_api_v1_estudiantes__id_estudiante__perfil_get",
    dominio: "estudiantes",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/estudiantes/{id_estudiante}/reset-pin",
    operacionId: "reset_pin_api_v1_estudiantes__id_estudiante__reset_pin_post",
    dominio: "estudiantes",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/estudiantes/{id_estudiante}/ruta",
    operacionId: "ruta_api_v1_estudiantes__id_estudiante__ruta_put",
    dominio: "estudiantes",
  },
] as const;
