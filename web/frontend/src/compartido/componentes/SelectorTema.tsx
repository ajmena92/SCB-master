import { Laptop, Moon, Sun } from "lucide-react";
import { useTheme } from "next-themes";

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
    <div className="inline-flex items-center rounded-lg border border-border bg-card p-1" role="group" aria-label="Tema visual">
      {OPCIONES.map(({ valor, etiqueta, Icono }) => {
        const activo = opcionActiva.valor === valor;
        return (
          <button
            key={valor}
            type="button"
            title={etiqueta}
            aria-label={etiqueta}
            aria-pressed={activo}
            onClick={() => setTheme(valor)}
            className={`flex h-8 w-8 items-center justify-center rounded-md transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 ${activo ? "bg-primary text-primary-foreground" : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"}`}
          >
            <Icono className="h-4 w-4" aria-hidden="true" />
          </button>
        );
      })}
      <span className="sr-only">Tema actual: {opcionActiva.etiqueta}</span>
      <IconoActivo className="sr-only" aria-hidden="true" />
    </div>
  );
}
