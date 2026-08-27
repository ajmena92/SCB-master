import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { PortalEstudiante } from "@/funcionalidades/estudiantes/componentes/PortalEstudiante";
import { usePortalEstudiante } from "@/funcionalidades/estudiantes/estado/usePortalEstudiante";

export default function PaginaPortalEstudiante() {
  const { session, logout } = useAutenticacion();
  const estadoPortal = usePortalEstudiante();
  const nombre =
    session?.usuario?.Nombre || session?.usuario?.nombreCompleto || session?.usuario?.nombre || "";

  return (
    <PortalEstudiante
      nombre={nombre}
      sesion={session}
      alCerrarSesion={logout}
      estadoPortal={estadoPortal}
    />
  );
}
