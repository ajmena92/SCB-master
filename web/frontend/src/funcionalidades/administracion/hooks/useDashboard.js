import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import { consultarDashboard } from "@/funcionalidades/administracion/consultas/dashboard";

export function useDashboard(fecha, filtros = {}) {
  const consulta = useQuery({
    queryKey: ["admin", "dashboard", fecha, filtros],
    queryFn: () => consultarDashboard(fecha, filtros),
    // Los filtros de la lista nominal no deben desmontar el tablero ni mover
    // el foco mientras llega la respuesta siguiente.
    placeholderData: (datosAnteriores) => datosAnteriores,
  });
  return {
    ...consulta,
    mensajeError: consulta.error ? mensajeError(consulta.error) : "",
  };
}
