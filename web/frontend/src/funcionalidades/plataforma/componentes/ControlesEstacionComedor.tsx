import { Expand, History, LogOut, Volume2, VolumeX } from "lucide-react";

export function ControlesEstacionComedor({
  fecha,
  estadoCamara,
  silenciado,
  mostrarHistorial,
  alAlternarSonido,
  alAlternarHistorial,
  alAlternarPantallaCompleta,
  alSalir,
}: {
  fecha: string;
  estadoCamara: "iniciando" | "activo" | "error";
  silenciado: boolean;
  mostrarHistorial: boolean;
  alAlternarSonido: () => void;
  alAlternarHistorial: () => void;
  alAlternarPantallaCompleta: () => void;
  alSalir: () => void;
}) {
  const colorCamara =
    estadoCamara === "activo"
      ? "bg-emerald-400"
      : estadoCamara === "error"
        ? "bg-rose-400"
        : "bg-amber-300";
  return (
    <header className="flex min-h-14 items-center justify-between gap-3 border-b border-white/10 px-3 sm:px-5">
      <div className="flex min-w-0 items-center gap-3">
        <span className={`h-2.5 w-2.5 rounded-full ${colorCamara}`} />
        <p className="truncate text-sm font-bold tracking-wide">Estación de comedor</p>
        <span className="hidden text-xs text-slate-400 sm:inline">{fecha}</span>
      </div>
      <div className="flex items-center gap-1">
        <button
          type="button"
          onClick={alAlternarSonido}
          className="grid h-11 w-11 place-items-center rounded-xl text-slate-300 hover:bg-white/10"
          aria-label={silenciado ? "Activar sonido" : "Silenciar sonido"}
        >
          {silenciado ? <VolumeX className="h-5 w-5" /> : <Volume2 className="h-5 w-5" />}
        </button>
        <button
          type="button"
          onClick={alAlternarHistorial}
          className="hidden min-h-11 items-center gap-2 rounded-xl px-3 text-sm font-bold text-slate-300 hover:bg-white/10 sm:inline-flex"
          aria-expanded={mostrarHistorial}
        >
          <History className="h-4 w-4" /> Historial <kbd className="text-xs text-slate-500">F4</kbd>
        </button>
        <button
          type="button"
          onClick={alAlternarPantallaCompleta}
          className="grid h-11 w-11 place-items-center rounded-xl text-slate-300 hover:bg-white/10"
          aria-label="Alternar pantalla completa"
        >
          <Expand className="h-5 w-5" />
        </button>
        <button
          type="button"
          onClick={alSalir}
          className="grid h-11 w-11 place-items-center rounded-xl text-slate-300 hover:bg-white/10"
          aria-label="Salir de estación"
        >
          <LogOut className="h-5 w-5" />
        </button>
      </div>
    </header>
  );
}
