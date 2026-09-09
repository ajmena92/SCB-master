import type { Persona } from "@/compartido/contratos/plataforma";
import { Campo } from "./ElementosComunes";

interface PropiedadesDatosPadronExpediente {
  persona: Persona;
}

/** Datos administrados por el padrón anual, sin mutaciones ni consultas propias. */
export function DatosPadronExpediente({ persona }: PropiedadesDatosPadronExpediente) {
  const esEstudiante = persona.tipo === "estudiante";
  return (
    <section className="grid gap-4 rounded-xl border border-border bg-card p-4 shadow-sm">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
            Importado del padrón
          </p>
          <h2 className="font-heading text-xl font-semibold">
            {esEstudiante ? "Datos del estudiante" : "Datos del profesor"}
          </h2>
        </div>
        <span
          className="rounded-full bg-muted px-2 py-1 text-xs text-muted-foreground"
          aria-label="Datos bloqueados"
        >
          🔒 Solo lectura
        </span>
      </div>
      <p className="text-sm text-muted-foreground">
        Estos datos los administra el padrón anual. Para corregirlos, actualice el padrón y vuelva a
        importarlo.
      </p>
      <div className="grid gap-4 sm:grid-cols-2">
        <Campo etiqueta="Cédula">
          <input value={persona.cedula ?? ""} disabled />
        </Campo>
        <Campo etiqueta="Nombre completo">
          <input value={persona.nombres} disabled />
        </Campo>
        {esEstudiante && (
          <Campo etiqueta="Sección">
            <input value={persona.seccion ?? "Sin sección"} disabled />
          </Campo>
        )}
      </div>
    </section>
  );
}
