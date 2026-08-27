import { useState } from "react";
import { NavLink } from "react-router-dom";
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
import { useAuth } from "@/context/AuthContext";
import { ADMIN_NAVIGATION, getVisibleAdminModules } from "@/config/adminNavigation";

const primaryIds = ["dashboard", "menu", "estudiantes", "reporte"];

export default function AdminBottomNav() {
  const { session } = useAuth();
  const [open, setOpen] = useState(false);
  const visible = getVisibleAdminModules(session);
  const primary = primaryIds
    .map((id) => ADMIN_NAVIGATION.find((item) => item.id === id))
    .filter((item) => item && visible.some((module) => module.id === item.id));
  const extras = visible.filter((item) => !primaryIds.includes(item.id));

  return (
    <nav
      className="fixed inset-x-0 bottom-0 z-40 border-t border-secondary-foreground/10 bg-secondary px-2 pb-[max(0.5rem,env(safe-area-inset-bottom))] pt-2 text-secondary-foreground shadow-[0_-8px_24px_rgb(24_32_82_/_0.14)] lg:hidden"
      aria-label="Navegación rápida"
    >
      <div className="mx-auto grid max-w-lg grid-cols-5 gap-1">
        {primary.map(({ id, label, path, icon: Icon }) => (
          <NavLink
            key={id}
            to={path}
            data-testid={`admin-bottom-${id}`}
            className={({ isActive }) =>
              `flex min-h-12 flex-col items-center justify-center gap-1 rounded-xl text-[10px] font-bold transition-colors ${isActive ? "bg-primary text-primary-foreground" : "text-secondary-foreground/75 hover:bg-secondary-foreground/10"}`
            }
          >
            <Icon className="h-5 w-5" aria-hidden="true" />
            <span>{label}</span>
          </NavLink>
        ))}
        <Drawer open={open} onOpenChange={setOpen}>
          <DrawerTrigger asChild>
            <button
              type="button"
              data-testid="admin-bottom-more"
              className="flex min-h-12 flex-col items-center justify-center gap-1 rounded-xl text-[10px] font-bold text-secondary-foreground/75 hover:bg-secondary-foreground/10"
            >
              <MoreHorizontal className="h-5 w-5" aria-hidden="true" />
              <span>Más</span>
            </button>
          </DrawerTrigger>
          <DrawerContent className="bg-background">
            <DrawerHeader>
              <DrawerTitle>Más opciones</DrawerTitle>
              <DrawerDescription>Configuración y control del sistema.</DrawerDescription>
            </DrawerHeader>
            <div className="grid gap-2 px-4 pb-8">
              {extras.map((item) => {
                const Icon = item.icon;
                return (
                  <DrawerClose asChild key={item.id}>
                    <NavLink
                      to={item.path}
                      className="flex min-h-12 items-center gap-3 rounded-xl border border-border bg-card px-4 text-sm font-semibold text-foreground"
                    >
                      <Icon className="h-5 w-5 text-primary" />
                      {item.label}
                    </NavLink>
                  </DrawerClose>
                );
              })}
            </div>
          </DrawerContent>
        </Drawer>
      </div>
    </nav>
  );
}
