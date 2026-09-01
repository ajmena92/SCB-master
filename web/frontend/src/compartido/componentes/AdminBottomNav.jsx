import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import { MoreHorizontal } from "lucide-react";
import {
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerDescription,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
} from "@/components/ui/drawer";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { obtenerModulosVisibles } from "@/config/adminNavigation";

export default function AdminBottomNav() {
  const { session } = useAutenticacion();
  const location = useLocation();
  const [abierto, setAbierto] = useState(false);
  const visibles = obtenerModulosVisibles(session);
  const tieneMas = visibles.length > 4;
  const directos = tieneMas ? visibles.slice(0, 3) : visibles.slice(0, 4);
  const extras = tieneMas ? visibles.slice(3) : [];
  const extraActivo = extras.some(
    (item) => location.pathname === item.path || location.pathname.startsWith(`${item.path}/`),
  );
  const columnas = directos.length + (tieneMas ? 1 : 0);

  if (!columnas) return null;
  return (
    <nav
      className="fixed inset-x-0 bottom-0 z-40 border-t border-secondary-foreground/10 bg-secondary px-2 pb-[max(0.5rem,env(safe-area-inset-bottom))] pt-2 text-secondary-foreground shadow-[0_-8px_24px_rgb(24_32_82_/_0.14)] lg:hidden"
      aria-label="Navegación rápida"
    >
      <div
        className="mx-auto grid max-w-lg gap-1"
        style={{ gridTemplateColumns: `repeat(${columnas}, minmax(0, 1fr))` }}
      >
        {directos.map(({ id, shortLabel, label, path, icon: Icon }) => (
          <NavLink
            key={id}
            to={path}
            data-testid={`admin-bottom-${id}`}
            className={({ isActive }) =>
              `flex min-h-12 min-w-0 flex-col items-center justify-center gap-1 rounded-xl px-1 text-[10px] font-bold transition-colors focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary ${isActive ? "bg-primary text-primary-foreground" : "text-secondary-foreground/75 hover:bg-secondary-foreground/10"}`
            }
          >
            <Icon className="h-5 w-5" aria-hidden="true" />
            <span className="w-full truncate text-center">{shortLabel || label}</span>
          </NavLink>
        ))}
        {tieneMas && (
          <Drawer open={abierto} onOpenChange={setAbierto}>
            <DrawerTrigger asChild>
              <button
                type="button"
                data-testid="admin-bottom-more"
                aria-label="Abrir más módulos"
                className={`flex min-h-12 min-w-0 flex-col items-center justify-center gap-1 rounded-xl px-1 text-[10px] font-bold focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary ${extraActivo ? "bg-primary text-primary-foreground" : "text-secondary-foreground/75 hover:bg-secondary-foreground/10"}`}
              >
                <MoreHorizontal className="h-5 w-5" aria-hidden="true" />
                <span>Más</span>
              </button>
            </DrawerTrigger>
            <DrawerContent className="max-h-[82dvh] bg-background pb-[env(safe-area-inset-bottom)]">
              <DrawerHeader className="text-left">
                <DrawerTitle>Más módulos</DrawerTitle>
                <DrawerDescription>Administración y funciones adicionales.</DrawerDescription>
              </DrawerHeader>
              <div className="grid min-h-0 gap-2 overflow-y-auto px-4 pb-6">
                {extras.map((item) => {
                  const Icon = item.icon;
                  return (
                    <DrawerClose asChild key={item.id}>
                      <NavLink
                        to={item.path}
                        className={({ isActive }) =>
                          `flex min-h-12 items-center gap-3 rounded-xl border px-4 text-sm font-semibold ${isActive ? "border-primary bg-primary/10 text-foreground" : "border-border bg-card text-foreground"}`
                        }
                      >
                        <Icon className="h-5 w-5 shrink-0 text-primary" aria-hidden="true" />
                        <span>{item.label}</span>
                      </NavLink>
                    </DrawerClose>
                  );
                })}
              </div>
            </DrawerContent>
          </Drawer>
        )}
      </div>
    </nav>
  );
}
