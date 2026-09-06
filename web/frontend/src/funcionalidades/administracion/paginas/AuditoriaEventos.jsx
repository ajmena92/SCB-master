import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { EstadoPanel } from "@/compartido/componentes/Estados";
import { useAuditoria } from "@/funcionalidades/administracion/hooks/useAuditoria";

const COLOR = {
  Confirmación: "bg-success text-white",
  Cancelación: "bg-muted text-foreground",
  Corrección: "bg-primary text-white",
  "PIN cambiado": "bg-secondary text-white",
  "PIN reiniciado": "bg-secondary text-white",
  "Parámetros del portal": "bg-secondary text-white",
};

const EVENT_LABEL = {
  ParametrosPortal: "Parámetros del portal",
  "Parametros del portal": "Parámetros del portal",
};

export function etiquetaEventoAuditoria(evento) {
  return EVENT_LABEL[evento] || evento;
}

export default function AuditoriaTab() {
  const { data: rows = [], isPending: loading } = useAuditoria();

  const fmt = (iso) => {
    try {
      return new Date(iso).toLocaleString("es-CR");
    } catch {
      return iso;
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="font-display text-2xl font-bold tracking-tight">Auditoría</h2>
        <p className="text-sm text-muted-foreground">
          Registro de confirmaciones, cancelaciones, correcciones, cambios de PIN y parámetros del
          portal.
        </p>
      </div>
      {loading ? (
        <EstadoPanel variante="carga">Cargando auditoría…</EstadoPanel>
      ) : (
        <div className="rounded-lg border bg-card">
          <div
            className="divide-y divide-border md:hidden"
            role="list"
            aria-label="Eventos de auditoría"
          >
            {rows.length === 0 ? (
              <EstadoPanel variante="vacio">No hay eventos para mostrar.</EstadoPanel>
            ) : (
              rows.map((r) => (
                <article key={r.IdAuditoria} className="space-y-3 p-4" role="listitem">
                  <div className="flex items-start justify-between gap-3">
                    <span className="text-sm text-foreground">{fmt(r.FechaEvento)}</span>
                    <Badge className={COLOR[etiquetaEventoAuditoria(r.Evento)] || ""}>
                      {etiquetaEventoAuditoria(r.Evento)}
                    </Badge>
                  </div>
                  <div>
                    <span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                      Estudiante
                    </span>
                    <p className="mt-1 text-sm text-foreground">{r.NombreEstudiante || "—"}</p>
                  </div>
                  <div>
                    <span className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                      Detalle
                    </span>
                    <p className="mt-1 break-words text-sm text-muted-foreground">
                      {r.Detalle || "—"}
                    </p>
                  </div>
                  <div className="text-xs text-muted-foreground">IP: {r.DireccionIp || "—"}</div>
                </article>
              ))
            )}
          </div>
          <div className="hidden overflow-x-auto md:block">
            <Table data-testid="auditoria-table">
              <TableHeader>
                <TableRow>
                  <TableHead>Fecha/Hora</TableHead>
                  <TableHead>Evento</TableHead>
                  <TableHead>Estudiante</TableHead>
                  <TableHead>Detalle</TableHead>
                  <TableHead>IP</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {rows.length === 0 ? (
                  <TableRow>
                    <TableCell
                      colSpan={5}
                      className="py-8 text-center text-sm text-muted-foreground"
                      role="status"
                    >
                      No hay eventos para mostrar.
                    </TableCell>
                  </TableRow>
                ) : (
                  rows.map((r) => (
                    <TableRow key={r.IdAuditoria} className="hover:bg-muted/40">
                      <TableCell className="whitespace-nowrap text-sm">
                        {fmt(r.FechaEvento)}
                      </TableCell>
                      <TableCell>
                        <Badge className={COLOR[etiquetaEventoAuditoria(r.Evento)] || ""}>
                          {etiquetaEventoAuditoria(r.Evento)}
                        </Badge>
                      </TableCell>
                      <TableCell>{r.NombreEstudiante || "—"}</TableCell>
                      <TableCell className="text-sm text-muted-foreground max-w-xs truncate">
                        {r.Detalle}
                      </TableCell>
                      <TableCell className="text-xs text-muted-foreground">
                        {r.DireccionIp || "—"}
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </div>
      )}
    </div>
  );
}
