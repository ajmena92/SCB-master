import { useCalendario } from "@/funcionalidades/administracion/hooks/useCalendario";
import { Skeleton } from "@/components/ui/skeleton";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { ChevronLeft, ChevronRight, Replace as ReplaceIcon, CalendarRange } from "lucide-react";

const MESES = [
  "Enero",
  "Febrero",
  "Marzo",
  "Abril",
  "Mayo",
  "Junio",
  "Julio",
  "Agosto",
  "Septiembre",
  "Octubre",
  "Noviembre",
  "Diciembre",
];
const DIAS = ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes"];

export default function CalendarioTab() {
  const { anio, mes, hoyISO, dias, loading, mover, semanas } = useCalendario();

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-4">
        <div>
          <h2 className="font-display text-2xl font-bold tracking-tight flex items-center gap-2">
            <CalendarRange className="h-6 w-6 text-primary" /> Calendario mensual de menús
          </h2>
          <p className="text-sm text-muted-foreground">
            Vista de lunes a viernes. Las sustituciones prevalecen sobre la plantilla semanal.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline" size="icon" data-testid="cal-prev" onClick={() => mover(-1)}>
            <ChevronLeft className="h-4 w-4" />
          </Button>
          <span className="font-display font-bold w-40 text-center" data-testid="cal-titulo">
            {MESES[mes - 1]} {anio}
          </span>
          <Button variant="outline" size="icon" data-testid="cal-next" onClick={() => mover(1)}>
            <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {loading ? (
        <Skeleton className="h-96 w-full rounded-lg" />
      ) : (
        <div className="overflow-x-auto">
          <div className="min-w-[900px]">
            <div className="grid grid-cols-5 gap-3 mb-3">
              {DIAS.map((d) => (
                <div
                  key={d}
                  className="text-xs font-bold uppercase tracking-widest text-muted-foreground text-center"
                >
                  {d}
                </div>
              ))}
            </div>
            {semanas.map((sem) => (
              <div key={sem} className="grid grid-cols-5 gap-3 mb-3">
                {[1, 2, 3, 4, 5].map((dia) => {
                  const cell = dias.find((d) => d.semanaMes === sem && d.diaSemana === dia);
                  if (!cell)
                    return (
                      <div
                        key={dia}
                        className="rounded-lg border border-dashed bg-muted/20 min-h-[110px]"
                      />
                    );
                  const esHoy = cell.fecha === hoyISO;
                  return (
                    <div
                      key={dia}
                      data-testid={`cal-dia-${cell.fecha}`}
                      className={`rounded-lg border p-3 min-h-[110px] transition-shadow hover:shadow-md ${esHoy ? "border-primary ring-1 ring-primary bg-primary/5" : "bg-card"}`}
                    >
                      <div className="flex items-center justify-between">
                        <span className={`text-sm font-bold ${esHoy ? "text-primary" : ""}`}>
                          {cell.dia}
                        </span>
                        {cell.origen === "sustitucion" && (
                          <Badge className="bg-primary text-white text-[10px] gap-1">
                            <ReplaceIcon className="h-2.5 w-2.5" />
                            Sust.
                          </Badge>
                        )}
                      </div>
                      {cell.titulo ? (
                        <>
                          <p className="text-sm font-semibold mt-2 leading-tight line-clamp-3">
                            {cell.titulo}
                          </p>
                          <p className="text-xs text-muted-foreground mt-1">
                            {cell.componentes} comp.
                          </p>
                        </>
                      ) : (
                        <p className="text-xs text-muted-foreground mt-2">Sin menú</p>
                      )}
                    </div>
                  );
                })}
              </div>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
