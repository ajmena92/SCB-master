import type { ReactNode } from "react";
import { Navigate } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { esAdministrador, type AutenticacionPlataforma } from "../seguridad";

export default function RutaRol({
  soloAdministrador = false,
  children,
}: {
  soloAdministrador?: boolean;
  children: ReactNode;
}) {
  const { session } = useAutenticacion() as unknown as AutenticacionPlataforma;
  if (soloAdministrador && !esAdministrador(session))
    return <Navigate to="/admin/panel/inicio" replace />;
  return children;
}
