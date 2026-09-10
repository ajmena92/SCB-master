import { api } from "@/compartido/consultas/cliente_http";
import { API } from "@/compartido/consultas/configuracion_api";

export async function consultarDashboard(fecha, filtros = {}) {
  return (
    await api.get("/v1/reportes/dashboard", {
      params: { fecha, porPagina: 25, ...filtros },
    })
  ).data;
}

/** Ruta de una exportación completa; no depende de la página nominal visible. */
export function urlListaControl(fecha, servicio, formato, filtros = {}) {
  const parametros = new URLSearchParams({ fecha, servicio, formato });
  ["busqueda", "ruta", "seccion", "confirmacion", "asistencia", "beneficio", "asignacion"].forEach(
    (clave) => {
      if (filtros[clave]) parametros.set(clave, filtros[clave]);
    },
  );
  return `${API}/v1/reportes/lista-control?${parametros.toString()}`;
}
