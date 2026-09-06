import { useNavigate, Outlet } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ShieldCheck, LogOut } from "lucide-react";
import { obtenerRutaAdministrativaPredeterminada } from "@/config/adminNavigation";
import AdminSidebar from "@/compartido/componentes/AdminSidebar";
import AdminBottomNav from "@/compartido/componentes/AdminBottomNav";
import { SelectorTema } from "@/compartido/componentes/SelectorTema";
import { plataformaApi } from "@/funcionalidades/plataforma/consultas/plataforma";
import { esAdministrador } from "@/funcionalidades/plataforma/seguridad";

export default function AdminPanel() {
  const { session, logout } = useAutenticacion();
  const navigate = useNavigate();
  const institucion = useQuery({
    queryKey: ["institucion"],
    queryFn: plataformaApi.tiquetes.institucion,
    enabled: esAdministrador(session),
    staleTime: 5 * 60 * 1000,
    retry: false,
  });
  const nombreColegio = institucion.data?.nombreColegio || "CTP Platanares";

  return (
    <div className="min-h-[100dvh] bg-background text-foreground">
      <a
        href="#admin-content"
        className="sr-only fixed left-4 top-4 z-50 rounded-lg bg-card px-4 py-3 font-semibold text-foreground shadow-lg focus:not-sr-only"
      >
        Saltar al contenido
      </a>
      <header className="sticky top-0 z-30 border-b border-border/80 bg-card/95 text-foreground backdrop-blur-xl">
        <div className="flex min-h-16 w-full items-center justify-between px-4 sm:px-6 lg:px-8">
          <button
            type="button"
            className="flex min-w-0 items-center gap-3 rounded-xl text-left focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
            onClick={() =>
              navigate(obtenerRutaAdministrativaPredeterminada(session) || "/admin/panel")
            }
            aria-label="Ir al inicio de administración"
          >
            <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-xl bg-secondary text-secondary-foreground">
              <ShieldCheck className="h-5 w-5" />
            </span>
            <span className="min-w-0">
              <span className="block truncate font-heading text-sm font-bold tracking-tight sm:text-base">
                {nombreColegio}
              </span>
              <span className="hidden text-xs font-medium text-muted-foreground sm:block">
                Administración
              </span>
            </span>
          </button>
          <div className="flex items-center gap-3">
            <SelectorTema />
            <div className="text-right hidden sm:block">
              <p className="text-sm font-semibold leading-tight">
                {session?.nombres || session?.usuario}
              </p>
              <Badge
                className="bg-primary text-primary-foreground text-[10px]"
                data-testid="admin-rol-badge"
              >
                {session?.rol}
              </Badge>
            </div>
            <Button
              variant="ghost"
              size="icon"
              data-testid="admin-logout"
              aria-label="Cerrar sesión"
              onClick={logout}
              className="text-foreground hover:bg-primary/10 hover:text-primary"
            >
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </header>

      <div className="flex min-h-[calc(100dvh-4rem)] w-full min-w-0">
        <AdminSidebar />
        <main
          id="admin-content"
          className="min-w-0 flex-1 overflow-x-hidden px-4 py-6 pb-28 sm:px-6 lg:px-8 lg:py-8 lg:pb-8 xl:px-10"
        >
          <Outlet />
        </main>
      </div>
      <AdminBottomNav />
    </div>
  );
}
