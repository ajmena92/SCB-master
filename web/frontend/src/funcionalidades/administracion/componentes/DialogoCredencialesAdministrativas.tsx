import { useState } from "react";
import { Check, Copy, KeyRound, ShieldCheck } from "lucide-react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { CredencialesParaMostrar } from "@/compartido/contratos/usuarios_administrativos";

export default function DialogoCredencialesAdministrativas({
  credenciales,
  alCerrar,
}: {
  credenciales?: CredencialesParaMostrar;
  alCerrar: () => void;
}) {
  const [copiado, setCopiado] = useState(false);

  async function copiar() {
    if (!credenciales) return;
    const lineas = [
      `Profesor: ${credenciales.nombres}`,
      `Usuario: ${credenciales.usuario}`,
      `Contraseña temporal: ${credenciales.contrasena}`,
    ];
    if (credenciales.pin) lineas.push(`PIN temporal del portal: ${credenciales.pin}`);
    await navigator.clipboard.writeText(lineas.join("\n"));
    setCopiado(true);
  }

  return (
    <AlertDialog open={Boolean(credenciales)}>
      <AlertDialogContent className="max-w-xl">
        <AlertDialogHeader>
          <span className="mb-2 flex h-11 w-11 items-center justify-center rounded-xl bg-primary/10 text-primary">
            <KeyRound className="h-5 w-5" aria-hidden="true" />
          </span>
          <AlertDialogTitle>Guardá las credenciales ahora</AlertDialogTitle>
          <AlertDialogDescription>
            Por seguridad se muestran una sola vez. Entregalas directamente al profesor.
          </AlertDialogDescription>
        </AlertDialogHeader>
        {credenciales && (
          <dl className="grid gap-3 rounded-2xl border bg-muted/40 p-4 text-sm sm:grid-cols-2">
            <div className="sm:col-span-2">
              <dt className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
                Profesor
              </dt>
              <dd className="mt-1 font-semibold">{credenciales.nombres}</dd>
            </div>
            <div>
              <dt className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
                Usuario
              </dt>
              <dd className="mt-1 break-all font-semibold">{credenciales.usuario}</dd>
            </div>
            <div>
              <dt className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
                Contraseña temporal
              </dt>
              <dd className="mt-1 break-all font-mono text-base font-bold">
                {credenciales.contrasena}
              </dd>
            </div>
            {credenciales.pin && (
              <div className="sm:col-span-2">
                <dt className="text-xs font-bold uppercase tracking-wide text-muted-foreground">
                  PIN temporal del portal
                </dt>
                <dd className="mt-1 font-mono text-xl font-bold tracking-[0.18em]">
                  {credenciales.pin}
                </dd>
              </div>
            )}
          </dl>
        )}
        <p className="flex gap-2 text-xs leading-relaxed text-muted-foreground">
          <ShieldCheck className="mt-0.5 h-4 w-4 shrink-0" aria-hidden="true" />
          La contraseña administrativa deberá cambiarse al iniciar sesión. El PIN corresponde al
          acceso independiente del portal docente.
        </p>
        <AlertDialogFooter className="gap-2">
          <button
            type="button"
            className="inline-flex min-h-11 items-center justify-center gap-2 rounded-lg border bg-card px-4 text-sm font-bold"
            onClick={copiar}
          >
            {copiado ? <Check className="h-4 w-4" /> : <Copy className="h-4 w-4" />}
            {copiado ? "Copiado" : "Copiar credenciales"}
          </button>
          <AlertDialogAction
            className="min-h-11 font-bold"
            onClick={() => {
              setCopiado(false);
              alCerrar();
            }}
          >
            Ya las guardé
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
