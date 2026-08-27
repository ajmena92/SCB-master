import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { mensajeError } from "@/compartido/consultas/errores";
import { toast } from "sonner";
import { consultarEventosAuditoria } from "@/funcionalidades/administracion/consultas/auditoria";

export function useAuditoria() {
  const consulta = useQuery({
    queryKey: ["admin", "auditoria"],
    queryFn: consultarEventosAuditoria,
  });
  useEffect(() => {
    if (consulta.error) toast.error(mensajeError(consulta.error));
  }, [consulta.error]);
  return consulta;
}
