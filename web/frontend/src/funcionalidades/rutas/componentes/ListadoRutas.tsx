import { Pencil, Route as IconoRuta } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { EstadoPanel } from "@/compartido/componentes/Estados";
import type { Ruta } from "../consultas/rutas";

interface PropiedadesListadoRutas {
  rutas: Ruta[];
  onEditar: (ruta: Ruta) => void;
  onDesactivar: (ruta: Ruta) => void;
}
export function ListadoRutas({ rutas, onEditar, onDesactivar }: PropiedadesListadoRutas) {
  if (!rutas.length)
    return (
      <div data-testid="rutas-empty">
        <EstadoPanel variante="vacio" titulo="No hay rutas para mostrar">
          Probá otra búsqueda o agregá una nueva ruta.
        </EstadoPanel>
      </div>
    );
  return (
    <div className="grid gap-4 md:grid-cols-2 xl:grid-cols-3" data-testid="rutas-list">
      {rutas.map((ruta) => (
        <article
          key={ruta.idRuta}
          className={`rounded-xl border bg-card p-4 shadow-[0_4px_14px_rgb(15_72_131_/_0.06)] transition-colors duration-200 hover:border-primary/35 ${ruta.activo ? "" : "opacity-65"}`}
          data-testid={`ruta-${ruta.idRuta}`}
        >
          <div className="flex items-start justify-between gap-3">
            <div className="flex min-w-0 items-start gap-3">
              <span
                className="mt-1 flex h-9 w-9 shrink-0 items-center justify-center rounded-full border-2"
                style={{
                  backgroundColor: ruta.colorCarnetHex || "rgb(var(--border))",
                  borderColor:
                    ruta.colorCarnetHex === "#FFFFFF"
                      ? "rgb(var(--border))"
                      : ruta.colorCarnetHex || "rgb(var(--border))",
                }}
                role="img"
                aria-label={`Color de la ruta ${ruta.codigo}`}
              >
                <IconoRuta className="h-4 w-4 text-foreground" />
              </span>
              <div className="min-w-0">
                <p className="text-xs font-semibold tracking-wide text-primary">
                  Ruta {ruta.codigo}
                </p>
                <p className="text-sm font-medium leading-relaxed text-foreground">
                  {ruta.descripcion}
                </p>
              </div>
            </div>
            <Badge
              className="text-[11px] font-medium"
              variant={ruta.activo ? "secondary" : "outline"}
            >
              {ruta.activo ? "Activa" : "Inactiva"}
            </Badge>
          </div>
          <div className="mt-4 flex flex-col gap-3 border-t pt-3 text-xs text-muted-foreground sm:flex-row sm:items-center sm:justify-between">
            <span>
              {ruta.estudiantesAsignados} estudiante{ruta.estudiantesAsignados === 1 ? "" : "s"}
            </span>
            <span className="hidden items-center gap-1 sm:inline-flex">
              <i
                className="h-3 w-3 rounded-full border"
                style={{ backgroundColor: ruta.colorCarnetHex ?? undefined }}
              />
              Color TE-01
            </span>
            <div className="flex w-full gap-2 sm:w-auto">
              <Button
                className="min-h-11 flex-1 sm:flex-none"
                variant="ghost"
                size="sm"
                onClick={() => onEditar(ruta)}
                disabled={ruta.codigo === "0"}
                data-testid={`ruta-editar-${ruta.idRuta}`}
              >
                <Pencil className="mr-1 h-3.5 w-3.5" /> Editar ruta
              </Button>
              {ruta.activo && ruta.codigo !== "0" && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="min-h-11 flex-1 text-destructive hover:text-destructive sm:flex-none"
                  onClick={() => onDesactivar(ruta)}
                  data-testid={`ruta-desactivar-${ruta.idRuta}`}
                >
                  Desactivar
                </Button>
              )}
            </div>
          </div>
        </article>
      ))}
    </div>
  );
}
