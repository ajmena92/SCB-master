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
  return <span className={`person-tag person-tag--${tono}`}>{children}</span>;
}

function AccionExpediente({ persona, alEditar, alReiniciarPin }: Pick<Propiedades, "alEditar" | "alReiniciarPin"> & { persona: Persona }) {
  const nombreTipo = persona.tipo === "estudiante" ? "estudiante" : "profesor";
  return <span className="person-quick-actions">
    <button className="expediente-action" type="button" onClick={() => alEditar(persona)} aria-label={`Editar ${nombreTipo} ${persona.nombres}`} title={`Editar ${nombreTipo}`}><PencilSimple aria-hidden="true" size={19} /></button>
    <button className="expediente-action" type="button" onClick={() => alReiniciarPin(persona)} aria-label={`Reiniciar PIN de ${persona.nombres}`} title="Reiniciar PIN" disabled={!persona.activo}><Key aria-hidden="true" size={18} /></button>
  </span>;
}

function BeneficiosPersona({ persona }: { persona: Persona }) {
  if (persona.tipo !== "estudiante") return <span className="person-benefits person-benefits--empty">No aplica</span>;
  const ruta = persona.rutaId ? persona.descripcionRuta ?? "Sin descripción" : "Sin ruta asignada";
  return <div className="person-benefits">
    <p><span>Comedor</span><strong>{persona.becado ? "Beneficiario" : "No beneficiario"}</strong></p>
    <p title={ruta}><span>Ruta</span><strong>{ruta}</strong></p>
  </div>;
}

function EncabezadoOrdenable({ columna, children, ordenarPor, direccion, alOrdenar }: Pick<Propiedades, "ordenarPor" | "direccion" | "alOrdenar"> & { columna: OrdenPersonas; children: string }) {
  const activo = ordenarPor === columna;
  return <th aria-sort={activo ? (direccion === "asc" ? "ascending" : "descending") : "none"}>
    <button className="person-sort" type="button" onClick={() => alOrdenar(columna)}>
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
    <div className="person-table" role="region" aria-label="Padrón de personas">
      <table>
        <colgroup><col className="person-column--nombre" /><col className="person-column--cedula" /><col className="person-column--tipo" /><col className="person-column--beneficios" /><col className="person-column--estado" /><col className="person-column--acciones" /></colgroup>
        <thead><tr><EncabezadoOrdenable columna="nombres" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Persona</EncabezadoOrdenable><EncabezadoOrdenable columna="cedula" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Cédula</EncabezadoOrdenable><EncabezadoOrdenable columna="tipo" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Tipo</EncabezadoOrdenable><th>Beneficios</th><EncabezadoOrdenable columna="estado" ordenarPor={ordenarPor} direccion={direccion} alOrdenar={alOrdenar}>Estado</EncabezadoOrdenable><th className="person-actions-heading">Acciones</th></tr></thead>
        <tbody>{personas.map((persona) => <tr key={persona.id}>
          <td><p className="person-name">{persona.nombres}</p></td>
          <td><span className="person-meta person-id">{persona.cedula ?? "Sin cédula"}</span></td>
          <td><Etiqueta tono={persona.tipo === "estudiante" ? "info" : "neutral"}>{persona.tipo === "estudiante" ? "Estudiante" : "Profesor"}</Etiqueta></td>
          <td><BeneficiosPersona persona={persona} /></td>
          <td><Etiqueta tono={persona.activo ? "exito" : "alerta"}>{persona.activo ? "Activa" : "Inactiva"}</Etiqueta></td>
          <td className="person-action-cell"><AccionExpediente persona={persona} alEditar={alEditar} alReiniciarPin={alReiniciarPin} /></td>
        </tr>)}</tbody>
      </table>
    </div>
    <div className="person-cards">{personas.map((persona) => <article className="person-card" key={persona.id}>
      <div className="person-card-header"><div><p className="person-name">{persona.nombres}</p><p className="person-meta">Cédula: {persona.cedula ?? "Sin cédula"}</p></div><AccionExpediente persona={persona} alEditar={alEditar} alReiniciarPin={alReiniciarPin} /></div>
      <div className="person-card-details"><Etiqueta tono={persona.tipo === "estudiante" ? "info" : "neutral"}>{persona.tipo === "estudiante" ? "Estudiante" : "Profesor"}</Etiqueta><Etiqueta tono={persona.activo ? "exito" : "alerta"}>{persona.activo ? "Activa" : "Inactiva"}</Etiqueta></div>
      <div className="person-card-benefits"><span>Beneficios</span><BeneficiosPersona persona={persona} /></div>
    </article>)}</div>
    <nav className="person-pagination" aria-label="Paginación de personas">
      <span>Mostrando {primera}–{ultima} de {total}</span>
      {totalPaginas > 1 && <div>
        <button className="button secondary" type="button" disabled={pagina === 1} onClick={() => alCambiarPagina(pagina - 1)}>Anterior</button>
        <span>Página {pagina} de {totalPaginas}</span>
        <button className="button secondary" type="button" disabled={pagina === totalPaginas} onClick={() => alCambiarPagina(pagina + 1)}>Siguiente</button>
      </div>}
    </nav>
  </>;
}
