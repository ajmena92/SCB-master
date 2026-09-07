import { EstadoPanel } from "@/compartido/componentes/Estados";
import { NavegacionEstudiante } from "@/funcionalidades/estudiantes/componentes/NavegacionEstudiante";
import { CabeceraPortalEstudiante } from "@/funcionalidades/estudiantes/componentes/CabeceraPortalEstudiante";
import { VistaCarnetEstudiante } from "@/funcionalidades/estudiantes/componentes/VistaCarnetEstudiante";
import { VistaMenuEstudiante } from "@/funcionalidades/estudiantes/componentes/VistaMenuEstudiante";
import type {
  EstadoPortal,
  TipoPersonaComedor,
} from "@/funcionalidades/estudiantes/estado/usePortalEstudiante";

export function PortalComedor({
  nombre,
  sesion,
  tipoPersona = "estudiante",
  alCerrarSesion,
  estadoPortal,
  alCambiarVista,
}: {
  nombre: string;
  sesion: { usuario?: Record<string, unknown> } | null;
  tipoPersona?: TipoPersonaComedor;
  alCerrarSesion: () => void;
  estadoPortal: EstadoPortal;
  alCambiarVista: (vista: "menu" | "carnet") => void;
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
    minutosAviso,
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
      <main className="mx-auto max-w-2xl px-5 pb-32 pt-8 sm:pb-28 lg:max-w-4xl">
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
                  {" "}· Hora servidor{" "}
                  <span
                    data-testid="server-clock"
                    className="ml-1 inline-flex rounded-md bg-muted px-2 py-0.5 font-display text-sm font-bold tabular-nums text-foreground shadow-sm"
                    aria-label={`Hora del servidor ${horaServidor}`}
                  >
                    {horaServidor}
                  </span>
                </span>
              )}
            </div>
          )}
        </div>

        {cargando && (
          <EstadoPanel variante="carga">Cargando tu información…</EstadoPanel>
        )}
        {!cargando && error && (
          <div data-testid="student-error"><EstadoPanel variante="error">{error}</EstadoPanel></div>
        )}
        {!cargando && !error && vistaActiva === "carnet" && (
          <VistaCarnetEstudiante sesion={sesion} carnet={carnet} tipoPersona={tipoPersona} />
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
            minutosAviso={minutosAviso}
            cierreProximo={cierreProximo}
            ejecutando={ejecutando}
            tarjetaConfirmacion={tarjetaConfirmacion}
            registrarAsistencia={registrarAsistencia}
          />
        )}
      </main>
      <NavegacionEstudiante
        vistaActiva={vistaActiva}
        alCambiar={(vista) => {
          setVistaActiva(vista);
          alCambiarVista(vista);
        }}
      />
    </div>
  );
}
