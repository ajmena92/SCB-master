import { useRef } from "react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

export default function CampoPin({ label, value, onChange, testid }) {
  const pinInputsRef = useRef([]);

  const actualizarDigitos = (index, digits) => {
    const siguiente = value.padEnd(6, " ").split("");
    if (!digits) siguiente[index] = " ";
    digits.split("").forEach((digit, offset) => {
      if (index + offset < 6) siguiente[index + offset] = digit;
    });
    onChange(siguiente.slice(0, 6).join("").trimEnd());
    const nextIndex = digits ? Math.min(index + digits.length, 5) : index;
    pinInputsRef.current[nextIndex]?.focus();
  };

  const manejarTecla = (event, index) => {
    if (event.key === "Backspace" && !value[index] && index > 0) {
      event.preventDefault();
      onChange(value.slice(0, index - 1) + value.slice(index));
      pinInputsRef.current[index - 1]?.focus();
    }
    if (event.key === "ArrowLeft" && index > 0) pinInputsRef.current[index - 1]?.focus();
    if (event.key === "ArrowRight" && index < 5) pinInputsRef.current[index + 1]?.focus();
  };

  const pegar = (event, index) => {
    event.preventDefault();
    const digits = event.clipboardData
      .getData("text")
      .replace(/\D/g, "")
      .slice(0, 6 - index);
    if (digits) actualizarDigitos(index, digits);
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
              actualizarDigitos(index, event.target.value.replace(/\D/g, "").slice(-1))
            }
            onKeyDown={(event) => manejarTecla(event, index)}
            onPaste={(event) => pegar(event, index)}
            aria-label={`${label}, dígito ${index + 1}`}
            className="h-11 w-full min-w-0 rounded-xl p-0 text-center text-xl font-semibold sm:h-12"
          />
        ))}
      </div>
    </div>
  );
}
