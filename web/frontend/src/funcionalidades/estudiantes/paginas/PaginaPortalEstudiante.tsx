import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { useNavigate } from "react-router-dom";
import { PortalComedor } from "@/funcionalidades/estudiantes/componentes/PortalComedor";
import { usePortalEstudiante } from "@/funcionalidades/estudiantes/estado/usePortalEstudiante";

export default function PaginaPortalComedor() {
  const navigate = useNavigate();
  const rutaActual = window.location.pathname;
  const vistaInicial = rutaActual.endsWith("/carnet") ? "carnet" : "menu";
  const { session, logout } = useAutenticacion() as unknown as {
    session: { tipo?: string; usuario?: Record<string, string> } | null;
    logout: () => void;
  };
  const tipoPersona =
    session?.tipo === "profesor" || session?.usuario?.tipoPersona === "profesor"
      ? "profesor"
      : "estudiante";
  const estadoPortal = usePortalEstudiante(tipoPersona, vistaInicial);
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
      alCambiarVista={(vista) =>
        navigate(`${rutaActual.startsWith("/comedor") ? "/comedor" : "/portal"}/${vista}`)
      }
    />
  );
}
