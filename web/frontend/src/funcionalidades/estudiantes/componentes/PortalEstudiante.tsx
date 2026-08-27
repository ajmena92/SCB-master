import { AlertTriangle } from "lucide-react";
import { Skeleton } from "@/components/ui/skeleton";
import { NavegacionEstudiante } from "@/funcionalidades/estudiantes/componentes/NavegacionEstudiante";
import { CabeceraPortalEstudiante } from "@/funcionalidades/estudiantes/componentes/CabeceraPortalEstudiante";
import { VistaCarnetEstudiante } from "@/funcionalidades/estudiantes/componentes/VistaCarnetEstudiante";
import { VistaMenuEstudiante } from "@/funcionalidades/estudiantes/componentes/VistaMenuEstudiante";
import type { EstadoPortal } from "@/funcionalidades/estudiantes/estado/usePortalEstudiante";

export function PortalEstudiante({
  nombre,
  sesion,
  alCerrarSesion,
  estadoPortal,
}: {
  nombre: string;
  sesion: { usuario?: Record<string, unknown> } | null;
  alCerrarSesion: () => void;
  estadoPortal: EstadoPortal;
}) {
  const {
    vistaActiva,
    setVistaActiva,
    estado,
    cargando,
    error,
    cerrado,
    abierto,
    asistenciaConfirmada,
    rechazada,
    servicioDisponible,
    cuentaRegresiva,
    horaServidor,
    cierreProximo,
    vistaAsistencia,
    tarjetaConfirmacion,
    ejecutando,
    registrarAsistencia,
    carnet,
    menu,
  } = estadoPortal;

  return (
    <div className="min-h-screen bg-background">
      <CabeceraPortalEstudiante alCerrarSesion={alCerrarSesion} />
      <main className="mx-auto max-w-2xl px-5 pb-32 pt-8 sm:pb-28">
        <div className="animate-fade-up">
          <p className="text-xs font-bold uppercase tracking-[0.2em] text-primary">
            Hola, {nombre.split(" ")[0]}
          </p>
          <h1 className="mt-1 font-display text-3xl font-bold tracking-tight">
            {vistaActiva === "carnet" ? "Mi carnet digital" : "Menú de hoy"}
          </h1>
          {estado && (
            <div className="mt-1 text-sm text-muted-foreground">
              <span>{estado.descripcionHorario}</span>
              {!cerrado && !asistenciaConfirmada && horaServidor && (
                <span>
                  {" "}
                  · Hora servidor <span data-testid="server-clock">{horaServidor}</span>
                </span>
              )}
            </div>
          )}
        </div>

        {cargando && (
          <div className="space-y-4">
            <Skeleton className="h-40 w-full rounded-2xl" />
            <Skeleton className="h-28 w-full rounded-2xl" />
          </div>
        )}
        {!cargando && error && (
          <div
            data-testid="student-error"
            className="flex items-center gap-3 rounded-2xl border border-destructive/30 bg-destructive/10 p-6"
          >
            <AlertTriangle className="h-6 w-6 shrink-0 text-destructive" />
            <p className="text-sm font-medium text-destructive">{error}</p>
          </div>
        )}
        {!cargando && !error && vistaActiva === "carnet" && (
          <VistaCarnetEstudiante sesion={sesion} carnet={carnet} />
        )}
        {!cargando && !error && vistaActiva === "menu" && (
          <VistaMenuEstudiante
            menu={menu}
            estado={estado ?? {}}
            vistaAsistencia={vistaAsistencia}
            abierto={abierto}
            asistenciaConfirmada={asistenciaConfirmada}
            rechazada={rechazada}
            servicioDisponible={servicioDisponible}
            cuentaRegresiva={cuentaRegresiva}
            cierreProximo={cierreProximo}
            ejecutando={ejecutando}
            tarjetaConfirmacion={tarjetaConfirmacion}
            registrarAsistencia={registrarAsistencia}
          />
        )}
      </main>
      <NavegacionEstudiante vistaActiva={vistaActiva} alCambiar={setVistaActiva} />
    </div>
  );
}
