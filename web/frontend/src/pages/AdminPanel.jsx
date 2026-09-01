import { useLocation, useNavigate, Outlet } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ShieldCheck, LogOut } from "lucide-react";
import {
  obtenerGrupoAdministrativoActivo,
  ADMIN_NAVIGATION,
  obtenerRutaAdministrativaPredeterminada,
} from "@/config/adminNavigation";
import AdminSidebar from "@/compartido/componentes/AdminSidebar";
import AdminBottomNav from "@/compartido/componentes/AdminBottomNav";

export default function AdminPanel() {
  const { session, logout } = useAutenticacion();
  const navigate = useNavigate();
  const location = useLocation();
  const activeModule = ADMIN_NAVIGATION.find((item) => item.path === location.pathname);
  const activeGroup = obtenerGrupoAdministrativoActivo(location.pathname);

  return (
    <div className="min-h-[100dvh] bg-background text-foreground">
      <a
        href="#admin-content"
        className="sr-only fixed left-4 top-4 z-50 rounded-lg bg-card px-4 py-3 font-semibold text-foreground shadow-lg focus:not-sr-only"
      >
        Saltar al contenido
      </a>
      <header className="sticky top-0 z-30 border-b border-border/80 bg-card/95 text-secondary backdrop-blur-xl">
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
              <span className="block truncate font-display text-sm font-black tracking-tight sm:text-base">
                Comedor SCSC
              </span>
              <span className="hidden text-xs font-medium text-muted-foreground sm:block">
                Administración
              </span>
            </span>
          </button>
          <div className="flex items-center gap-3">
            <div className="text-right hidden sm:block">
              <p className="text-sm font-semibold leading-tight">
                {session?.nombres || session?.usuario}
              </p>
              <Badge className="bg-primary text-white text-[10px]" data-testid="admin-rol-badge">
                {session?.rol}
              </Badge>
            </div>
            <Button
              variant="ghost"
              size="icon"
              data-testid="admin-logout"
              aria-label="Cerrar sesión"
              onClick={logout}
              className="text-secondary hover:bg-primary/10 hover:text-secondary"
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
          <div className="mb-8 flex min-w-0 items-end justify-between gap-4 border-b border-border/80 pb-5">
            <div className="min-w-0">
              <p className="text-xs font-semibold capitalize tracking-wide text-muted-foreground">
                {activeGroup || "Administración"}
              </p>
              <h1 className="mt-1 truncate font-display text-2xl font-black tracking-tight text-foreground">
                {activeModule?.label || "Panel administrativo"}
              </h1>
            </div>
            <span className="hidden shrink-0 rounded-lg border border-border bg-card px-3 py-1.5 text-xs font-semibold text-muted-foreground sm:inline-flex">
              Vista web
            </span>
          </div>
          <Outlet />
        </main>
      </div>
      <AdminBottomNav />
    </div>
  );
}
