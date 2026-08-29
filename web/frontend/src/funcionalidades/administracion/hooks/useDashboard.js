import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import { consultarDashboard } from "@/funcionalidades/administracion/consultas/dashboard";

export function useDashboard(fecha, filtros = {}) {
  const consulta = useQuery({
    queryKey: ["admin", "dashboard", fecha, filtros],
    queryFn: () => consultarDashboard(fecha, filtros),
  });
  return {
    ...consulta,
    mensajeError: consulta.error ? mensajeError(consulta.error) : "",
  };
}
