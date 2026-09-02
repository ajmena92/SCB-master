import { useEffect, useRef, useState } from "react";
import type { FormEvent } from "react";
import { toast } from "sonner";
import { errMsg } from "@/compartido/consultas/errores_api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { registrarMarcaTransporteEtapaDos } from "@/funcionalidades/rutas/consultas/rutas";

export function CapturaTransporte() {
  const [resultado, setResultado] = useState("");
  const [marcando, setMarcando] = useState(false);
  const capturaRef = useRef<HTMLInputElement>(null);

  useEffect(() => capturaRef.current?.focus(), []);

  const registrar = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    const formulario = event.currentTarget;
    const codigo = String(new FormData(formulario).get("codigo") || "").trim();
    if (!codigo) return;
    setMarcando(true);
    setResultado("");
    try {
      const mensaje = await registrarMarcaTransporteEtapaDos(codigo);
      setResultado(mensaje);
      toast.success("Marca registrada");
      formulario.reset();
    } catch (error) {
      const mensaje = errMsg(error);
      setResultado(mensaje);
      toast.error(mensaje);
    } finally {
      setMarcando(false);
      formulario.querySelector<HTMLInputElement>("input[name='codigo']")?.focus();
    }
  };

  return (
    <form
      className="rounded-2xl border bg-card p-4 shadow-[0_8px_30px_rgb(45_54_150_/_0.05)]"
      onSubmit={registrar}
      data-testid="transporte-captura"
    >
      <div className="flex flex-col gap-3 md:flex-row md:items-end">
        <div className="flex-1">
          <label htmlFor="transporte-codigo" className="text-xs font-bold uppercase tracking-wide">
            Captura de transporte
          </label>
          <Input
            id="transporte-codigo"
            ref={capturaRef}
            name="codigo"
            autoComplete="off"
            required
            placeholder="Escanee o escriba el código del estudiante"
            className="mt-2 h-12 text-base"
          />
        </div>
        <Button type="submit" disabled={marcando} className="h-12">
          {marcando ? "Registrando…" : "Registrar marca"}
        </Button>
      </div>
      {resultado && (
        <p className="mt-3 text-sm font-medium" role="status">
          {resultado}
        </p>
      )}
    </form>
  );
}
