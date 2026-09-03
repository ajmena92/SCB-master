import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { useTiquetes } from "@/funcionalidades/comedor/hooks/useTiquetes";

export function TiquetesPanel() {
  const {
    idPersona,
    cantidad,
    saldo,
    movimiento,
    error,
    cargando,
    setIdPersona,
    setCantidad,
    consultar,
    comprar,
  } = useTiquetes();

  return (
    <section className="space-y-6" aria-labelledby="tiquetes-title">
      <div>
        <h1 id="tiquetes-title" className="font-display text-2xl font-bold">
          Compras y saldo de tiquetes
        </h1>
        <p className="mt-1 text-sm text-muted-foreground">
          Consulte el saldo y registre compras para personas no becadas habilitadas.
        </p>
      </div>
      <div className="grid gap-4 rounded-xl border bg-card p-4 sm:grid-cols-[1fr_auto]">
        <Input
          value={idPersona}
          onChange={(e) => setIdPersona(e.target.value)}
          type="number"
          min="1"
          placeholder="ID de la persona de comedor"
          aria-label="ID de la persona de comedor"
        />
        <Button onClick={() => void consultar()} disabled={!idPersona || cargando}>
          {cargando ? "Consultando…" : "Consultar saldo"}
        </Button>
      </div>
      {saldo && (
        <div className="rounded-xl border bg-primary/5 p-4" role="status">
          <p className="text-sm text-muted-foreground">Saldo disponible</p>
          <p className="mt-1 font-display text-3xl font-bold">{saldo.saldo}</p>
          <p className="text-xs text-muted-foreground">
            Cuenta #{saldo.idCuenta} · {saldo.disponibles} disponibles
          </p>
        </div>
      )}
      <form
        className="space-y-3 rounded-xl border bg-card p-4"
        onSubmit={(event) => {
          event.preventDefault();
          void comprar();
        }}
      >
        <h2 className="font-semibold">Registrar compra o recarga</h2>
        <Input
          value={cantidad}
          onChange={(e) => setCantidad(e.target.value)}
          required
          type="number"
          step="1"
          min="1"
          placeholder="Cantidad de tiquetes"
          aria-label="Cantidad de tiquetes"
        />
        <Button type="submit" disabled={!idPersona || !cantidad || cargando}>
          {cargando ? "Guardando…" : "Registrar compra"}
        </Button>
      </form>
      {error && (
        <p role="alert" className="text-sm text-destructive">
          {error}
        </p>
      )}
      {movimiento && (
        <p role="status" className="text-sm text-muted-foreground">
          Compra registrada. Nuevo saldo: {String(movimiento.saldoNuevo ?? "actualizado")}
        </p>
      )}
      <p className="text-xs text-muted-foreground">
        La compra se aplica a cualquier persona habilitada del catálogo de comedor, incluido el
        profesorado no becado.
      </p>
    </section>
  );
}
