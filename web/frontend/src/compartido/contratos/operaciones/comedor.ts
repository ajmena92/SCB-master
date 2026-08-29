/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_COMEDOR: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/comedor/operacion/configuracion",
    operacionId: "configuracion_operacion_api_v1_comedor_operacion_configuracion_get",
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
    ruta: "/api/v1/comedor/operacion/historial",
    operacionId: "historial_operacion_api_v1_comedor_operacion_historial_get",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/operacion/ingresos",
    operacionId: "ingresar_api_v1_comedor_operacion_ingresos_post",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/comedor/personas",
    operacionId: "personas_api_v1_comedor_personas_get",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/personas/profesores",
    operacionId: "crear_profesor_api_v1_comedor_personas_profesores_post",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/comedor/personas/{id_persona}/cuenta",
    operacionId: "cuenta_api_v1_comedor_personas__id_persona__cuenta_get",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/comedor/personas/{id_persona}/movimientos",
    operacionId: "movimientos_api_v1_comedor_personas__id_persona__movimientos_get",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/personas/{id_persona}/tiquetes",
    operacionId: "recargar_api_v1_comedor_personas__id_persona__tiquetes_post",
    dominio: "comedor",
  },
  {
    metodo: "DELETE",
    ruta: "/api/v1/comedor/reservas",
    operacionId: "cancelar_administrativa_api_v1_comedor_reservas_delete",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/reservas",
    operacionId: "reservar_administrativa_api_v1_comedor_reservas_post",
    dominio: "comedor",
  },
  {
    metodo: "DELETE",
    ruta: "/api/v1/comedor/reservas/estudiante",
    operacionId: "cancelar_estudiante_api_v1_comedor_reservas_estudiante_delete",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/reservas/estudiante",
    operacionId: "reservar_estudiante_api_v1_comedor_reservas_estudiante_post",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/comedor/reservas/profesor",
    operacionId: "reservar_profesor_api_v1_comedor_reservas_profesor_post",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/profesores/asistencia/hoy",
    operacionId: "asistencia_hoy_api_v1_profesores_asistencia_hoy_get",
    dominio: "comedor",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/profesores/asistencia/{accion}",
    operacionId: "registrar_asistencia_api_v1_profesores_asistencia__accion__post",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/profesores/carnet",
    operacionId: "carnet_api_v1_profesores_carnet_get",
    dominio: "comedor",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/profesores/menu",
    operacionId: "menu_api_v1_profesores_menu_get",
    dominio: "comedor",
  },
] as const;
