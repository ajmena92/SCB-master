import { useState } from "react";
import { api, errMsg } from "@/lib/api";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { toast } from "sonner";
export default function Comedor() {
  const [idEstudiante, setIdEstudiante] = useState("");
  const [fecha, setFecha] = useState(() => new Date().toISOString().slice(0, 10));
  const [guardando, setGuardando] = useState(false);
  async function registrar() {
    if (!idEstudiante) {
      toast.error("Indique el estudiante");
      return;
    }
    setGuardando(true);
    try {
      await api.post("/v1/comedor/registros", { idEstudiante: Number(idEstudiante), fecha });
      toast.success("Consumo registrado");
      setIdEstudiante("");
    } catch (e) {
      toast.error(errMsg(e));
    } finally {
      setGuardando(false);
    }
  }
  return (
    <section className="space-y-6">
      <h2 className="font-display text-2xl font-black">Registro de comedor</h2>
      <p className="text-muted-foreground">Registre el servicio de alimentación del día.</p>
      <div className="max-w-md space-y-4">
        <Input
          aria-label="ID del estudiante"
          value={idEstudiante}
          onChange={(e) => setIdEstudiante(e.target.value)}
          placeholder="ID del estudiante"
          inputMode="numeric"
        />
        <Input
          aria-label="Fecha"
          type="date"
          value={fecha}
          onChange={(e) => setFecha(e.target.value)}
        />
        <Button disabled={guardando} onClick={registrar}>
          {guardando ? "Registrando…" : "Registrar consumo"}
        </Button>
      </div>
    </section>
  );
}
