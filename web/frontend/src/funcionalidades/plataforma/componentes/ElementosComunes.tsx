import type { ReactNode } from "react";
import { EstadoVacio } from "@/compartido/componentes/Estados";

export { EstadoPanel, EstadoCarga, EstadoVacio } from "@/compartido/componentes/Estados";

export function EncabezadoPagina({
  titulo,
  descripcion,
  accion,
  id,
}: {
  titulo: string;
  descripcion: string;
  accion?: ReactNode;
  id?: string;
}) {
  return (
    <header className="mb-6 flex flex-col gap-3 border-b border-border pb-4 sm:flex-row sm:items-end sm:justify-between">
      <div>
        <h1
          id={id}
          className="font-heading text-xl font-semibold tracking-tight text-foreground sm:text-2xl"
        >
          {titulo}
        </h1>
        <p className="mt-1.5 max-w-[70ch] text-sm leading-6 text-muted-foreground sm:text-base">
          {descripcion}
        </p>
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
      className={`rounded-xl border px-4 py-3 text-sm leading-6 ${tipo === "error" ? "border-destructive/35 bg-destructive/10 text-foreground" : tipo === "exito" ? "border-success/35 bg-success/10 text-foreground" : "border-primary/25 bg-primary/10 text-foreground"}`}
      role={tipo === "error" ? "alert" : "status"}
      aria-live={tipo === "error" ? "assertive" : "polite"}
    >
      {children}
    </div>
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
  if (!filas.length) return <EstadoVacio>{vacio}</EstadoVacio>;
  return (
    <div className="rounded-xl border border-border bg-card">
      <div className="divide-y divide-border md:hidden" role="list" aria-label="Registros">
        {filas.map((fila, filaIndice) => (
          <article key={`movil-${filaIndice}`} className="space-y-3 p-4" role="listitem">
            {fila.map((celda, indice) => (
              <div key={`${columnas[indice]}-${filaIndice}`} className="min-w-0">
                <span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                  {columnas[indice]}
                </span>
                <div className="mt-1 break-words text-sm text-foreground">{celda}</div>
              </div>
            ))}
          </article>
        ))}
      </div>
      <div className="hidden overflow-x-auto md:block">
        <table className="w-full min-w-max border-collapse text-sm">
          <thead className="bg-muted text-left text-xs font-medium uppercase tracking-wide text-muted-foreground">
            <tr>
              {columnas.map((columna) => (
                <th key={columna} className="px-4 py-3">
                  {columna}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-border">
            {filas.map((fila) => (
              <tr key={JSON.stringify(fila)}>
                {fila.map((celda, indice) => (
                  <td key={columnas[indice]} className="px-4 py-3 align-top">
                    {celda}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
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
