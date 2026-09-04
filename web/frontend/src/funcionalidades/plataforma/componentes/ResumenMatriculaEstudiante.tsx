import { BowlFood, Bus, Ticket } from "@phosphor-icons/react";
import type { ReactNode } from "react";
import type { Persona } from "@/compartido/contratos/plataforma";

function EstadoBeneficio({
  icono,
  etiqueta,
  valor,
  gestionar,
}: {
  icono: ReactNode;
  etiqueta: string;
  valor: ReactNode;
  gestionar?: string;
}) {
  return (
    <div className="flex items-center gap-3 border-b border-border py-3 last:border-b-0">
      <span
        className="grid size-9 shrink-0 place-items-center rounded-lg bg-primary/10 text-primary"
        aria-hidden="true"
      >
        {icono}
      </span>
      <div className="min-w-0 flex-1">
        <small className="block text-xs text-muted-foreground">{etiqueta}</small>
        <p className="truncate text-sm font-semibold text-foreground">{valor}</p>
      </div>
      {gestionar && (
        <a className="text-xs font-semibold text-primary hover:underline" href={gestionar}>
          Gestionar
        </a>
      )}
    </div>
  );
}

export function ResumenMatriculaEstudiante({ persona }: { persona: Persona }) {
  const rutaActual = persona.descripcionRuta ?? "Sin ruta asignada";
  return (
    <section
      className="rounded-xl border border-border bg-card p-4 shadow-sm"
      aria-label="Resumen de la matrícula vigente"
    >
      <header className="mb-2 flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            Matrícula vigente
          </p>
          <h2 className="font-heading text-lg font-semibold">Resumen operativo</h2>
        </div>
        <span className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground">
          {persona.seccion ?? "Sin sección"}
        </span>
      </header>
      <EstadoBeneficio
        icono={<BowlFood size={20} />}
        etiqueta="Comedor"
        valor={persona.becado ? "Beneficiario" : "No beneficiario"}
        gestionar="#beca-comedor"
      />
      <EstadoBeneficio
        icono={<Bus size={20} />}
        etiqueta="Transporte"
        valor={rutaActual}
        gestionar="#ruta-transporte"
      />
      <EstadoBeneficio
        icono={<Ticket size={20} />}
        etiqueta="Saldo de tiquetes"
        valor={
          <>
            <strong>{persona.saldoTiquetes ?? 0}</strong>
            <span> tiquetes disponibles</span>
          </>
        }
      />
    </section>
  );
}
