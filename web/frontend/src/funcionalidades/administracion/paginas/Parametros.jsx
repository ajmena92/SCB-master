import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { useParametros } from "@/funcionalidades/administracion/hooks/useParametros";
import { CheckCircle2, Loader2, Settings2 } from "lucide-react";

export default function ParametrosTab() {
  const {
    parametros,
    horariosEditables,
    loading,
    loadError,
    error,
    success,
    saving,
    actualizarHorario,
    actualizarMinutos,
    guardar,
  } = useParametros();

  return (
    <section className="max-w-3xl space-y-6" aria-labelledby="parametros-title">
      <div>
        <h2 id="parametros-title" className="font-display text-2xl font-bold tracking-tight">
          Parámetros
        </h2>
        <p className="text-sm text-muted-foreground">
          Configuración exclusiva del portal de comedor. Los cambios quedan registrados en
          Auditoría.
        </p>
      </div>

      {(error || loadError) && (
        <Alert variant="destructive" role="alert" data-testid="parametros-error">
          <AlertTitle>No se pudieron guardar los parámetros</AlertTitle>
          <AlertDescription>{error || loadError}</AlertDescription>
        </Alert>
      )}
      {success && (
        <Alert
          className="border-success/40 bg-success/10"
          role="status"
          data-testid="parametros-success"
        >
          <CheckCircle2 className="h-4 w-4 text-success" />
          <AlertTitle>Parámetros actualizados</AlertTitle>
          <AlertDescription>{success}</AlertDescription>
        </Alert>
      )}

      {loading ? (
        <Skeleton className="h-64 w-full rounded-lg" data-testid="parametros-loading" />
      ) : (
        <div className="space-y-5">
          <div className="rounded-lg border bg-card p-5">
            <Label htmlFor="minutos-aviso">Aviso previo al cierre (minutos)</Label>
            <p className="mt-1 text-sm text-muted-foreground">
              El portal destacará el tiempo restante durante este periodo.
            </p>
            <Input
              id="minutos-aviso"
              data-testid="parametros-minutos-aviso"
              className="mt-3 max-w-xs"
              type="number"
              min="1"
              max="120"
              step="1"
              value={parametros.minutosAvisoPrevio}
              onChange={(event) => actualizarMinutos(event.target.value)}
            />
          </div>

          <div className="space-y-4">
            <div>
              <h3 className="font-display text-lg font-bold">Hora límite por horario</h3>
              <p className="text-sm text-muted-foreground">
                El portal conservará la hora de apertura vigente de cada horario. Los horarios
                inactivos son solo de consulta.
              </p>
            </div>
            {parametros.horarios.length === 0 ? (
              <Alert>
                <AlertTitle>Sin horarios</AlertTitle>
                <AlertDescription>
                  No hay horarios disponibles para consultar o configurar.
                </AlertDescription>
              </Alert>
            ) : (
              parametros.horarios.map((horario) => (
                <div
                  key={horario.idHorario}
                  className={`rounded-lg border p-5 ${horario.activo === false ? "bg-muted/40" : "bg-card"}`}
                  data-testid={`parametro-horario-${horario.idHorario}`}
                >
                  <div className="flex flex-col justify-between gap-4 sm:flex-row sm:items-end">
                    <div>
                      <div className="flex items-center gap-2">
                        <h4 className="font-semibold">{horario.descripcion}</h4>
                        {horario.activo === false && (
                          <Badge
                            variant="secondary"
                            data-testid={`parametro-horario-inactivo-${horario.idHorario}`}
                          >
                            Inactivo
                          </Badge>
                        )}
                      </div>
                      {horario.horaApertura && (
                        <p className="text-sm text-muted-foreground">
                          Apertura: {horario.horaApertura}
                        </p>
                      )}
                    </div>
                    <div className="w-full sm:w-48">
                      <Label htmlFor={`hora-limite-${horario.idHorario}`}>Hora límite</Label>
                      <Input
                        id={`hora-limite-${horario.idHorario}`}
                        data-testid={`parametro-hora-limite-${horario.idHorario}`}
                        className="mt-2"
                        type="time"
                        disabled={horario.activo === false}
                        value={horario.horaLimite}
                        onChange={(event) =>
                          actualizarHorario(horario.idHorario, event.target.value)
                        }
                      />
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>

          <Button
            data-testid="parametros-guardar"
            onClick={guardar}
            disabled={saving || horariosEditables.length === 0}
          >
            {saving ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" /> Guardando…
              </>
            ) : (
              <>
                <Settings2 className="mr-2 h-4 w-4" /> Guardar parámetros
              </>
            )}
          </Button>
        </div>
      )}
    </section>
  );
}
