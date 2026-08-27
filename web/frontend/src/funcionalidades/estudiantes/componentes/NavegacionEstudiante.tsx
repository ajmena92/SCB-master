import { IdCard, Utensils } from "lucide-react";

const items = [
  { id: "menu", label: "Menú", Icon: Utensils },
  { id: "carnet", label: "Carnet", Icon: IdCard },
] as const;

export function NavegacionEstudiante({
  vistaActiva,
  alCambiar,
}: {
  vistaActiva: "menu" | "carnet";
  alCambiar: (vista: "menu" | "carnet") => void;
}) {
  return (
    <nav
      aria-label="Navegación estudiantil"
      className="fixed inset-x-0 bottom-0 z-30 border-t border-white/60 bg-white/90 px-4 pb-[calc(env(safe-area-inset-bottom)+0.5rem)] pt-2 shadow-[0_-12px_35px_rgb(var(--brand-primary)_/_0.12)] backdrop-blur-xl sm:bottom-5 sm:left-1/2 sm:right-auto sm:w-[min(22rem,calc(100%-2rem))] sm:-translate-x-1/2 sm:rounded-[1.75rem] sm:border sm:border-white/80 sm:p-2"
    >
      <div className="mx-auto grid max-w-sm grid-cols-2 gap-1">
        {items.map(({ id, label, Icon }) => {
          const active = vistaActiva === id;
          return (
            <button
              key={id}
              type="button"
              aria-current={active ? "page" : undefined}
              aria-label={`Ver ${label}`}
              onClick={() => alCambiar(id)}
              className={`flex min-h-14 flex-col items-center justify-center gap-1 rounded-2xl px-4 text-xs font-bold transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary ${active ? "bg-primary text-primary-foreground shadow-[0_8px_20px_rgb(var(--brand-primary)_/_0.28)]" : "text-secondary/65 hover:bg-primary/10 hover:text-secondary"}`}
            >
              <Icon className="h-5 w-5" aria-hidden="true" />
              <span>{label}</span>
            </button>
          );
        })}
      </div>
    </nav>
  );
}
