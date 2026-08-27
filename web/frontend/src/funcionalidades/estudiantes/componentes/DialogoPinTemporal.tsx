import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";

type Props = { pin: string | null; alCerrar: () => void };

export function DialogoPinTemporal({ pin, alCerrar }: Props) {
  return (
    <AlertDialog open={!!pin} onOpenChange={(abierto) => !abierto && alCerrar()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>PIN temporal generado</AlertDialogTitle>
          <AlertDialogDescription>
            Comuníquelo al estudiante de forma segura. No se volverá a mostrar.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <p
          data-testid="nuevo-pin-value"
          className="py-4 text-center font-display text-4xl font-black tracking-[0.3em] text-primary"
        >
          {pin}
        </p>
        <AlertDialogFooter>
          <AlertDialogAction onClick={alCerrar}>Entendido</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
