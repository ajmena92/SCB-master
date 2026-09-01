import { useState } from "react";
import { NavLink } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { ADMIN_NAVIGATION_GROUPS, obtenerModulosVisibles } from "@/config/adminNavigation";
import { ChevronLeft, ChevronRight, PanelLeft } from "lucide-react";

export default function AdminSidebar() {
  const { session } = useAutenticacion();
  const [compacto, setCompacto] = useState(false);
  const modulos = obtenerModulosVisibles(session);

  return (
    <aside
      className={`${compacto ? "w-[4.75rem]" : "w-64"} hidden shrink-0 border-r border-secondary-foreground/10 bg-secondary text-secondary-foreground transition-[width] duration-200 lg:sticky lg:top-16 lg:block lg:h-[calc(100dvh-4rem)] lg:overflow-y-auto`}
      aria-label="Navegación de administración"
      data-testid="admin-sidebar"
      data-compact={compacto ? "true" : "false"}
    >
      <div className="flex min-h-full flex-col px-4 pb-6 pt-5">
        <div
          className={`mb-5 flex items-center ${compacto ? "justify-center" : "justify-between px-2"}`}
        >
          {!compacto && (
            <p className="text-xs font-semibold tracking-wide text-secondary-foreground/60">
              Navegación
            </p>
          )}
          <button
            type="button"
            onClick={() => setCompacto((valor) => !valor)}
            className="flex h-10 w-10 items-center justify-center rounded-xl text-secondary-foreground/70 hover:bg-secondary-foreground/10 hover:text-secondary-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary"
            aria-label={compacto ? "Expandir menú" : "Contraer menú"}
          >
            {compacto ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
          </button>
        </div>
        <nav className="space-y-5" aria-label="Módulos administrativos">
          {ADMIN_NAVIGATION_GROUPS.map((group) => {
            const items = modulos.filter((item) => item.group === group.id);
            if (!items.length) return null;
            return (
              <section key={group.id} aria-label={group.label}>
                {!compacto && (
                  <h2 className="px-3 pb-2 text-[11px] font-semibold uppercase tracking-[0.12em] text-secondary-foreground/55">
                    {group.label}
                  </h2>
                )}
                <div className="space-y-1.5">
                  {items.map((item) => {
                    const Icon = item.icon;
                    return (
                      <NavLink
                        key={item.id}
                        to={item.path}
                        data-testid={`admin-sidebar-${item.id}`}
                        title={compacto ? item.label : undefined}
                        className={({ isActive }) =>
                          `group relative flex min-h-12 items-center gap-3 rounded-xl px-3 text-sm font-semibold transition-[background-color,color,transform] duration-200 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary ${isActive ? "bg-secondary-foreground/15 text-secondary-foreground shadow-[inset_0_0_0_1px_rgb(255_255_255_/_0.08)] before:absolute before:-left-4 before:h-7 before:w-1 before:rounded-r-full before:bg-primary" : "text-secondary-foreground/75 hover:translate-x-0.5 hover:bg-secondary-foreground/10 hover:text-secondary-foreground"}`
                        }
                      >
                        {({ isActive }) => (
                          <>
                            <span
                              className={`flex h-8 w-8 shrink-0 items-center justify-center rounded-lg transition-colors ${isActive ? "bg-primary text-primary-foreground" : "bg-secondary-foreground/10 text-secondary-foreground/75 group-hover:bg-secondary-foreground/15 group-hover:text-secondary-foreground"}`}
                            >
                              <Icon className="h-[18px] w-[18px]" aria-hidden="true" />
                            </span>
                            {!compacto && <span className="min-w-0 truncate">{item.label}</span>}
                          </>
                        )}
                      </NavLink>
                    );
                  })}
                </div>
              </section>
            );
          })}
        </nav>

        {!modulos.length && (
          <div className="mt-4 px-2 text-center text-xs text-secondary-foreground/60" role="status">
            <PanelLeft className="mx-auto mb-2 h-5 w-5" aria-hidden="true" />
            Sin módulos disponibles
          </div>
        )}
      </div>
    </aside>
  );
}
