import type { ReactNode } from "react";

export function EstadoError({ children }: { children: ReactNode }) {
  return (
    <p
      className="rounded-xl border border-destructive/35 bg-destructive/10 px-4 py-3 text-sm leading-6 text-foreground"
      role="alert"
    >
      {children}
    </p>
  );
}

export function EstadoExito({ children }: { children: ReactNode }) {
  return (
    <p
      className="rounded-xl border border-success/35 bg-success/10 px-4 py-3 text-sm leading-6 text-foreground"
      role="status"
    >
      {children}
    </p>
  );
}
