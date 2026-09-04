/** Generado por web/scripts/generar_cliente_openapi.py; no editar manualmente. */

import type { OperacionApi } from "../operaciones";

export const OPERACIONES_MENU: readonly OperacionApi[] = [
  {
    metodo: "GET",
    ruta: "/api/v1/menu/calendario",
    operacionId: "calendario_api_v1_menu_calendario_get",
    dominio: "menu",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/menu/calendario",
    operacionId: "actualizar_calendario_api_v1_menu_calendario_put",
    dominio: "menu",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/menu/ciclo",
    operacionId: "ciclo_menu_api_v1_menu_ciclo_get",
    dominio: "menu",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/menu/ciclo",
    operacionId: "configurar_ciclo_menu_api_v1_menu_ciclo_put",
    dominio: "menu",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/menu/plantillas",
    operacionId: "plantillas_api_v1_menu_plantillas_get",
    dominio: "menu",
  },
  {
    metodo: "POST",
    ruta: "/api/v1/menu/plantillas",
    operacionId: "crear_api_v1_menu_plantillas_post",
    dominio: "menu",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/menu/plantillas/{semana}/{dia}",
    operacionId: "actualizar_api_v1_menu_plantillas__semana___dia__put",
    dominio: "menu",
  },
  {
    metodo: "GET",
    ruta: "/api/v1/menu/sustituciones",
    operacionId: "sustituciones_api_v1_menu_sustituciones_get",
    dominio: "menu",
  },
  {
    metodo: "PUT",
    ruta: "/api/v1/menu/sustituciones/{fecha}",
    operacionId: "actualizar_sustitucion_api_v1_menu_sustituciones__fecha__put",
    dominio: "menu",
  },
] as const;
