import { CaretDown, CaretUp, Key, PencilSimple } from "@phosphor-icons/react";
import type { Persona } from "@/compartido/contratos/plataforma";

export type OrdenPersonas = "nombres" | "cedula" | "tipo" | "estado";

type Tono = "neutral" | "exito" | "alerta" | "info";

interface Propiedades {
  personas: Persona[];
  total: number;
  pagina: number;
  tamano: number;
  ordenarPor: OrdenPersonas;
  direccion: "asc" | "desc";
  vacio: string;
  alCambiarPagina: (pagina: number) => void;
  alOrdenar: (columna: OrdenPersonas) => void;
  alEditar: (persona: Persona) => void;
  alReiniciarPin: (persona: Persona) => void;
}

function Etiqueta({ tono = "neutral", children }: { tono?: Tono; children: string }) {
  const clases = tono === "exito" ? "bg-success/15 text-emerald-800 dark:text-emerald-200" : tono === "alerta" ? "bg-warning/15 text-amber-800 dark:text-amber-200" : tono === "info" ? "bg-primary/15 text-primary" : "bg-muted text-muted-foreground";
  return <span className={`inline-flex max-w-full rounded-full px-2.5 py-1 text-xs font-medium leading-tight ${clases}`}>{children}</span>;
}

function AccionExpediente({ persona, alEditar, alReiniciarPin }: Pick<Propiedades, "alEditar" | "alReiniciarPin"> & { persona: Persona }) {
  const nombreTipo = persona.tipo === "estudiante" ? "estudiante" : "profesor";
  return <span className="inline-flex gap-1">
    <button className="grid h-11 w-11 place-items-center rounded-lg border border-border bg-card text-primary transition-colors hover:border-primary hover:bg-primary/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring" type="button" onClick={() => alEditar(persona)} aria-label={`Editar ${nombreTipo} ${persona.nombres}`} title={`Editar ${nombreTipo}`}><PencilSimple aria-hidden="true" size={19} /></button>
    <button className="grid h-11 w-11 place-items-center rounded-lg border border-border bg-card text-primary transition-colors hover:border-primary hover:bg-primary/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring disabled:opacity-50" type="button" onClick={() => alReiniciarPin(persona)} aria-label={`Reiniciar PIN de ${persona.nombres}`} title="Reiniciar PIN" disabled={!persona.activo}><Key aria-hidden="true" size={18} /></button>
  </span>;
}

function BeneficiosPersona({ persona }: { persona: Persona }) {
  if (persona.tipo !== "estudiante") return <span className="text-sm text-muted-foreground">No aplica</span>;
  const ruta = persona.rutaId ? persona.descripcionRuta ?? "Sin descripción" : "Sin ruta asignada";
  return <div className="grid min-w-0 gap-1 text-sm"><p className="grid grid-cols-[4.7rem_minmax(0,1fr)] gap-2 text-muted-foreground"><span>Comedor</span><strong className="truncate font-medium text-foreground">{persona.becado ? "Beneficiario" : "No beneficiario"}</strong></p>
    <p className="grid grid-cols-[4.7rem_minmax(0,1fr)] gap-2 text-muted-foreground" title={ruta}><span>Ruta</span><strong className="truncate font-medium text-foreground">{ruta}</strong></p>
  </div>;
}

function EncabezadoOrdenable({ columna, children, ordenarPor, direccion, alOrdenar }: Pick<Propiedades, "ordenarPor" | "direccion" | "alOrdenar"> & { columna: OrdenPersonas; children: string }) {
  const activo = ordenarPor === columna;
  return <th aria-sort={activo ? (direccion === "asc" ? "ascending" : "descending") : "none"}>
    <button className="inline-flex items-center gap-1 border-0 bg-transparent p-0 font-medium text-inherit hover:text-primary" type="button" onClick={() => alOrdenar(columna)}>
      {children}{activo && (direccion === "asc" ? <CaretUp aria-hidden="true" size={14} /> : <CaretDown aria-hidden="true" size={14} />)}
    </button>
  </th>;
}

export default function ListadoPersonas({ personas, total, pagina, tamano, ordenarPor, direccion, vacio, alCambiarPagina, alOrdenar, alEditar, alReiniciarPin }: Propiedades) {
  if (!personas.length) return <p className="empty-state">{vacio}</p>;
  const totalPaginas = Math.ceil(total / tamano);
  const primera = (pagina - 1) * tamano + 1;
  const ultima = Math.min(pagina * tamano, total);
  return <>
    <div className="overflow-x-auto rounded-xl border border-border bg-card" role="region" aria-label="Padrón de personas">
      <table className="w-full min-w-[64rem] table-fixed border-collapse text-sm">
        <colgroup><col className="w-[23%]" /><col className="w-[12%]" /><col className="w-[11%]" /><col className="w-[31%]" /><col className="w-[10%]" /><col className="w-32" /></colgroup>
        <thead className="bg-muted text-left text-xs uppercase tracking-wide text-muted-foreground"><tr><EncabezadoOrdenable columna="nombres" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Persona</EncabezadoOrdenable><EncabezadoOrdenable columna="cedula" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Cédula</EncabezadoOrdenable><EncabezadoOrdenable columna="tipo" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Tipo</EncabezadoOrdenable><th className="px-4 py-3">Beneficios</th><EncabezadoOrdenable columna="estado" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Estado</EncabezadoOrdenable><th className="px-4 py-3 text-right">Acciones</th></tr></thead>
        <tbody className="divide-y divide-border">{personas.map((persona) => <tr className="hover:bg-primary/5" key={persona.id}>
          <td className="px-4 py-3"><p className="font-medium leading-5 text-foreground">{persona.nombres}</p></td>
          <td className="px-4 py-3 text-muted-foreground">{persona.cedula ?? "Sin cédula"}</td>
          <td><Etiqueta tono={persona.tipo === "estudiante" ? "info" : "neutral"}>{persona.tipo === "estudiante" ? "Estudiante" : "Profesor"}</Etiqueta></td>
          <td><BeneficiosPersona persona={persona} /></td>
          <td><Etiqueta tono={persona.activo ? "exito" : "alerta"}>{persona.activo ? "Activa" : "Inactiva"}</Etiqueta></td>
          <td className="px-4 py-3 text-right"><AccionExpediente persona={persona} alEditar={alEditar} alReiniciarPin={alReiniciarPin} /></td>
        </tr>)}</tbody>
      </table>
    </div>
    <div className="grid gap-3 md:hidden">{personas.map((persona) => <article className="rounded-xl border border-border bg-card p-4" key={persona.id}>
      <div className="flex items-start justify-between gap-3"><div><p className="font-medium text-foreground">{persona.nombres}</p><p className="mt-1 text-sm text-muted-foreground">Cédula: {persona.cedula ?? "Sin cédula"}</p></div><AccionExpediente persona={persona} alEditar={alEditar} alReiniciarPin={alReiniciarPin} /></div>
      <div className="mt-3 flex flex-wrap gap-2"><Etiqueta tono={persona.tipo === "estudiante" ? "info" : "neutral"}>{persona.tipo === "estudiante" ? "Estudiante" : "Profesor"}</Etiqueta><Etiqueta tono={persona.activo ? "exito" : "alerta"}>{persona.activo ? "Activa" : "Inactiva"}</Etiqueta></div>
      <div className="mt-3 border-t border-border pt-3"><span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">Beneficios</span><div className="mt-2"><BeneficiosPersona persona={persona} /></div></div>
    </article>)}</div>
    <nav className="mt-4 flex flex-col gap-3 text-sm text-muted-foreground sm:flex-row sm:items-center sm:justify-between" aria-label="Paginación de personas">
      <span>Mostrando {primera}–{ultima} de {total}</span>
      {totalPaginas > 1 && <div>
        <button className="button secondary" type="button" disabled={pagina === 1} onClick={() => alCambiarPagina(pagina - 1)}>Anterior</button>
        <span>Página {pagina} de {totalPaginas}</span>
        <button className="button secondary" type="button" disabled={pagina === totalPaginas} onClick={() => alCambiarPagina(pagina + 1)}>Siguiente</button>
      </div>}
    </nav>
  </>;
}
