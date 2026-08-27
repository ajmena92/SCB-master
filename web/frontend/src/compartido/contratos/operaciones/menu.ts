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
] as const;
