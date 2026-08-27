import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useCuentas } from "@/funcionalidades/administracion/hooks/useCuentas";
export default function Cuentas() {
  const { id, setId, saldo, movimiento, error, cargando, consultar, registrar } = useCuentas();
  const enviarMovimiento = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const datos = Object.fromEntries(new FormData(e.currentTarget));
    await registrar({
      tipo: datos.tipo as "recarga" | "consumo" | "ajuste",
      monto: String(datos.monto),
      concepto: String(datos.concepto || ""),
    });
    e.currentTarget.reset();
  };
  return (
    <main className="space-y-6 p-6">
      <h1 className="text-2xl font-semibold">Cuentas y saldos</h1>
      <div className="flex gap-2">
        <Input
          value={id}
          onChange={(e) => setId(e.target.value)}
          type="number"
          min="1"
          placeholder="ID del estudiante"
          aria-label="ID del estudiante"
        />
        <Button onClick={() => void consultar()} disabled={!id || cargando}>
          Consultar saldo
        </Button>
      </div>
      {error && <p role="alert">{error}</p>}
      {saldo && (
        <p role="status">
          Saldo actual: <strong>{saldo.saldo}</strong>
        </p>
      )}
      <form className="space-y-3 rounded-xl border p-4" onSubmit={enviarMovimiento}>
        <h2 className="font-semibold">Registrar movimiento</h2>
        <select name="tipo" defaultValue="recarga" aria-label="Tipo">
          <option value="recarga">Recarga</option>
          <option value="consumo">Consumo</option>
          <option value="ajuste">Ajuste</option>
        </select>
        <Input
          name="monto"
          required
          type="number"
          step="0.01"
          min="0.01"
          placeholder="Monto"
          aria-label="Monto"
        />
        <Input name="concepto" placeholder="Concepto" aria-label="Concepto" />
        <Button type="submit" disabled={!id || cargando}>
          Guardar movimiento
        </Button>
      </form>
      {movimiento && (
        <p role="status">
          Movimiento #{movimiento.idMovimiento} guardado. Nuevo saldo: {movimiento.saldoNuevo}
        </p>
      )}
    </main>
  );
}
