import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { KeyRound, LogOut, UtensilsCrossed } from "lucide-react";

export function CabeceraPortalEstudiante({ alCerrarSesion }) {
  const navegar = useNavigate();

  return (
    <header className="sticky top-0 z-20 border-b bg-background/80 backdrop-blur-xl">
      <div className="mx-auto flex h-16 max-w-2xl items-center justify-between px-5">
        <div className="flex items-center gap-2 text-secondary">
          <UtensilsCrossed className="h-6 w-6" />
          <span className="font-display font-black tracking-tight">Comedor SCSC</span>
        </div>
        <div className="flex items-center gap-1">
          <Button
            variant="ghost"
            size="sm"
            data-testid="open-change-pin"
            onClick={() => navegar("/cambiar-pin")}
          >
            <KeyRound className="mr-1 h-4 w-4" /> PIN
          </Button>
          <Button
            variant="ghost"
            size="sm"
            aria-label="Cerrar sesión"
            data-testid="student-logout"
            onClick={alCerrarSesion}
          >
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
      </div>
    </header>
  );
}
