import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { KeyRound, LogOut, UtensilsCrossed } from "lucide-react";
import { SelectorTema } from "@/compartido/componentes/SelectorTema";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

export function CabeceraPortalEstudiante({ alCerrarSesion }: { alCerrarSesion: () => void }) {
  const navegar = useNavigate();

  return (
    <header className="sticky top-0 z-20 border-b bg-background/80 backdrop-blur-xl">
      <div className="mx-auto flex h-16 max-w-2xl items-center justify-between px-5">
        <div className="flex items-center gap-2 text-foreground">
          <UtensilsCrossed className="h-6 w-6 text-primary" aria-hidden="true" />
          <span className="font-heading font-bold tracking-tight">Comedor SCSC</span>
        </div>
        <div className="flex items-center gap-1">
          <SelectorTema />
          <TooltipProvider delayDuration={300}>
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  data-testid="open-change-pin"
                  aria-label="Cambiar PIN"
                  onClick={() => navegar("/cambiar-pin")}
                >
                  <KeyRound className="h-4 w-4" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Cambiar PIN</TooltipContent>
            </Tooltip>
            <Tooltip>
              <TooltipTrigger asChild>
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label="Cerrar sesión"
                  data-testid="student-logout"
                  onClick={alCerrarSesion}
                >
                  <LogOut className="h-4 w-4" />
                </Button>
              </TooltipTrigger>
              <TooltipContent>Cerrar sesión</TooltipContent>
            </Tooltip>
          </TooltipProvider>
        </div>
      </div>
    </header>
  );
}
