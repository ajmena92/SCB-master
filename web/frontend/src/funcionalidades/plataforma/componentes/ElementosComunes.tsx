import type { ReactNode } from "react";

export function EncabezadoPagina({
  titulo,
  descripcion,
  accion,
}: {
  titulo: string;
  descripcion: string;
  accion?: ReactNode;
}) {
  return (
    <header className="mb-8 flex flex-col gap-4 border-b border-border pb-5 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <h1 className="font-heading text-2xl font-bold tracking-tight text-foreground sm:text-3xl">{titulo}</h1>
        <p className="mt-2 max-w-[70ch] text-base leading-relaxed text-muted-foreground">{descripcion}</p>
      </div>
      {accion}
    </header>
  );
}

export function Aviso({
  tipo = "info",
  children,
}: {
  tipo?: "info" | "error" | "exito";
  children: ReactNode;
}) {
  return (
    <div
      className={`rounded-xl border px-4 py-3 text-sm leading-6 ${tipo === "error" ? "border-destructive/35 bg-destructive/10 text-foreground" : tipo === "exito" ? "border-success/35 bg-success/15 text-foreground" : "border-primary/25 bg-primary/10 text-foreground"}`}
      role={tipo === "error" ? "alert" : "status"}
    >
      {children}
    </div>
  );
}

export function EstadoCarga() {
  return (
    <p className="rounded-xl border border-dashed border-border bg-card px-6 py-8 text-center text-sm text-muted-foreground" role="status">
      Cargando información…
    </p>
  );
}

export function Tabla({
  columnas,
  filas,
  vacio = "No hay registros.",
}: {
  columnas: string[];
  filas: ReactNode[][];
  vacio?: string;
}) {
  if (!filas.length) return <p className="rounded-xl border border-dashed border-border bg-card px-6 py-8 text-center text-sm text-muted-foreground">{vacio}</p>;
  return (
    <div className="overflow-x-auto rounded-xl border border-border bg-card">
      <table className="w-full min-w-max border-collapse text-sm">
        <thead className="bg-muted text-left text-xs font-medium uppercase tracking-wide text-muted-foreground">
          <tr>
            {columnas.map((columna) => (
              <th key={columna} className="px-4 py-3">{columna}</th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-border">
          {filas.map((fila) => (
            <tr key={JSON.stringify(fila)}>
              {fila.map((celda, indice) => (
                <td key={columnas[indice]} className="px-4 py-3 align-top">{celda}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function Campo({ etiqueta, children }: { etiqueta: string; children: ReactNode }) {
  return (
    <label className="flex min-w-0 flex-col gap-2 font-body text-sm font-medium text-foreground">
      <span>{etiqueta}</span>
      {children}
    </label>
  );
}
