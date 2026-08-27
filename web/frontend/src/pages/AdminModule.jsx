import { Suspense } from "react";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import {
  ADMIN_NAVIGATION,
  obtenerModulosVisibles,
  isAdministratorSession,
} from "@/config/adminNavigation";

export default function AdminModule({ moduleId }) {
  const { session } = useAutenticacion();
  const module = ADMIN_NAVIGATION.find((item) => item.id === moduleId);
  if (!module) return null;

  const Component = module.C;
  const esAdmin = isAdministratorSession(session);
  if (!obtenerModulosVisibles(session).some((item) => item.id === moduleId)) {
    return (
      <section
        className="mx-auto max-w-2xl rounded-2xl border border-destructive/30 bg-card p-6 text-center shadow-sm"
        role="alert"
      >
        <h1 className="text-xl font-bold text-foreground">Acceso no autorizado</h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Tu rol no tiene permisos para consultar esta sección.
        </p>
      </section>
    );
  }
  return (
    <Suspense
      fallback={
        <div
          className="h-32 w-full animate-pulse rounded-xl bg-muted"
          aria-label="Cargando sección"
        />
      }
    >
      <Component esAdmin={esAdmin} />
    </Suspense>
  );
}
