import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { PortalComedor } from "@/funcionalidades/estudiantes/componentes/PortalComedor";
import { usePortalEstudiante } from "@/funcionalidades/estudiantes/estado/usePortalEstudiante";

export default function PaginaPortalComedor() {
  const { session, logout } = useAutenticacion() as unknown as {
    session: { tipo?: string; usuario?: Record<string, string> } | null;
    logout: () => void;
  };
  const tipoPersona =
    session?.tipo === "profesor" || session?.usuario?.tipoPersona === "profesor"
      ? "profesor"
      : "estudiante";
  const estadoPortal = usePortalEstudiante(tipoPersona);
  const nombre =
    session?.usuario?.Nombre ||
    session?.usuario?.nombreCompleto ||
    session?.usuario?.nombres ||
    session?.usuario?.nombre ||
    "";

  return (
    <PortalComedor
      nombre={nombre}
      sesion={session}
      tipoPersona={tipoPersona}
      alCerrarSesion={logout}
      estadoPortal={estadoPortal}
    />
  );
}
