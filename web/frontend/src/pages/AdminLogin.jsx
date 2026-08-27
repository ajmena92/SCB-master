import { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { api, errMsg } from "@/lib/api";
import { useAutenticacion } from "@/aplicacion/estado/ContextoAutenticacion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Loader2, ShieldCheck, ArrowLeft } from "lucide-react";

export default function AdminLogin() {
  const [nombreUsuario, setNombreUsuario] = useState("");
  const [password, setPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { loadMe } = useAutenticacion();

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);
    try {
      await api.post(
        "/v1/autenticacion",
        { nombreUsuario, contrasena: password },
        { omitirManejoFalloAutenticacion: true, omitirCsrf: true },
      );
      await loadMe();
      navigate("/admin/panel", { replace: true });
    } catch (err) {
      // El login no representa una sesión vencida: la API devuelve un detalle
      // seguro, como "Credenciales inválidas", que debe mostrarse al usuario.
      setError(errMsg(err, { showUnauthorizedDetail: true }));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-white p-4 sm:p-6">
      <div className="w-full max-w-sm animate-fade-up">
        <div className="mb-8 flex items-center gap-3 text-secondary sm:mb-10">
          <span className="flex h-11 w-11 items-center justify-center rounded-2xl bg-primary/10 text-primary">
            <ShieldCheck className="h-6 w-6" />
          </span>
          <span className="font-display font-black text-lg tracking-tight">
            Panel Administrativo — Comedor SCSC
          </span>
        </div>
        <div className="rounded-3xl border border-border bg-card p-6 shadow-[0_16px_40px_rgb(45_54_150_/_0.10)] sm:p-8">
          <p className="text-xs uppercase tracking-[0.2em] font-bold text-primary mb-2">
            Usuario de la plataforma web
          </p>
          <h1 className="font-display text-2xl font-bold tracking-tight mb-6">Iniciar sesión</h1>
          <form onSubmit={submit} className="space-y-5">
            <div className="space-y-2">
              <Label htmlFor="usuario">Nombre de usuario</Label>
              <Input
                id="usuario"
                data-testid="admin-user-input"
                value={nombreUsuario}
                onChange={(e) => setNombreUsuario(e.target.value)}
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
                value={password}
                onChange={(e) => setPassword(e.target.value)}
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
              disabled={loading}
              className="w-full font-bold"
            >
              {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : "Ingresar"}
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
