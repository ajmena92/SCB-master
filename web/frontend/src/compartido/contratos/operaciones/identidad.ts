/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_IDENTIDAD: readonly OperacionApi[] = [
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion/administracion",
    operacionId: "administracion_api_v1_autenticacion_administracion_post",
    dominio: "identidad",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion/administracion/contrasena",
    operacionId: "cambiar_contrasena_administrativa_api_v1_autenticacion_administracion_contrasena_post",
    dominio: "identidad",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/autenticacion/csrf",
    operacionId: "csrf_api_v1_autenticacion_csrf_get",
    dominio: "identidad",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion/logout",
    operacionId: "logout_api_v1_autenticacion_logout_post",
    dominio: "identidad",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion/portal",
    operacionId: "portal_api_v1_autenticacion_portal_post",
    dominio: "identidad",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion/portal/pin",
    operacionId: "cambiar_pin_api_v1_autenticacion_portal_pin_post",
    dominio: "identidad",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/autenticacion/renovar",
    operacionId: "renovar_api_v1_autenticacion_renovar_post",
    dominio: "identidad",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/sesion",
    operacionId: "consultar_api_v1_sesion_get",
    dominio: "identidad",
  },
] as const;
