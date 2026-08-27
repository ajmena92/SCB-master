import { Navigate } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Loader2 } from "lucide-react";

export function ProtectedRoute({ tipo, children }) {
  const { session } = useAutenticacion();
  if (session === null)
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <Loader2 className="h-8 w-8 animate-spin text-primary" />
      </div>
    );
  if (!session) return <Navigate to={tipo === "admin" ? "/admin" : "/"} replace />;
  if (tipo && session.tipo !== tipo)
    return <Navigate to={session.tipo === "admin" ? "/admin/panel" : "/estudiante"} replace />;
  return children;
}
