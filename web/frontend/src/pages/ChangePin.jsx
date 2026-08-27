import { useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import { api, errMsg } from "@/lib/api";
import { useAuth } from "@/context/AuthContext";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";
import { Loader2, KeyRound } from "lucide-react";

function PinField({ label, value, onChange, testid }) {
  const pinInputsRef = useRef([]);

  const updateDigits = (index, digits) => {
    const next = value.padEnd(6, " ").split("");
    if (!digits) next[index] = " ";
    digits.split("").forEach((digit, offset) => {
      if (index + offset < 6) next[index + offset] = digit;
    });
    const result = next.slice(0, 6).join("");
    onChange(result.trimEnd());
    const nextIndex = digits ? Math.min(index + digits.length, 5) : index;
    pinInputsRef.current[nextIndex]?.focus();
  };

  const handleKeyDown = (event, index) => {
    if (event.key === "Backspace" && !value[index] && index > 0) {
      event.preventDefault();
      const next = value.slice(0, index - 1) + value.slice(index);
      onChange(next);
      pinInputsRef.current[index - 1]?.focus();
    }
    if (event.key === "ArrowLeft" && index > 0) pinInputsRef.current[index - 1]?.focus();
    if (event.key === "ArrowRight" && index < 5) pinInputsRef.current[index + 1]?.focus();
  };

  const handlePaste = (event, index) => {
    event.preventDefault();
    const digits = event.clipboardData
      .getData("text")
      .replace(/\D/g, "")
      .slice(0, 6 - index);
    if (digits) updateDigits(index, digits);
  };

  return (
    <div className="space-y-2">
      <Label htmlFor={testid}>{label}</Label>
      <div
        className="grid w-full grid-cols-6 gap-1 sm:gap-2"
        role="group"
        aria-label={label}
        data-testid={testid}
      >
        {[0, 1, 2, 3, 4, 5].map((index) => (
          <Input
            key={index}
            ref={(element) => {
              pinInputsRef.current[index] = element;
            }}
            id={`${testid}-${index + 1}`}
            type="password"
            inputMode="numeric"
            pattern="[0-9]*"
            autoComplete={index === 0 ? "one-time-code" : "off"}
            maxLength={1}
            value={value[index] || ""}
            onChange={(event) =>
              updateDigits(index, event.target.value.replace(/\D/g, "").slice(-1))
            }
            onKeyDown={(event) => handleKeyDown(event, index)}
            onPaste={(event) => handlePaste(event, index)}
            aria-label={`${label}, dígito ${index + 1}`}
            className="h-11 w-full min-w-0 rounded-xl p-0 text-center text-xl font-semibold sm:h-12"
          />
        ))}
      </div>
    </div>
  );
}

export default function ChangePin() {
  const [actual, setActual] = useState("");
  const [nuevo, setNuevo] = useState("");
  const [confirmar, setConfirmar] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const navigate = useNavigate();
  const { setDebeCambiarPin, debeCambiarPin } = useAuth();

  const submit = async (e) => {
    e.preventDefault();
    setError("");
    if (nuevo.length !== 6) return setError("El nuevo PIN debe tener 6 dígitos");
    if (nuevo !== confirmar) return setError("Los PIN nuevos no coinciden");
    setLoading(true);
    try {
      await api.post("/v1/estudiantes/pin", { pinActual: actual, pinNuevo: nuevo });
      setDebeCambiarPin(false);
      toast.success("PIN actualizado correctamente");
      navigate("/estudiante", { replace: true });
    } catch (err) {
      setError(errMsg(err));
    } finally {
      setLoading(false);
    }
  };

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
          onSubmit={submit}
          className="space-y-6 rounded-2xl border bg-card p-4 shadow-[0_8px_30px_rgb(45_54_150_/_0.08)] sm:p-6"
        >
          <PinField
            label="PIN actual"
            value={actual}
            onChange={setActual}
            testid="pin-actual-input"
          />
          <PinField label="Nuevo PIN" value={nuevo} onChange={setNuevo} testid="pin-nuevo-input" />
          <PinField
            label="Confirmar nuevo PIN"
            value={confirmar}
            onChange={setConfirmar}
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
            disabled={loading}
            className="w-full h-12 rounded-full font-bold"
          >
            {loading ? <Loader2 className="h-5 w-5 animate-spin" /> : "Guardar PIN"}
          </Button>
        </form>
      </div>
    </div>
  );
}
