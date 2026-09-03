import type { ReactNode } from "react";
import { Navigate, useLocation } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";
import { obtenerRutaAdministrativaPredeterminada } from "@/config/adminNavigation";

export default function RutaRol({
  soloAdministrador = false,
  permisos = [],
  children,
}: {
  soloAdministrador?: boolean;
  permisos?: string[];
  children: ReactNode;
}) {
  const { session } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const location = useLocation();
  const autorizado =
    esAdministrador(session) ||
    (!soloAdministrador &&
      permisos.some((permiso) => Boolean(session && session.permisos?.includes(permiso))));
  if (!autorizado) {
    const destino = obtenerRutaAdministrativaPredeterminada(session);
    if (destino && destino !== location.pathname) return <Navigate to={destino} replace />;
    return (
      <section className="mx-auto max-w-lg rounded-2xl border border-dashed p-8 text-center">
        <h2 className="font-display text-xl font-bold">Sin módulos asignados</h2>
        <p className="mt-2 text-sm text-muted-foreground">
          Tu cuenta no tiene permiso para abrir esta sección. Solicitá el acceso a un administrador.
        </p>
      </section>
    );
  }
  return children;
}
