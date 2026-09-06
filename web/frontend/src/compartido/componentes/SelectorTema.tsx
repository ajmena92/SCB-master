import { Laptop, Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";

import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuRadioGroup,
  DropdownMenuRadioItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Tooltip, TooltipContent, TooltipProvider, TooltipTrigger } from "@/components/ui/tooltip";

const OPCIONES = [
  { valor: "light", etiqueta: "Tema claro", Icono: Sun },
  { valor: "dark", etiqueta: "Tema oscuro", Icono: Moon },
  { valor: "system", etiqueta: "Usar tema del dispositivo", Icono: Laptop },
] as const;

export function SelectorTema() {
  const { theme, setTheme } = useTheme();
  const opcionActiva = OPCIONES.find((opcion) => opcion.valor === theme) ?? OPCIONES[2];
  const IconoActivo = opcionActiva.Icono;

  return (
    <TooltipProvider delayDuration={300}>
      <Tooltip>
        <DropdownMenu>
          <TooltipTrigger asChild>
            <DropdownMenuTrigger asChild>
              <Button
                type="button"
                variant="ghost"
                size="icon"
                aria-label={`Tema visual: ${opcionActiva.etiqueta}`}
              >
                <IconoActivo className="h-4 w-4" aria-hidden="true" />
              </Button>
            </DropdownMenuTrigger>
          </TooltipTrigger>
          <TooltipContent side="bottom">{`Tema visual: ${opcionActiva.etiqueta}`}</TooltipContent>
          <DropdownMenuContent align="end" className="min-w-0">
            <DropdownMenuRadioGroup
              value={opcionActiva.valor}
              onValueChange={setTheme}
              aria-label="Seleccionar tema visual"
              className="flex gap-1"
            >
              {OPCIONES.map(({ valor, etiqueta, Icono }) => (
                <DropdownMenuRadioItem
                  key={valor}
                  value={valor}
                  aria-label={etiqueta}
                  title={etiqueta}
                  className="h-11 w-11 justify-center p-0 [&>span]:left-1"
                >
                  <Icono aria-hidden="true" />
                </DropdownMenuRadioItem>
              ))}
            </DropdownMenuRadioGroup>
          </DropdownMenuContent>
        </DropdownMenu>
      </Tooltip>
    </TooltipProvider>
  );
}
