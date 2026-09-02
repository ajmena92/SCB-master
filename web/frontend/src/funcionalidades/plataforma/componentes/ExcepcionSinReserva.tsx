import { useRef } from "react";
import { CheckCircle2, XCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { errMsg } from "@/compartido/consultas/errores_api";

type Props = {
  codigo: string;
  alCambiarCodigo: (codigo: string) => void;
  alDecidir: (decision: "aprobada" | "rechazada", motivo: string) => void;
  pendiente: boolean;
  error: unknown;
  exito: boolean;
};

export function ExcepcionSinReserva({
  codigo,
  alCambiarCodigo,
  alDecidir,
  pendiente,
  error,
  exito,
}: Props) {
  const formulario = useRef<HTMLFormElement>(null);

  function decidir(decision: "aprobada" | "rechazada") {
    const actual = formulario.current;
    if (!actual?.reportValidity()) return;
    const datos = new FormData(actual);
    alDecidir(decision, String(datos.get("motivo") ?? ""));
  }

  return (
    <aside className="rounded-2xl border border-amber-300 bg-amber-50 p-5 text-amber-950 sm:p-6">
      <p className="text-xs font-black uppercase tracking-[0.16em]">Decisión de operador</p>
      <h2 className="mt-1 font-display text-2xl font-black">Estudiante sin reserva</h2>
      <p className="mt-2 text-sm leading-6 text-amber-950/80">
        Confirmá la decisión y el motivo. Quedará registrada a tu nombre.
      </p>
      <form ref={formulario} className="mt-5 space-y-4">
        <label htmlFor="excepcion-codigo" className="block text-sm font-bold">
          Cédula del estudiante
          <Input
            id="excepcion-codigo"
            name="codigo"
            value={codigo}
            onChange={(evento) => alCambiarCodigo(evento.target.value)}
            className="mt-2 bg-background"
            placeholder="Ej. 1-2091-0218"
            required
          />
        </label>
        <label className="block text-sm font-bold">
          Motivo
          <textarea
            name="motivo"
            className="mt-2 min-h-24 w-full rounded-md border bg-background p-3"
            placeholder="Ej. Autorización de coordinación"
            required
          />
        </label>
        {error && <p className="text-sm font-semibold text-destructive">{errMsg(error)}</p>}
        {exito && <p className="text-sm font-semibold text-success">Decisión guardada.</p>}
        <div className="grid gap-3 sm:grid-cols-2">
          <Button type="button" className="h-12 font-bold" disabled={pendiente} onClick={() => decidir("aprobada")}>
            <CheckCircle2 className="mr-2 h-5 w-5" /> Aprobar ingreso
          </Button>
          <Button type="button" variant="destructive" className="h-12 font-bold" disabled={pendiente} onClick={() => decidir("rechazada")}>
            <XCircle className="mr-2 h-5 w-5" /> Rechazar ingreso
          </Button>
        </div>
      </form>
    </aside>
  );
}
