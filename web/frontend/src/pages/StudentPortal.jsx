import { useEffect, useState, useCallback, useRef } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate } from "react-router-dom";
import { api, errMsg } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";
import {
  attendanceViewState,
  formatCountdown,
  formatServerClock,
  isClosingSoon,
  parseServerClock,
  secondsRemainingAt,
  serverClockAt,
} from "@/lib/studentAttendance";
import {
  UtensilsCrossed,
  LogOut,
  KeyRound,
  CheckCircle2,
  XCircle,
  Clock,
  Lock,
  AlertTriangle,
  Soup,
  Coffee,
  Salad,
  Cookie,
  Bell,
} from "lucide-react";
import { StudentCardPreview } from "@/components/StudentCard";
import { StudentBottomNav } from "@/components/StudentBottomNav";

const ICON = { Principal: Soup, Acompañamiento: Salad, Bebida: Coffee, Postre: Cookie };

export default function StudentPortal() {
  const navigate = useNavigate();
  const { session, logout } = useAuth();
  const [menu, setMenu] = useState(null);
  const [activeView, setActiveView] = useState("menu");
  const [estado, setEstado] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [acting, setActing] = useState(false);
  const [timeSync, setTimeSync] = useState(null);
  const [nowMs, setNowMs] = useState(() => Date.now());
  const [pageVisible, setPageVisible] = useState(
    () => typeof document === "undefined" || document.visibilityState !== "hidden",
  );
  const remindedRef = useRef(false);
  // All reloads share one queue. In particular, the reload requested after a
  // confirmation must run *after* any visibility/minute reload that was
  // already in flight, so the resulting attendance state is authoritative.
  const refreshQueueRef = useRef(Promise.resolve());
  const openingRefreshRef = useRef(false);
  const closeRefreshRef = useRef(false);
  const confirmationCardRef = useRef(null);
  const focusConfirmationRef = useRef(false);
  const configuredWarningMinutes = Number(estado?.minutosAvisoPrevio);
  const warningMinutes =
    Number.isFinite(configuredWarningMinutes) && configuredWarningMinutes > 0
      ? configuredWarningMinutes
      : 15;

  const cargar = useCallback(async () => {
    const refresh = async () => {
      setError("");
      const [menuResult, attendanceResult] = await Promise.allSettled([
        api.get("/v1/estudiantes/menu"),
        api.get("/v1/estudiantes/asistencia/hoy"),
      ]);

      // A temporary menu outage must not discard an attendance response. Keep
      // the last known menu until the menu endpoint itself returns again.
      if (menuResult.status === "fulfilled") {
        setMenu(menuResult.value.data.menu);
      }

      if (attendanceResult.status === "fulfilled") {
        const nextEstado = attendanceResult.value.data;
        const syncedAtMs = Date.now();
        setEstado(nextEstado);
        setTimeSync({
          segundosParaApertura: Number.isFinite(nextEstado.segundosParaApertura)
            ? Math.max(0, nextEstado.segundosParaApertura)
            : null,
          segundosParaCierre: Number.isFinite(nextEstado.segundosParaCierre)
            ? Math.max(0, nextEstado.segundosParaCierre)
            : null,
          horaServidorSegundos: parseServerClock(nextEstado.horaServidor),
          syncedAtMs,
        });
        setNowMs(syncedAtMs);
      } else {
        setError(errMsg(attendanceResult.reason));
      }

      setLoading(false);
    };

    const scheduledRefresh = refreshQueueRef.current.then(refresh, refresh);
    // Keep the queue usable after an unexpected rendering/data error, while
    // returning the actual refresh promise to action callers that must await it.
    refreshQueueRef.current = scheduledRefresh.catch(() => undefined);
    return scheduledRefresh;
  }, []);

  const {
    data: cardData = null,
    error: cardQueryError,
    isPending: cardLoading,
    refetch: cargarCarnet,
  } = useQuery({
    queryKey: ["student", "carnet"],
    queryFn: async () => (await api.get("/v1/estudiantes/carnet")).data,
    enabled: activeView === "carnet",
    retry: false,
  });
  const cardError = cardQueryError ? errMsg(cardQueryError) : "";

  useEffect(() => {
    let activo = true;
    Promise.allSettled([api.get("/v1/estudiantes/menu"), api.get("/v1/estudiantes/asistencia/hoy")]).then(
      ([menuResult, attendanceResult]) => {
        if (!activo) return;
        if (menuResult.status === "fulfilled") {
          setMenu(menuResult.value.data.menu);
        }
        if (attendanceResult.status === "fulfilled") {
          const nextEstado = attendanceResult.value.data;
          const syncedAtMs = Date.now();
          setEstado(nextEstado);
          setTimeSync({
            segundosParaApertura: Number.isFinite(nextEstado.segundosParaApertura)
              ? Math.max(0, nextEstado.segundosParaApertura)
              : null,
            segundosParaCierre: Number.isFinite(nextEstado.segundosParaCierre)
              ? Math.max(0, nextEstado.segundosParaCierre)
              : null,
            horaServidorSegundos: parseServerClock(nextEstado.horaServidor),
            syncedAtMs,
          });
          setNowMs(syncedAtMs);
        } else {
          setError(errMsg(attendanceResult.reason));
        }
        setLoading(false);
      },
    );
    return () => {
      activo = false;
    };
  }, []);

  const secsLeft = secondsRemainingAt(timeSync?.segundosParaCierre, timeSync?.syncedAtMs, nowMs);
  const secsUntilOpening = secondsRemainingAt(
    timeSync?.segundosParaApertura,
    timeSync?.syncedAtMs,
    nowMs,
  );
  const serverClockSeconds = serverClockAt(
    timeSync?.horaServidorSegundos,
    timeSync?.syncedAtMs,
    nowMs,
  );
  const locallyClosed = Boolean(estado?.periodoAbierto) && secsLeft === 0;
  const cerrado = Boolean(estado?.periodoCerrado) || locallyClosed;
  const abierto = Boolean(estado?.periodoAbierto) && !cerrado;
  const confirmadaTick = estado?.estado === "Confirmada" || estado?.estado === "Corregida";
  // Keep the local countdown alive after confirming as well: the student must
  // still be able to cancel until the server-authoritative closing time.
  // The server clock remains hidden after confirmation, but the timer still
  // reconciles the final transition to the closed state.
  const shouldTick =
    Boolean(estado) && !cerrado && Number.isFinite(timeSync?.horaServidorSegundos) && pageVisible;

  useEffect(() => {
    if (!shouldTick) return undefined;

    const t = setInterval(() => setNowMs(Date.now()), 1_000);
    return () => clearInterval(t);
  }, [shouldTick]);

  useEffect(() => {
    // Reconcile the browser display with SQL Server regularly. This also
    // transitions at opening/closing even if the tab remains open all day.
    const t = setInterval(cargar, 60_000);
    return () => clearInterval(t);
  }, [cargar]);

  useEffect(() => {
    const reconcileVisibility = () => {
      const visible = document.visibilityState !== "hidden";
      setPageVisible(visible);
      if (visible) {
        // A hidden tab can throttle timers. Calculate from the synchronized
        // base immediately, then ask SQL Server to reconcile its authority.
        setNowMs(Date.now());
        cargar();
      }
    };

    document.addEventListener("visibilitychange", reconcileVisibility);
    return () => document.removeEventListener("visibilitychange", reconcileVisibility);
  }, [cargar]);

  useEffect(() => {
    if (secsUntilOpening > 0) openingRefreshRef.current = false;
    if (
      !estado?.periodoAbierto &&
      !estado?.periodoCerrado &&
      secsUntilOpening === 0 &&
      !openingRefreshRef.current
    ) {
      // Opening is always confirmed by SQL Server; reaching zero locally never
      // enables actions by itself.
      openingRefreshRef.current = true;
      cargar();
    }
  }, [secsUntilOpening, estado, cargar]);

  useEffect(() => {
    if (secsLeft > 0) closeRefreshRef.current = false;
    if (
      estado?.periodoAbierto &&
      !estado?.periodoCerrado &&
      secsLeft === 0 &&
      !closeRefreshRef.current
    ) {
      closeRefreshRef.current = true;
      cargar();
    }
  }, [secsLeft, estado, cargar]);

  const estadoParaVista =
    cerrado === Boolean(estado?.periodoCerrado) ? estado : { ...estado, periodoCerrado: cerrado };
  const vistaAsistencia = attendanceViewState(estadoParaVista);
  const rechazada = estado?.estado === "Cancelada";
  const servicioDisponible = Boolean(menu);
  const countdown = formatCountdown(secsLeft);
  const horaServidor = formatServerClock(serverClockSeconds) || estado?.horaServidor;
  const closingSoon = isClosingSoon(secsLeft, warningMinutes);
  useEffect(() => {
    if (closingSoon && !confirmadaTick && !cerrado && !remindedRef.current) {
      remindedRef.current = true;
      toast.warning(
        `Faltan menos de ${warningMinutes} minutos para el cierre. ¡No olvidés confirmar tu asistencia!`,
      );
    }
  }, [cerrado, closingSoon, confirmadaTick, warningMinutes]);

  useEffect(() => {
    if (confirmadaTick && focusConfirmationRef.current) {
      confirmationCardRef.current?.focus();
      focusConfirmationRef.current = false;
    }
  }, [confirmadaTick]);

  const accion = async (tipo) => {
    setActing(true);
    try {
      await api.post(`/v1/estudiantes/asistencia/${tipo}`);
      if (tipo === "confirm") focusConfirmationRef.current = true;
      toast.success(
        tipo === "confirm" ? "¡Asistencia confirmada!" : "Registrado: no asistirás hoy",
      );
      await cargar();
    } catch (err) {
      toast.error(errMsg(err));
    } finally {
      setActing(false);
    }
  };

  const nombre = session?.usuario?.Nombre || session?.usuario?.nombreCompleto || session?.usuario?.nombre || "";

  return (
    <div className="min-h-screen bg-background">
      <header className="sticky top-0 z-20 backdrop-blur-xl bg-background/80 border-b">
        <div className="max-w-2xl mx-auto px-5 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2 text-secondary">
            <UtensilsCrossed className="h-6 w-6" />
            <span className="font-display font-black tracking-tight">Comedor SCSC</span>
          </div>
          <div className="flex items-center gap-1">
            <Button
              variant="ghost"
              size="sm"
              data-testid="open-change-pin"
              onClick={() => navigate("/cambiar-pin")}
            >
              <KeyRound className="h-4 w-4 mr-1" /> PIN
            </Button>
            <Button
              variant="ghost"
              size="sm"
              aria-label="Cerrar sesión"
              data-testid="student-logout"
              onClick={logout}
            >
              <LogOut className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-2xl px-5 pb-32 pt-8 sm:pb-28">
        <div className="animate-fade-up">
          <p className="text-xs uppercase tracking-[0.2em] font-bold text-primary">
            Hola, {nombre.split(" ")[0]}
          </p>
          <h1 className="font-display text-3xl font-bold tracking-tight mt-1">
            {activeView === "carnet" ? "Mi carnet digital" : "Menú de hoy"}
          </h1>
          {estado && (
            <div className="text-sm text-muted-foreground mt-1">
              <span>{estado.descripcionHorario}</span>
              {!cerrado && !confirmadaTick && horaServidor && (
                <span>
                  {" "}
                  · Hora servidor <span data-testid="server-clock">{horaServidor}</span>
                </span>
              )}
            </div>
          )}
        </div>

        {loading && (
          <div className="space-y-4">
            <Skeleton className="h-40 w-full rounded-2xl" />
            <Skeleton className="h-28 w-full rounded-2xl" />
          </div>
        )}

        {!loading && error && (
          <div
            data-testid="student-error"
            className="bg-destructive/10 border border-destructive/30 rounded-2xl p-6 flex items-center gap-3"
          >
            <AlertTriangle className="h-6 w-6 text-destructive shrink-0" />
            <p className="text-sm font-medium text-destructive">{error}</p>
          </div>
        )}

        {!loading && !error && activeView === "carnet" && (
          <div className="animate-fade-up">
            <StudentCardPreview
              hasPhoto={session?.usuario?.TieneFoto ?? session?.usuario?.tieneFoto}
              cardData={cardData}
              loading={cardLoading}
              error={cardError}
              onRetry={cargarCarnet}
            />
          </div>
        )}

        {!loading && !error && activeView === "menu" && (
          <>
            {abierto && !confirmadaTick && countdown && (
              <section
                data-testid="countdown-card"
                className={`rounded-2xl border p-6 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] animate-fade-up ${closingSoon ? "bg-primary/10 border-primary/40" : "bg-card"}`}
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
                      className={`mt-1 font-display text-4xl font-bold tracking-tight ${closingSoon ? "text-primary" : "text-secondary"}`}
                    >
                      {countdown}
                    </p>
                  </div>
                  <div
                    className={`rounded-full p-3 ${closingSoon ? "bg-primary text-primary-foreground" : "bg-accent text-secondary"}`}
                    aria-hidden="true"
                  >
                    {closingSoon ? <Bell className="h-6 w-6" /> : <Clock className="h-6 w-6" />}
                  </div>
                </div>
                <p className="mt-4 text-sm text-muted-foreground">
                  Cierre exacto:{" "}
                  <span className="font-bold text-foreground">{estado.horaLimite}</span> (hora de
                  Costa Rica).
                </p>
                {closingSoon && (
                  <p
                    data-testid="reminder-banner"
                    className="mt-3 flex items-center gap-2 text-sm font-semibold text-primary"
                  >
                    <AlertTriangle className="h-4 w-4 shrink-0" /> Estás en el aviso previo al
                    cierre. Confirmá ahora.
                  </p>
                )}
              </section>
            )}

            {vistaAsistencia === "confirmed" && (
              <section
                ref={confirmationCardRef}
                tabIndex={-1}
                data-testid="confirmation-card"
                role="status"
                aria-live="polite"
                aria-atomic="true"
                className="rounded-2xl border border-success/40 bg-success/10 p-6 text-center shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] animate-fade-up focus:outline-none focus-visible:ring-2 focus-visible:ring-success"
              >
                <CheckCircle2 className="h-14 w-14 mx-auto text-success" aria-hidden="true" />
                <h2 className="mt-3 font-display text-2xl font-bold">¡Almuerzo confirmado!</h2>
                {estado.fechaHoraConfirmacionServidor && (
                  <p
                    className="mt-2 text-sm text-muted-foreground"
                    data-testid="marca-hora-servidor"
                  >
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
              className="bg-card border rounded-2xl p-6 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] animate-fade-up"
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
                      <Badge className="bg-primary shrink-0">Especial</Badge>
                    )}
                  </div>
                  <ul className="mt-5 grid sm:grid-cols-2 gap-3">
                    {menu.Componentes.map((c) => {
                      const Ic = ICON[c.TipoComponente] || Soup;
                      return (
                        <li
                          key={c.Orden}
                          className="flex items-center gap-3 rounded-xl bg-accent/40 px-4 py-3"
                        >
                          <Ic className="h-5 w-5 text-secondary shrink-0" />
                          <div>
                            <p className="font-semibold text-sm">{c.Nombre}</p>
                            <p className="text-xs text-muted-foreground">{c.TipoComponente}</p>
                          </div>
                        </li>
                      );
                    })}
                  </ul>
                  {menu.Observaciones && (
                    <p className="mt-5 text-sm text-muted-foreground border-t pt-4">
                      <span className="font-semibold text-foreground">Observaciones: </span>
                      {menu.Observaciones}
                    </p>
                  )}
                </>
              ) : (
                <div data-testid="menu-empty" className="text-center py-6">
                  <Soup className="h-10 w-10 mx-auto text-muted-foreground/50" />
                  <p className="mt-3 font-semibold">No hay menú publicado para hoy</p>
                  <p className="text-sm text-muted-foreground">
                    Consultá con el personal del comedor.
                  </p>
                </div>
              )}
            </section>

            <section
              data-testid="attendance-card"
              className="bg-card border rounded-2xl p-6 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] animate-fade-up"
            >
              {vistaAsistencia === "expired-confirmed" ||
              vistaAsistencia === "expired-unconfirmed" ? (
                <div
                  className="text-center py-2"
                  data-testid="periodo-cerrado"
                  role="status"
                  aria-live="polite"
                >
                  <Lock className="h-8 w-8 mx-auto text-muted-foreground" />
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
                <div className="text-center py-2">
                  <Clock className="h-10 w-10 mx-auto text-primary" />
                  <p className="mt-3 font-display text-xl font-bold">
                    Aún no inicia el periodo de confirmación
                  </p>
                  <p className="text-sm text-muted-foreground">
                    Podrás marcar el servicio de comedor desde las {estado.horaInicio} (hora de
                    Costa Rica).
                  </p>
                </div>
              ) : (
                <div>
                  <div className="flex items-center gap-2 text-muted-foreground text-sm mb-4">
                    <Clock className="h-4 w-4" /> ¿Asistirás hoy al comedor?
                    {rechazada && (
                      <Badge variant="secondary" className="ml-auto" data-testid="estado-rechazado">
                        Marcado: No asistiré
                      </Badge>
                    )}
                  </div>
                  <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                    <Button
                      data-testid="confirm-btn"
                      disabled={acting || confirmadaTick || !servicioDisponible}
                      onClick={() => accion("confirm")}
                      className="h-14 rounded-full text-base font-bold transition-transform hover:-translate-y-0.5"
                    >
                      <CheckCircle2 className="h-5 w-5 mr-2" /> Confirmar almuerzo
                    </Button>
                    <Button
                      variant={confirmadaTick ? "destructive" : "outline"}
                      data-testid="decline-btn"
                      disabled={acting || rechazada || !servicioDisponible}
                      onClick={() => accion("decline")}
                      className="h-14 rounded-full text-base font-bold"
                    >
                      <XCircle className="h-5 w-5 mr-2" /> No asistiré
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
        )}
      </main>
      <StudentBottomNav activeView={activeView} onChange={setActiveView} />
    </div>
  );
}
