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
    <header className="page-heading">
      <div>
        <h1>{titulo}</h1>
        <p>{descripcion}</p>
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
    <div className={`notice notice--${tipo}`} role={tipo === "error" ? "alert" : "status"}>
      {children}
    </div>
  );
}

export function EstadoCarga() {
  return (
    <p className="empty-state" role="status">
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
  if (!filas.length) return <p className="empty-state">{vacio}</p>;
  return (
    <div className="table-scroll">
      <table>
        <thead>
          <tr>
            {columnas.map((columna) => (
              <th key={columna}>{columna}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {filas.map((fila) => (
            <tr key={JSON.stringify(fila)}>
              {fila.map((celda, indice) => (
                <td key={columnas[indice]}>{celda}</td>
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
    <label className="field">
      <span>{etiqueta}</span>
      {children}
    </label>
  );
}
