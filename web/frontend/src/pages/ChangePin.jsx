import { Button } from "@/components/ui/button";
import { Loader2, KeyRound } from "lucide-react";
import CampoPin from "@/funcionalidades/identidad/componentes/CampoPin";
import { useCambioPin } from "@/funcionalidades/identidad/hooks/useCambioPin";

export default function ChangePin() {
  const {
    actual,
    nuevo,
    confirmar,
    cambiarActual,
    cambiarNuevo,
    cambiarConfirmar,
    enviar,
    debeCambiarPin,
    cargando,
    error,
  } = useCambioPin();

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4 sm:p-6">
      <div className="w-full max-w-md animate-fade-up">
        <div className="mx-auto mb-6 h-14 w-14 rounded-2xl bg-primary/10 flex items-center justify-center">
          <KeyRound className="h-7 w-7 text-primary" />
        </div>
        <h1 className="font-display text-3xl font-bold tracking-tight text-center">
          Cambiá tu PIN
        </h1>
        <p className="text-center text-muted-foreground mt-2 mb-8">
          {debeCambiarPin
            ? "Por seguridad, definí un nuevo PIN en tu primer ingreso."
            : "Actualizá tu PIN de acceso."}
        </p>
        <form
          onSubmit={enviar}
          className="space-y-6 rounded-2xl border bg-card p-4 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] sm:p-6"
        >
          <CampoPin
            label="PIN actual"
            value={actual}
            onChange={cambiarActual}
            testid="pin-actual-input"
          />
          <CampoPin
            label="Nuevo PIN"
            value={nuevo}
            onChange={cambiarNuevo}
            testid="pin-nuevo-input"
          />
          <CampoPin
            label="Confirmar nuevo PIN"
            value={confirmar}
            onChange={cambiarConfirmar}
            testid="pin-confirmar-input"
          />
          {error && (
            <p data-testid="change-pin-error" className="text-sm font-medium text-destructive">
              {error}
            </p>
          )}
          <Button
            type="submit"
            data-testid="change-pin-submit"
            disabled={cargando}
            className="w-full h-12 rounded-full font-bold"
          >
            {cargando ? <Loader2 className="h-5 w-5 animate-spin" /> : "Guardar PIN"}
          </Button>
        </form>
      </div>
    </div>
  );
}
