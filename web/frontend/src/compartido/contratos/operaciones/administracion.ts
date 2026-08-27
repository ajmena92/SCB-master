/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_ADMINISTRACION: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/administracion/permisos",
    operacionId: "permisos_api_v1_administracion_permisos_get",
    dominio: "administracion",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/administracion/roles",
    operacionId: "roles_api_v1_administracion_roles_get",
    dominio: "administracion",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/administracion/roles",
    operacionId: "crear_rol_api_v1_administracion_roles_post",
    dominio: "administracion",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/administracion/usuarios",
    operacionId: "usuarios_api_v1_administracion_usuarios_get",
    dominio: "administracion",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/administracion/usuarios",
    operacionId: "crear_usuario_api_v1_administracion_usuarios_post",
    dominio: "administracion",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/administracion/usuarios/{id_usuario}",
    operacionId: "editar_usuario_api_v1_administracion_usuarios__id_usuario__put",
    dominio: "administracion",
  },
] as const;
