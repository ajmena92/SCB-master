import { useEffect, useRef } from "react";
import { Link } from "react-router-dom";
import { useInicioSesionEstudiantil } from "@/funcionalidades/identidad/hooks/useInicioSesion";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { InputOTP, InputOTPGroup, InputOTPSlot } from "@/components/ui/input-otp";
import { AlertTriangle, Loader2, UtensilsCrossed, ShieldCheck } from "lucide-react";

export default function StudentLogin() {
  const pinInputRef = useRef(null);
  const {
    cedula,
    pin,
    cambiarCedula,
    cambiarPin,
    enviar,
    cargando,
    error,
    tipoError: errorKind,
  } = useInicioSesionEstudiantil();

  useEffect(() => {
    if (error) pinInputRef.current?.focus();
  }, [error]);

  return (
    <div className="min-h-screen grid lg:grid-cols-2 bg-background">
      <div
        className="relative hidden lg:block overflow-hidden bg-cover bg-center"
        style={{ backgroundImage: "url('/images/student-login-background.avif')" }}
      >
        <div className="absolute inset-0 bg-secondary/85" />
        <div className="relative z-10 flex flex-col justify-between h-full p-12 text-white">
          <div className="flex items-center gap-3">
            <UtensilsCrossed className="h-8 w-8" />
            <span className="font-display font-black text-xl tracking-tight">Comedor SCSC</span>
          </div>
          <div>
            <h1 className="font-display text-4xl sm:text-5xl font-black tracking-tighter leading-none">
              Reservá tu almuerzo antes del cierre.
            </h1>
            <p className="mt-4 text-base text-white/80 max-w-md">
              Consultá el menú del día y avisá si asistirás al comedor. Tu confirmación se registra
              al instante.
            </p>
          </div>
        </div>
      </div>

      <div className="flex items-center justify-center p-6 sm:p-12">
        <div className="w-full max-w-sm animate-fade-up">
          <div className="lg:hidden flex items-center gap-2 mb-8 text-secondary">
            <UtensilsCrossed className="h-7 w-7" />
            <span className="font-display font-black text-lg">Comedor SCSC</span>
          </div>
          <p className="text-xs uppercase tracking-[0.2em] font-bold text-primary mb-2">
            Acceso de estudiantes y profesores
          </p>
          <h2 className="font-display text-3xl font-bold tracking-tight mb-8">
            Ingresá a tu portal
          </h2>

          <form onSubmit={enviar} className="space-y-6">
            <div className="space-y-2">
              <Label htmlFor="cedula">Cédula</Label>
              <Input
                id="cedula"
                data-testid="student-cedula-input"
                value={cedula}
                onChange={(e) => cambiarCedula(e.target.value)}
                placeholder="Número de cédula"
                className="h-12 text-base"
                autoComplete="username"
              />
            </div>
            <div className="space-y-2">
              <Label>PIN de 6 dígitos</Label>
              <InputOTP
                ref={pinInputRef}
                maxLength={6}
                value={pin}
                onChange={cambiarPin}
                data-testid="student-pin-input"
              >
                <InputOTPGroup className="w-full justify-between">
                  {[0, 1, 2, 3, 4, 5].map((i) => (
                    <InputOTPSlot
                      key={i}
                      index={i}
                      className="h-12 w-12 text-lg rounded-xl border-input"
                    />
                  ))}
                </InputOTPGroup>
              </InputOTP>
            </div>

            {error && (
              <div
                data-testid="student-login-error"
                role="alert"
                aria-live="assertive"
                className={`flex items-start gap-3 rounded-xl border px-4 py-3 text-sm font-semibold shadow-sm ${
                  errorKind === "servidor" || errorKind === "conexion"
                    ? "border-amber-500/40 bg-amber-50 text-amber-950"
                    : "border-destructive/40 bg-destructive/10 text-destructive"
                }`}
              >
                <AlertTriangle className="mt-0.5 h-5 w-5 shrink-0" aria-hidden="true" />
                <div>
                  <p className="font-bold">
                    {errorKind === "servidor"
                      ? "Servicio no disponible"
                      : errorKind === "conexion"
                        ? "No se pudo conectar"
                        : "No se pudo iniciar sesión"}
                  </p>
                  <p className="mt-1 font-medium">{error}</p>
                </div>
              </div>
            )}

            <Button
              type="submit"
              data-testid="student-login-submit"
              disabled={cargando}
              className="w-full h-12 rounded-full text-base font-bold transition-transform hover:-translate-y-0.5"
            >
              {cargando ? <Loader2 className="h-5 w-5 animate-spin" /> : "Ingresar"}
            </Button>
          </form>

          <Link
            to="/admin"
            data-testid="go-admin-link"
            className="mt-8 flex items-center justify-center gap-2 text-sm text-muted-foreground hover:text-secondary transition-colors"
          >
            <ShieldCheck className="h-4 w-4" /> Acceso administrativo
          </Link>
        </div>
      </div>
    </div>
  );
}
