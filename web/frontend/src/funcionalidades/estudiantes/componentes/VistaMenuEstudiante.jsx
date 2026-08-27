import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  AlertTriangle,
  Bell,
  CheckCircle2,
  Clock,
  Coffee,
  Cookie,
  Lock,
  Salad,
  Soup,
  XCircle,
} from "lucide-react";

const ICONOS_COMPONENTE = {
  Principal: Soup,
  Acompañamiento: Salad,
  Bebida: Coffee,
  Postre: Cookie,
};

export function VistaMenuEstudiante({
  menu,
  estado,
  vistaAsistencia,
  abierto,
  asistenciaConfirmada,
  rechazada,
  servicioDisponible,
  cuentaRegresiva,
  cierreProximo,
  ejecutando,
  tarjetaConfirmacion,
  registrarAsistencia,
}) {
  return (
    <>
      {abierto && !asistenciaConfirmada && cuentaRegresiva && (
        <section
          data-testid="countdown-card"
          className={`animate-fade-up rounded-2xl border p-6 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] ${cierreProximo ? "border-primary/40 bg-primary/10" : "bg-card"}`}
          aria-label="Tiempo restante para confirmar el almuerzo"
        >
          <div className="flex items-start justify-between gap-4">
            <div>
              <p id="countdown-title" className="text-sm font-semibold text-muted-foreground">
                Tiempo para confirmar
              </p>
              <p
                data-testid="countdown"
                aria-labelledby="countdown-title"
                className={`mt-1 font-display text-4xl font-bold tracking-tight ${cierreProximo ? "text-primary" : "text-secondary"}`}
              >
                {cuentaRegresiva}
              </p>
            </div>
            <div
              className={`rounded-full p-3 ${cierreProximo ? "bg-primary text-primary-foreground" : "bg-accent text-secondary"}`}
              aria-hidden="true"
            >
              {cierreProximo ? <Bell className="h-6 w-6" /> : <Clock className="h-6 w-6" />}
            </div>
          </div>
          <p className="mt-4 text-sm text-muted-foreground">
            Cierre exacto: <span className="font-bold text-foreground">{estado.horaLimite}</span>{" "}
            (hora de Costa Rica).
          </p>
          {cierreProximo && (
            <p
              data-testid="reminder-banner"
              className="mt-3 flex items-center gap-2 text-sm font-semibold text-primary"
            >
              <AlertTriangle className="h-4 w-4 shrink-0" /> Estás en el aviso previo al cierre.
              Confirmá ahora.
            </p>
          )}
        </section>
      )}

      {vistaAsistencia === "confirmed" && (
        <section
          ref={tarjetaConfirmacion}
          tabIndex={-1}
          data-testid="confirmation-card"
          role="status"
          aria-live="polite"
          aria-atomic="true"
          className="animate-fade-up rounded-2xl border border-success/40 bg-success/10 p-6 text-center shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] focus:outline-none focus-visible:ring-2 focus-visible:ring-success"
        >
          <CheckCircle2 className="mx-auto h-14 w-14 text-success" aria-hidden="true" />
          <h2 className="mt-3 font-display text-2xl font-bold">¡Almuerzo confirmado!</h2>
          {estado.fechaHoraConfirmacionServidor && (
            <p className="mt-2 text-sm text-muted-foreground" data-testid="marca-hora-servidor">
              Registrado el {estado.fechaHoraConfirmacionServidor} (hora de Costa Rica)
            </p>
          )}
          {abierto && (
            <p className="mt-3 text-sm text-muted-foreground">
              Podés indicar que no asistirás antes de las {estado.horaLimite}.
            </p>
          )}
        </section>
      )}

      <section
        data-testid="menu-card"
        className="animate-fade-up rounded-2xl border bg-card p-6 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)]"
      >
        {menu ? (
          <>
            <div className="flex items-start justify-between gap-4">
              <h2
                className="font-display text-2xl font-bold tracking-tight"
                data-testid="menu-titulo"
              >
                {menu.Titulo}
              </h2>
              {menu.origen === "sustitucion" && (
                <Badge className="shrink-0 bg-primary">Especial</Badge>
              )}
            </div>
            <ul className="mt-5 grid gap-3 sm:grid-cols-2">
              {menu.Componentes.map((componente) => {
                const Icono = ICONOS_COMPONENTE[componente.TipoComponente] || Soup;
                return (
                  <li
                    key={componente.Orden}
                    className="flex items-center gap-3 rounded-xl bg-accent/40 px-4 py-3"
                  >
                    <Icono className="h-5 w-5 shrink-0 text-secondary" />
                    <div>
                      <p className="text-sm font-semibold">{componente.Nombre}</p>
                      <p className="text-xs text-muted-foreground">{componente.TipoComponente}</p>
                    </div>
                  </li>
                );
              })}
            </ul>
            {menu.Observaciones && (
              <p className="mt-5 border-t pt-4 text-sm text-muted-foreground">
                <span className="font-semibold text-foreground">Observaciones: </span>
                {menu.Observaciones}
              </p>
            )}
          </>
        ) : (
          <div data-testid="menu-empty" className="py-6 text-center">
            <Soup className="mx-auto h-10 w-10 text-muted-foreground/50" />
            <p className="mt-3 font-semibold">No hay menú publicado para hoy</p>
            <p className="text-sm text-muted-foreground">Consultá con el personal del comedor.</p>
          </div>
        )}
      </section>

      <section
        data-testid="attendance-card"
        className="animate-fade-up rounded-2xl border bg-card p-6 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)]"
      >
        {vistaAsistencia === "expired-confirmed" || vistaAsistencia === "expired-unconfirmed" ? (
          <div
            className="py-2 text-center"
            data-testid="periodo-cerrado"
            role="status"
            aria-live="polite"
          >
            <Lock className="mx-auto h-8 w-8 text-muted-foreground" />
            <p className="mt-3 font-display text-lg font-bold">Registro de comedor cerrado</p>
            <div className="mt-4">
              <Badge
                variant={vistaAsistencia === "expired-confirmed" ? "default" : "secondary"}
                data-testid="final-attendance-status"
              >
                {vistaAsistencia === "expired-confirmed"
                  ? "Marcó asistencia al comedor"
                  : "No marcó asistencia al comedor"}
              </Badge>
            </div>
          </div>
        ) : vistaAsistencia === "pending" ? (
          <div className="py-2 text-center">
            <Clock className="mx-auto h-10 w-10 text-primary" />
            <p className="mt-3 font-display text-xl font-bold">
              Aún no inicia el periodo de confirmación
            </p>
            <p className="text-sm text-muted-foreground">
              Podrás marcar el servicio de comedor desde las {estado.horaInicio} (hora de Costa
              Rica).
            </p>
          </div>
        ) : (
          <div>
            <div className="mb-4 flex items-center gap-2 text-sm text-muted-foreground">
              <Clock className="h-4 w-4" /> ¿Asistirás hoy al comedor?
              {rechazada && (
                <Badge variant="secondary" className="ml-auto" data-testid="estado-rechazado">
                  Marcado: No asistiré
                </Badge>
              )}
            </div>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <Button
                data-testid="confirm-btn"
                disabled={ejecutando || asistenciaConfirmada || !servicioDisponible}
                onClick={() => registrarAsistencia("confirm")}
                className="h-14 rounded-full text-base font-bold transition-transform hover:-translate-y-0.5"
              >
                <CheckCircle2 className="mr-2 h-5 w-5" /> Confirmar almuerzo
              </Button>
              <Button
                variant={asistenciaConfirmada ? "destructive" : "outline"}
                data-testid="decline-btn"
                disabled={ejecutando || rechazada || !servicioDisponible}
                onClick={() => registrarAsistencia("decline")}
                className="h-14 rounded-full text-base font-bold"
              >
                <XCircle className="mr-2 h-5 w-5" /> No asistiré
              </Button>
            </div>
            {!servicioDisponible && (
              <p
                className="mt-3 text-sm text-muted-foreground"
                data-testid="attendance-disabled-no-menu"
              >
                La confirmación se habilitará cuando el menú de hoy sea publicado.
              </p>
            )}
          </div>
        )}
      </section>
    </>
  );
}
