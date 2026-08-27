import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { api, errMsg } from "@/lib/api";
import type { MovimientoEntrada, MovimientoSalida, SaldoSalida } from "@/compartido/contratos/api";
export default function Cuentas() {
  const [id, setId] = useState("");
  const [saldo, setSaldo] = useState<SaldoSalida>();
  const [mov, setMov] = useState<MovimientoSalida>();
  const [error, setError] = useState("");
  const consultar = async () => {
    try {
      setError("");
      setSaldo((await api.get<SaldoSalida>(`/v1/cuentas/${Number(id)}/saldo`)).data);
    } catch (e) {
      setError(errMsg(e));
    }
  };
  const registrar = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    try {
      const d = Object.fromEntries(new FormData(e.currentTarget));
      const datos: MovimientoEntrada = {
        tipo: d.tipo as MovimientoEntrada["tipo"],
        monto: String(d.monto),
        concepto: String(d.concepto || ""),
        claveIdempotencia: crypto.randomUUID(),
      };
      setMov(
        (await api.post<MovimientoSalida>(`/v1/cuentas/${Number(id)}/movimientos`, datos)).data,
      );
      await consultar();
      e.currentTarget.reset();
    } catch (x) {
      setError(errMsg(x));
    }
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
        <Button onClick={() => void consultar()} disabled={!id}>
          Consultar saldo
        </Button>
      </div>
      {error && <p role="alert">{error}</p>}
      {saldo && (
        <p role="status">
          Saldo actual: <strong>{saldo.saldo}</strong>
        </p>
      )}
      <form className="space-y-3 rounded-xl border p-4" onSubmit={registrar}>
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
        <Button type="submit" disabled={!id}>
          Guardar movimiento
        </Button>
      </form>
      {mov && (
        <p role="status">
          Movimiento #{mov.idMovimiento} guardado. Nuevo saldo: {mov.saldoNuevo}
        </p>
      )}
    </main>
  );
}
