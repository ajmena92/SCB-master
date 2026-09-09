import type { FormEventHandler, RefObject } from "react";
import { ScanBarcode, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface PropiedadesCapturaManualComedor {
  referenciaEntrada: RefObject<HTMLInputElement | null>;
  pendiente: boolean;
  alRegistrar: FormEventHandler<HTMLFormElement>;
  alOcultar: () => void;
}

/** Captura de respaldo; la página conserva la validación y el control de duplicados. */
export function CapturaManualComedor({
  referenciaEntrada,
  pendiente,
  alRegistrar,
  alOcultar,
}: PropiedadesCapturaManualComedor) {
  return (
    <form
      className="sticky bottom-2 z-20 mx-auto flex w-full max-w-4xl flex-col gap-3 rounded-2xl border border-white/15 bg-slate-900 p-4 shadow-2xl sm:static sm:flex-row"
      onSubmit={alRegistrar}
    >
      <label htmlFor="captura-comedor" className="sr-only">
        Lector USB o ingreso manual
      </label>
      <div className="relative min-w-0 flex-1">
        <ScanBarcode className="absolute left-4 top-1/2 h-5 w-5 -translate-y-1/2 text-emerald-300" />
        <Input
          ref={referenciaEntrada}
          id="captura-comedor"
          name="codigo"
          autoComplete="off"
          required
          placeholder="Lector USB o número de identificación"
          className="h-12 border-white/15 bg-slate-950 pl-12 text-base text-white placeholder:text-slate-300"
        />
      </div>
      <Button
        className="h-12 bg-emerald-400 px-6 font-bold text-slate-950 hover:bg-emerald-300"
        disabled={pendiente}
      >
        {pendiente ? "Validando…" : "Registrar"}
      </Button>
      <button
        type="button"
        onClick={alOcultar}
        className="grid h-12 w-12 place-items-center rounded-xl text-slate-300 hover:bg-white/10"
        aria-label="Ocultar respaldo"
      >
        <X className="h-5 w-5" />
      </button>
    </form>
  );
}
