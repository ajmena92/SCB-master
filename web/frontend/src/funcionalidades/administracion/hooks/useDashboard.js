import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import { consultarDashboard } from "@/funcionalidades/administracion/consultas/dashboard";

export function useDashboard(fecha) {
  const consulta = useQuery({
    queryKey: ["admin", "dashboard", fecha],
    queryFn: consultarDashboard,
  });
  return {
    ...consulta,
    mensajeError: consulta.error ? mensajeError(consulta.error) : "",
  };
}
