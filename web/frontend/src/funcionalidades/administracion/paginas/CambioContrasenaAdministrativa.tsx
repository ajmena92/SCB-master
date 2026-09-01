import { useState, type FormEvent } from "react";
import { useMutation } from "@tanstack/react-query";
import { KeyRound, Loader2, LogOut } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { errMsg } from "@/compartido/consultas/errores_api";
import { usuariosAdministrativosApi } from "../consultas/usuarios";
import type { AutenticacionPlataforma } from "@/funcionalidades/plataforma/seguridad";

export default function CambioContrasenaAdministrativa() {
  const { limpiarSesion, logout } = useAutenticacion() as unknown as AutenticacionPlataforma;
  const navigate = useNavigate();
  const [actual, setActual] = useState("");
  const [nueva, setNueva] = useState("");
  const [confirmacion, setConfirmacion] = useState("");
  const [errorLocal, setErrorLocal] = useState("");
  const cambiar = useMutation({
    mutationFn: () => usuariosAdministrativosApi.cambiarContrasena(actual, nueva),
    onSuccess: () => {
      limpiarSesion();
      navigate("/admin?contrasena=actualizada", { replace: true });
    },
  });

  function enviar(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault();
    if (nueva.length < 12)
      return setErrorLocal("La nueva contraseña debe tener al menos 12 caracteres.");
    if (nueva !== confirmacion)
      return setErrorLocal("La confirmación no coincide con la nueva contraseña.");
    if (actual === nueva)
      return setErrorLocal("La nueva contraseña debe ser diferente a la actual.");
    setErrorLocal("");
    cambiar.mutate();
  }

  return (
    <main className="flex min-h-[100dvh] items-center justify-center bg-background p-4 sm:p-6">
      <section
        className="w-full max-w-md rounded-3xl border bg-card p-6 shadow-[0_16px_40px_rgb(45_54_150_/_0.10)] sm:p-8"
        aria-labelledby="cambio-contrasena-titulo"
      >
        <div className="flex items-start justify-between gap-4">
          <span className="flex h-12 w-12 items-center justify-center rounded-2xl bg-primary/10 text-primary">
            <KeyRound className="h-6 w-6" />
          </span>
          <Button variant="ghost" size="icon" onClick={logout} aria-label="Cerrar sesión">
            <LogOut className="h-5 w-5" />
          </Button>
        </div>
        <p className="mt-5 text-xs font-bold uppercase tracking-[0.16em] text-primary">
          Seguridad de la cuenta
        </p>
        <h1 id="cambio-contrasena-titulo" className="mt-1 font-display text-2xl font-black">
          Creá tu contraseña definitiva
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          La contraseña temporal solo sirve para este primer acceso. Al guardarla tendrás que
          iniciar sesión otra vez.
        </p>
        {(errorLocal || cambiar.error) && (
          <Alert variant="destructive" className="mt-5">
            <AlertDescription>{errorLocal || errMsg(cambiar.error)}</AlertDescription>
          </Alert>
        )}
        <form onSubmit={enviar} className="mt-6 space-y-4">
          <div className="space-y-2">
            <Label htmlFor="contrasena-actual">Contraseña actual</Label>
            <Input
              id="contrasena-actual"
              type="password"
              autoComplete="current-password"
              required
              value={actual}
              onChange={(evento) => setActual(evento.target.value)}
            />
          </div>
          <div className="space-y-2">
            <Label htmlFor="contrasena-nueva">Nueva contraseña</Label>
            <Input
              id="contrasena-nueva"
              type="password"
              autoComplete="new-password"
              required
              minLength={12}
              value={nueva}
              onChange={(evento) => setNueva(evento.target.value)}
            />
            <p className="text-xs text-muted-foreground">
              Usá al menos 12 caracteres y evitá datos fáciles de adivinar.
            </p>
          </div>
          <div className="space-y-2">
            <Label htmlFor="contrasena-confirmacion">Confirmar nueva contraseña</Label>
            <Input
              id="contrasena-confirmacion"
              type="password"
              autoComplete="new-password"
              required
              minLength={12}
              value={confirmacion}
              onChange={(evento) => setConfirmacion(evento.target.value)}
            />
          </div>
          <Button type="submit" disabled={cambiar.isPending} className="min-h-11 w-full font-bold">
            {cambiar.isPending ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Guardando…
              </>
            ) : (
              "Guardar contraseña"
            )}
          </Button>
        </form>
      </section>
    </main>
  );
}
