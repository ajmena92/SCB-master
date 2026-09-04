import { Ban, CookingPot, Ellipsis, Replace } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { DIAS, type DiaCalendario } from "../calendario";

interface PropiedadesGrillaCalendarioMenu {
  semanas: Array<Array<DiaCalendario | null>>;
  hoy: string;
  onSeleccionar: (dia: DiaCalendario) => void;
}
function estadoDia(dia: DiaCalendario) {
  if (dia.origen === "sustitucion") return "Sustitución";
  if (dia.origen === "cerrado") return "Cerrado";
  return dia.origen === "sin_menu" ? "Sin menú" : "Menú";
}
function CeldaCalendario({
  dia,
  hoy,
  onSeleccionar,
}: {
  dia: DiaCalendario;
  hoy: string;
  onSeleccionar: (dia: DiaCalendario) => void;
}) {
  const esHoy = dia.fecha === hoy;
  const estado = estadoDia(dia);
  return (
    <article
      role={dia.esLectivo ? "button" : undefined}
      tabIndex={dia.esLectivo ? 0 : undefined}
      onClick={() => dia.esLectivo && onSeleccionar(dia)}
      onKeyDown={(evento) => {
        if (dia.esLectivo && (evento.key === "Enter" || evento.key === " ")) {
          evento.preventDefault();
          onSeleccionar(dia);
        }
      }}
      className={`relative rounded-xl border px-3 py-3 sm:min-h-28 sm:p-3 ${dia.esLectivo ? "cursor-pointer transition-all duration-200 hover:-translate-y-0.5 hover:border-primary/50 hover:shadow-md hover:shadow-primary/10 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-primary" : "border-dashed border-border/50 bg-muted/[0.07] sm:px-2"} ${esHoy ? "border-primary bg-primary/[0.06] ring-1 ring-primary" : ""} ${dia.origen === "cerrado" ? "border-destructive/30 bg-destructive/5" : ""} ${dia.origen === "sustitucion" ? "border-amber-500/60 bg-amber-500/10 hover:border-amber-500" : ""}`}
    >
      <div className="flex items-center justify-between gap-2">
        <strong
          className={`flex h-7 min-w-7 items-center text-sm font-bold tabular-nums ${esHoy ? "justify-center rounded-full bg-primary text-primary-foreground" : "text-foreground"}`}
        >
          {dia.diaMes}
        </strong>
        {dia.esLectivo && (
          <span className="flex items-center gap-1">
            <Badge
              className="h-6 border-primary/15 bg-primary/10 px-2 text-[10px] font-bold text-primary hover:bg-primary/10"
              variant={dia.origen === "cerrado" ? "destructive" : "secondary"}
            >
              {dia.semana ? `S${dia.semana}` : estado}
            </Badge>
            <Ellipsis aria-label="Ver detalle del día" className="h-4 w-4 text-muted-foreground" />
          </span>
        )}
      </div>
      {dia.esLectivo && (
        <>
          <p className="mt-1.5 line-clamp-2 text-sm font-semibold leading-snug text-foreground">
            {dia.titulo ?? "Sin menú configurado"}
          </p>
          <p
            className={`mt-2 flex items-center gap-1 text-xs ${dia.origen === "sustitucion" ? "font-semibold text-amber-700 dark:text-amber-300" : "text-muted-foreground"}`}
          >
            {dia.origen === "sustitucion" ? (
              <Replace className="h-3 w-3" />
            ) : dia.origen === "cerrado" ? (
              <Ban className="h-3 w-3" />
            ) : (
              <CookingPot className="h-3 w-3" />
            )}
            {dia.origen === "sustitucion"
              ? "Sustitución aplicada"
              : dia.semana
                ? `Semana del mes · ${dia.semana}`
                : estado}
          </p>
        </>
      )}
    </article>
  );
}
export function GrillaCalendarioMenu({
  semanas,
  hoy,
  onSeleccionar,
}: PropiedadesGrillaCalendarioMenu) {
  return (
    <div className="rounded-2xl border bg-card p-2 shadow-sm shadow-primary/5 sm:p-4">
      <div className="space-y-3">
        <div className="hidden grid-cols-[repeat(5,minmax(0,1fr))_minmax(4.5rem,.58fr)_minmax(4.5rem,.58fr)] gap-2 text-center sm:grid">
          {DIAS.map((nombre, indice) => (
            <p
              key={nombre}
              className={`text-center text-xs font-bold uppercase tracking-wide ${indice > 4 ? "text-muted-foreground/60" : "text-muted-foreground"}`}
            >
              {nombre}
            </p>
          ))}
        </div>
        {semanas.map((semana, indice) => (
          <section
            key={indice}
            aria-label={`Semana calendario ${indice + 1}`}
            className="grid grid-cols-1 gap-2 border-b border-border/60 pb-3 last:border-0 last:pb-0 sm:grid-cols-[repeat(5,minmax(0,1fr))_minmax(4.5rem,.58fr)_minmax(4.5rem,.58fr)] sm:gap-2 sm:border-0 sm:pb-0"
          >
            <p className="px-1 text-xs font-semibold uppercase tracking-wide text-muted-foreground sm:hidden">
              Semana del calendario {indice + 1}
            </p>
            {semana.map((dia, posicion) =>
              dia ? (
                <CeldaCalendario
                  key={dia.fecha}
                  dia={dia}
                  hoy={hoy}
                  onSeleccionar={onSeleccionar}
                />
              ) : (
                <div key={posicion} aria-hidden="true" className="hidden min-h-28 sm:block" />
              ),
            )}
          </section>
        ))}
      </div>
      <div className="mt-3 flex flex-wrap gap-x-5 gap-y-2 border-t pt-3 text-xs text-muted-foreground">
        <span className="inline-flex items-center gap-1">
          <CookingPot className="h-3.5 w-3.5" /> Menú
        </span>
        <span className="inline-flex items-center gap-1">
          <Replace className="h-3.5 w-3.5" /> Sustitución
        </span>
        <span className="inline-flex items-center gap-1">
          <Ban className="h-3.5 w-3.5" /> Cierre institucional
        </span>
        <span className="inline-flex items-center gap-1">
          <Ellipsis className="h-3.5 w-3.5" /> Ver detalle
        </span>
      </div>
    </div>
  );
}
