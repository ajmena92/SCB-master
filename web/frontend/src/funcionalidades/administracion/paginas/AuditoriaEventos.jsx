import { useEffect } from "react";
import { useQuery } from "@tanstack/react-query";
import { api, errMsg } from "@/lib/api";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { toast } from "sonner";

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
  const {
    data: rows = [],
    error,
    isPending: loading,
  } = useQuery({
    queryKey: ["admin", "auditoria"],
    queryFn: async () => (await api.get("/v1/auditoria/eventos")).data,
  });

  useEffect(() => {
    if (error) toast.error(errMsg(error));
  }, [error]);

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
        <Skeleton className="h-64 w-full rounded-lg" />
      ) : (
        <div className="bg-card border rounded-lg overflow-x-auto">
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
                  <TableCell colSpan={5} className="text-center text-muted-foreground py-8">
                    Sin eventos
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
      )}
    </div>
  );
}
