import { useRegistroConsumo } from "@/funcionalidades/comedor/hooks/useRegistroConsumo";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
export default function Comedor() {
  const { idEstudiante, fecha, guardando, setIdEstudiante, setFecha, registrar } =
    useRegistroConsumo();
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
