import { NavLink } from "react-router-dom";
import { useAuth } from "@/context/AuthContext";
import { ADMIN_NAVIGATION_GROUPS, getVisibleAdminModules } from "@/config/adminNavigation";

export default function AdminGroupHub({ groupId }) {
  const { session } = useAuth();
  const modules = getVisibleAdminModules(session).filter((item) => item.group === groupId);
  const group = ADMIN_NAVIGATION_GROUPS.find((item) => item.id === groupId);
  return (
    <section>
      <p className="mb-4 text-sm text-muted-foreground">
        Selecciona una función de {group?.label || "esta sección"}.
      </p>
      <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-3">
        {modules.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.id}
              to={item.path}
              className="group rounded-2xl border border-border bg-card p-5 shadow-sm transition hover:-translate-y-0.5 hover:border-primary/50 hover:shadow-md"
            >
              <Icon className="h-6 w-6 text-primary" />
              <h2 className="mt-4 font-bold text-foreground">{item.label}</h2>
              <p className="mt-1 text-sm text-muted-foreground">Abrir módulo</p>
            </NavLink>
          );
        })}
      </div>
    </section>
  );
}
