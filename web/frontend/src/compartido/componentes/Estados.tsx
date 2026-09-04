import type { ReactNode } from "react";

export function EstadoPanel({
  variante,
  titulo,
  children,
  accion,
}: {
  variante: "carga" | "vacio" | "error" | "exito";
  titulo?: string;
  children?: ReactNode;
  accion?: ReactNode;
}) {
  const estilos = {
    carga: "border-border bg-card text-muted-foreground",
    vacio: "border-border bg-card text-muted-foreground",
    error: "border-destructive/35 bg-destructive/10 text-foreground",
    exito: "border-success/35 bg-success/10 text-foreground",
  }[variante];
  return (
    <div
      className={`rounded-xl border px-4 py-4 text-sm leading-6 ${estilos}`}
      role={variante === "error" ? "alert" : "status"}
      aria-live={variante === "error" ? "assertive" : "polite"}
      aria-busy={variante === "carga"}
    >
      {titulo && <p className="font-semibold text-foreground">{titulo}</p>}
      {children && <div className={titulo ? "mt-1" : undefined}>{children}</div>}
      {accion && <div className="mt-3">{accion}</div>}
    </div>
  );
}

export function EstadoCarga({ children = "Cargando información…" }: { children?: ReactNode }) {
  return <EstadoPanel variante="carga">{children}</EstadoPanel>;
}

export function EstadoVacio({ children }: { children: ReactNode }) {
  return <EstadoPanel variante="vacio">{children}</EstadoPanel>;
}

export function EstadoError({ children }: { children: ReactNode }) {
  return <EstadoPanel variante="error">{children}</EstadoPanel>;
}

export function EstadoExito({ children }: { children: ReactNode }) {
  return <EstadoPanel variante="exito">{children}</EstadoPanel>;
}
