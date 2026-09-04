import { CheckCircle2, XCircle } from "lucide-react";
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { Button } from "@/components/ui/button";
import { errMsg } from "@/compartido/consultas/errores_api";

type Props = {
  codigo: string;
  alDecidir: (decision: "aprobada" | "rechazada") => void;
  pendiente: boolean;
  error: unknown;
};

export function ExcepcionSinReserva({ codigo, alDecidir, pendiente, error }: Props) {
  return (
    <AlertDialog open>
      <AlertDialogContent className="border-warning/40 bg-warning/10 text-foreground sm:max-w-md">
        <AlertDialogHeader>
          <p className="text-xs font-bold uppercase tracking-[0.16em] text-warning">
            Decisión de operador
          </p>
          <AlertDialogTitle className="font-display text-2xl font-bold text-foreground">
            Estudiante sin reserva
          </AlertDialogTitle>
          <AlertDialogDescription className="leading-6 text-foreground/80">
            Confirmá si permitís el ingreso. La decisión quedará registrada a tu nombre.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <div className="rounded-xl border border-warning/35 bg-card/80 px-4 py-3">
          <p className="text-xs font-bold uppercase tracking-wide text-warning">
            Cédula del estudiante
          </p>
          <p className="mt-1 font-mono text-lg font-bold tabular-nums text-foreground">{codigo}</p>
        </div>
        {error && <p className="text-sm font-semibold text-destructive">{errMsg(error)}</p>}
        <AlertDialogFooter className="sm:grid sm:grid-cols-2">
          <Button
            type="button"
            className="h-12 font-bold"
            disabled={pendiente}
            onClick={() => alDecidir("aprobada")}
          >
            <CheckCircle2 className="mr-2 h-5 w-5" /> Aprobar ingreso
          </Button>
          <Button
            type="button"
            variant="destructive"
            className="h-12 font-bold"
            disabled={pendiente}
            onClick={() => alDecidir("rechazada")}
          >
            <XCircle className="mr-2 h-5 w-5" /> Rechazar ingreso
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
