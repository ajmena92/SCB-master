import { Link, useSearchParams } from "react-router-dom";
import { useInicioSesionAdministrativo } from "@/funcionalidades/identidad/hooks/useInicioSesion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Loader2, ShieldCheck, ArrowLeft } from "lucide-react";
import { SelectorTema } from "@/compartido/componentes/SelectorTema";

export default function AdminLogin() {
  const [parametros] = useSearchParams();
  const {
    nombreUsuario,
    contrasena,
    cambiarNombreUsuario,
    cambiarContrasena,
    enviar,
    cargando,
    error,
  } = useInicioSesionAdministrativo();

  return (
    <div className="relative flex min-h-screen items-center justify-center bg-background p-4 sm:p-6">
      <div className="absolute right-4 top-4"><SelectorTema /></div>
      <div className="w-full max-w-sm animate-fade-up">
        <div className="mb-8 flex items-center gap-3 text-secondary sm:mb-10">
          <span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary/10 text-primary">
            <ShieldCheck className="h-6 w-6" />
          </span>
          <span className="font-heading font-bold text-lg tracking-tight">
            Panel Administrativo — Comedor SCSC
          </span>
        </div>
        <div className="rounded-3xl border border-border bg-card p-6 shadow-[0_16px_40px_rgb(45_54_150_/_0.10)] sm:p-8">
          <p className="mb-2 font-body text-xs font-medium uppercase tracking-[0.2em] text-primary">
            Usuario de la plataforma web
          </p>
          <h1 className="font-display text-2xl font-bold tracking-tight mb-6">Iniciar sesión</h1>
          {parametros.get("contrasena") === "actualizada" && (
            <p
              className="mb-5 rounded-xl bg-emerald-100 px-4 py-3 text-sm font-semibold text-emerald-900"
              role="status"
            >
              Contraseña actualizada. Iniciá sesión con tu nueva contraseña.
            </p>
          )}
          <form onSubmit={enviar} className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="usuario">Nombre de usuario</Label>
              <Input
                id="usuario"
                data-testid="admin-user-input"
                value={nombreUsuario}
                onChange={(e) => cambiarNombreUsuario(e.target.value)}
                className="h-11"
                placeholder="usuario o correo"
              />
            </div>
            <div className="space-y-2">
              <Label htmlFor="pass">Contraseña</Label>
              <Input
                id="pass"
                type="password"
                data-testid="admin-pass-input"
                value={contrasena}
                onChange={(e) => cambiarContrasena(e.target.value)}
                className="h-11"
              />
            </div>
            {error && (
              <p data-testid="admin-login-error" className="text-sm font-medium text-destructive">
                {error}
              </p>
            )}
            <Button
              type="submit"
              data-testid="admin-login-submit"
              disabled={cargando}
              className="w-full font-semibold"
            >
              {cargando ? <Loader2 className="h-5 w-5 animate-spin" /> : "Ingresar"}
            </Button>
          </form>
        </div>
        <Link
          to="/"
          className="mt-6 flex items-center justify-center gap-2 text-sm text-muted-foreground hover:text-secondary transition-colors"
        >
          <ArrowLeft className="h-4 w-4" /> Volver al acceso estudiantil
        </Link>
      </div>
    </div>
  );
}
