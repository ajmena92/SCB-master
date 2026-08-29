/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_MENU: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/menu/plantillas",
    operacionId: "listar_api_v1_menu_plantillas_get",
    dominio: "menu",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/menu/plantillas",
    operacionId: "guardar_api_v1_menu_plantillas_post",
    dominio: "menu",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/menu/sustitucion",
    operacionId: "guardar_sustitucion_api_v1_menu_sustitucion_post",
    dominio: "menu",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/menu/sustituciones",
    operacionId: "listar_sustituciones_api_v1_menu_sustituciones_get",
    dominio: "menu",
  },
] as const;
