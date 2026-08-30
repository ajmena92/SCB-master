import { useMarcacionComedor } from "@/funcionalidades/comedor/hooks/useMarcacionComedor";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Maximize2, ScanLine, UtensilsCrossed, Wifi, WifiOff } from "lucide-react";
import { useEffect, useState } from "react";

export default function Comedor() {
  const {
    codigoBarras,
    fecha,
    horarios,
    configuracion,
    guardando,
    ultimoIngreso,
    errorOperacion,
    totalIngresos,
    inputRef,
    setCodigoBarras,
    setFecha,
    registrar,
    modoManual,
    setModoManual,
    altoContraste,
    setAltoContraste,
    historial,
    pequeno,
    horaServidor,
    conexionDisponible,
    recargarHistorial,
  } = useMarcacionComedor();
  const [pantallaCompleta, setPantallaCompleta] = useState(false);
  const [mostrarHistorial, setMostrarHistorial] = useState(false);

  useEffect(() => {
    inputRef.current?.focus();
  }, [inputRef]);

  useEffect(() => {
    function teclas(event: KeyboardEvent) {
      if (event.key === "F2") {
        event.preventDefault();
        setCodigoBarras("");
      }
      if (event.key === "F3") {
        event.preventDefault();
        inputRef.current?.focus();
      }
      if (event.key === "F4") {
        event.preventDefault();
        setMostrarHistorial((actual) => !actual);
        void recargarHistorial();
      }
      if (event.key === "F7") {
        event.preventDefault();
        setAltoContraste((actual) => !actual);
      }
    }
    window.addEventListener("keydown", teclas);
    return () => window.removeEventListener("keydown", teclas);
  }, [inputRef, recargarHistorial, setAltoContraste, setCodigoBarras]);

  async function alternarPantallaCompleta() {
    if (!document.fullscreenElement) {
      await document.documentElement.requestFullscreen?.();
      setPantallaCompleta(true);
    } else {
      await document.exitFullscreen?.();
      setPantallaCompleta(false);
    }
  }

  return (
    <section
      className={`-mx-4 -my-6 flex min-h-[100dvh] flex-col text-white sm:-mx-6 sm:-my-8 lg:-mx-8 xl:-mx-10 ${altoContraste ? "bg-black" : "bg-slate-950"}`}
      data-testid="operacion-comedor"
    >
      <header className="flex flex-wrap items-center justify-between gap-4 border-b border-white/10 px-5 py-4 sm:px-8">
        <div className="flex items-center gap-3">
          <span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-emerald-400/15 text-emerald-300">
            <UtensilsCrossed />
          </span>
          <div>
            <h2 className="font-display text-xl font-black sm:text-2xl">Ingreso al comedor</h2>
            <p className="text-sm text-slate-400">Lectura operativa de carnets</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="rounded-full border border-white/10 px-3 py-2 text-sm tabular-nums text-slate-300">
            Servidor {fecha} · {horaServidor}
          </span>
          <span className="flex items-center gap-1 rounded-full border border-white/10 px-3 py-2 text-sm">
            {conexionDisponible ? (
              <Wifi className="h-4 w-4 text-emerald-300" />
            ) : (
              <WifiOff className="h-4 w-4 text-rose-300" />
            )}
            {conexionDisponible ? "Conectado" : "Sin conexión"}
          </span>
          <Button
            variant="outline"
            className="border-white/20 bg-transparent text-white hover:bg-white/10 hover:text-white"
            onClick={() => void alternarPantallaCompleta()}
          >
            <Maximize2 /> {pantallaCompleta ? "Salir" : "Maximizar"}
          </Button>
        </div>
      </header>
      {pequeno && (
        <div
          className="border-b border-amber-300/30 bg-amber-300/10 px-5 py-3 text-center text-amber-100"
          role="alert"
        >
          Maximice la ventana a 1280×720 o superior para habilitar la operación.
        </div>
      )}
      <div className="grid flex-1 gap-6 p-5 sm:p-8 lg:grid-cols-[17rem_1fr]">
        <aside className="space-y-4">
          <div className="rounded-3xl border border-white/10 bg-white/[0.06] p-5">
            <p className="text-xs font-bold uppercase tracking-widest text-slate-400">
              Ingresos de la sesión
            </p>
            <p className="mt-2 text-5xl font-black tabular-nums text-emerald-300">
              {totalIngresos}
            </p>
          </div>
          <div className="rounded-3xl border border-white/10 bg-white/[0.06] p-5">
            <p className="text-xs font-bold uppercase tracking-widest text-slate-400">
              Configuración
            </p>
            <p className="mt-2 text-sm text-slate-300">
              {configuracion.isPending
                ? "Cargando horarios…"
                : configuracion.isError
                  ? "No disponible"
                  : "Comedor habilitado"}
            </p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button
              variant="outline"
              className="border-white/20 bg-transparent text-white"
              onClick={() => setModoManual((actual) => !actual)}
            >
              {modoManual ? "Modo lector" : "Habilitar entrada manual"}
            </Button>
            <Button
              variant="outline"
              className="border-white/20 bg-transparent text-white"
              onClick={() => setAltoContraste((actual) => !actual)}
            >
              {altoContraste ? "Contraste normal" : "Alto contraste"}
            </Button>
          </div>
          <p className="text-sm text-slate-300" data-testid="lector-listo">
            ● Lector listo · F2 limpiar · F3 foco · F4 historial · F7 contraste
          </p>
          <p className="text-sm text-slate-300">
            {horarios.length === 1
              ? `Horario: ${horarios[0].descripcion}`
              : "Horario automático según la persona"}
          </p>
        </aside>
        <main className="flex min-w-0 flex-col justify-center">
          <div className="mx-auto w-full max-w-4xl rounded-[2rem] border border-white/10 bg-white/[0.06] p-5 shadow-2xl sm:p-10">
            <div className="mb-8 text-center">
              <ScanLine className="mx-auto h-12 w-12 text-emerald-300" />
              <h3 className="mt-4 text-2xl font-black sm:text-4xl">Esperando lectura de carnet</h3>
              <p className="mt-2 text-slate-400">
                Coloque el código frente al lector; la API valida la hora, el estado y el tiquete.
              </p>
            </div>
            <Input
              ref={inputRef}
              aria-label="Código de barras"
              value={codigoBarras}
              onChange={(e) => setCodigoBarras(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") void registrar();
              }}
              placeholder={
                modoManual ? "Escriba el código y presione Enter" : "Escanee el código de barras"
              }
              autoComplete="off"
              className="h-16 border-white/15 bg-slate-900 text-center text-xl text-white placeholder:text-slate-500 sm:text-2xl"
            />
            <div className="mt-5 flex flex-wrap justify-center gap-3">
              <Input
                aria-label="Fecha de operación"
                type="date"
                value={fecha}
                onChange={(e) => setFecha(e.target.value)}
                className="h-12 w-auto border-white/15 bg-slate-900 text-white"
              />
              <Button
                className="h-12 rounded-xl bg-emerald-500 px-8 text-slate-950 hover:bg-emerald-400"
                disabled={pequeno || guardando || !codigoBarras}
                onClick={() => void registrar()}
              >
                {guardando ? "Validando…" : "Registrar ingreso"}
              </Button>
            </div>
            {ultimoIngreso && (
              <div
                className="mt-8 rounded-2xl border border-emerald-400/30 bg-emerald-400/10 p-5 text-center"
                role="status"
              >
                <p className="text-sm font-bold uppercase tracking-widest text-emerald-300">
                  {ultimoIngreso.resultado === "tardio"
                    ? "Ingreso tardío permitido"
                    : "Ingreso registrado"}
                </p>
                <p className="mt-2 text-lg">
                  Modalidad:{" "}
                  <strong>{ultimoIngreso.modalidad === "beca" ? "Beca" : "Tiquete"}</strong>
                </p>
                <p className="text-sm text-slate-300">{ultimoIngreso.nombreCompleto}</p>
                {ultimoIngreso.advertencias?.map((advertencia) => (
                  <p key={advertencia} className="mt-1 text-sm text-amber-200">
                    Advertencia: {advertencia}
                  </p>
                ))}
              </div>
            )}
            {errorOperacion && (
              <div
                className="mt-8 rounded-2xl border border-rose-400/40 bg-rose-400/10 p-5 text-center"
                role="alert"
                data-testid={`operacion-error-${errorOperacion.codigo}`}
              >
                <p className="text-sm font-bold uppercase tracking-widest text-rose-200">
                  {errorOperacion.codigo.replaceAll("_", " ")}
                </p>
                <p className="mt-2 text-lg text-rose-50">{errorOperacion.mensaje}</p>
              </div>
            )}
            {mostrarHistorial && (
              <div
                className="mt-6 rounded-2xl border border-white/10 bg-black/20 p-4"
                aria-label="Historial de ingresos"
              >
                <h4 className="font-bold">Últimos ingresos</h4>
                {historial.length === 0 ? (
                  <p className="mt-2 text-sm text-slate-400">No hay ingresos cargados.</p>
                ) : (
                  historial.map((ingreso) => (
                    <p key={ingreso.idIngreso} className="mt-2 flex justify-between gap-3 text-sm">
                      <span>{ingreso.nombreCompleto}</span>
                      <span>{ingreso.horaMarca ?? ingreso.resultado}</span>
                    </p>
                  ))
                )}
              </div>
            )}
          </div>
        </main>
      </div>
    </section>
  );
}
