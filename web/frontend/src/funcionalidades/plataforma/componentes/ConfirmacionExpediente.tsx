import { Warning } from "@phosphor-icons/react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { Persona } from "@/compartido/contratos/plataforma";

type Confirmacion = "pin" | "desactivar" | undefined;

export function ConfirmacionExpediente({
  confirmacion,
  persona,
  ocupada,
  alCerrar,
  alConfirmar,
}: {
  confirmacion: Confirmacion;
  persona: Persona;
  ocupada: boolean;
  alCerrar: () => void;
  alConfirmar: (tipo: Exclude<Confirmacion, undefined>) => void;
}) {
  return (
    <AlertDialog open={Boolean(confirmacion)} onOpenChange={(abierto) => !abierto && alCerrar()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>
            {confirmacion === "pin" ? "¿Reiniciar el PIN?" : "¿Desactivar estudiante?"}
          </AlertDialogTitle>
          <AlertDialogDescription>
            {confirmacion === "pin"
              ? `Se generará un PIN temporal nuevo para ${persona.nombres}. El anterior dejará de funcionar.`
              : `Se desactivará a ${persona.nombres}, se revocarán sus sesiones y no podrá utilizar el sistema.`}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={ocupada}>Cancelar</AlertDialogCancel>
          <AlertDialogAction
            className={
              confirmacion === "desactivar"
                ? "bg-destructive text-destructive-foreground hover:bg-destructive/90"
                : undefined
            }
            disabled={ocupada}
            onClick={() => confirmacion && alConfirmar(confirmacion)}
          >
            {confirmacion === "desactivar" && <Warning aria-hidden="true" size={18} />}
            {confirmacion === "pin" ? "Reiniciar PIN" : "Desactivar estudiante"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
