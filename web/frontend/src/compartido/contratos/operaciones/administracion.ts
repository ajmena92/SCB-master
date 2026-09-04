/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_ADMINISTRACION: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/administracion/cuentas",
    operacionId: "cuentas_api_v1_administracion_cuentas_get",
    dominio: "administracion",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/administracion/cuentas",
    operacionId: "crear_cuenta_api_v1_administracion_cuentas_post",
    dominio: "administracion",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/administracion/cuentas/{cuenta_id}",
    operacionId: "actualizar_cuenta_api_v1_administracion_cuentas__cuenta_id__put",
    dominio: "administracion",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/administracion/cuentas/{cuenta_id}/restablecer-contrasena",
    operacionId: "restablecer_contrasena_api_v1_administracion_cuentas__cuenta_id__restablecer_contrasena_post",
    dominio: "administracion",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/administracion/permisos",
    operacionId: "permisos_api_v1_administracion_permisos_get",
    dominio: "administracion",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/administracion/profesores-disponibles",
    operacionId: "profesores_api_v1_administracion_profesores_disponibles_get",
    dominio: "administracion",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/administracion/vinculacion-inicial",
    operacionId: "vinculacion_api_v1_administracion_vinculacion_inicial_post",
    dominio: "administracion",
  },
] as const;
