import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import { api, errMsg } from "@/lib/api";
import { CheckCircle2, Loader2, Settings2 } from "lucide-react";

const TIME_PATTERN = /^([01]\d|2[0-3]):[0-5]\d$/;

function field(item, camel, pascal) {
  return item?.[camel] ?? item?.[pascal];
}

export function normalizeParametros(data) {
  const horarios = data?.horarios ?? data?.Horarios ?? [];
  return {
    minutosAvisoPrevio: String(
      data?.minutosAvisoPrevio ??
        data?.MinutosAvisoPrevio ??
        data?.minutosAviso ??
        data?.MinutosAviso ??
        "15",
    ),
    horarios: horarios.map((horario) => {
      const activo = field(horario, "activo", "Activo");
      return {
        idHorario: field(horario, "idHorario", "IdHorario"),
        descripcion: field(horario, "descripcion", "Descripcion") || "Horario",
        horaApertura:
          field(horario, "horaInicio", "HoraInicio") ??
          field(horario, "horaApertura", "HoraApertura"),
        horaLimite: field(horario, "horaLimite", "HoraLimite") || "",
        activo: activo !== false && activo !== 0,
      };
    }),
  };
}

export function validateParametros({ minutosAvisoPrevio, horarios }) {
  const horariosEditables = horarios.filter((horario) => horario.activo !== false);
  const minutes = Number(minutosAvisoPrevio);
  if (!Number.isInteger(minutes) || minutes < 1 || minutes > 120) {
    return "El aviso previo debe ser un número entre 1 y 120 minutos.";
  }
  if (horariosEditables.some((horario) => !TIME_PATTERN.test(horario.horaLimite))) {
    return "Cada hora límite debe tener el formato HH:mm.";
  }
  if (
    horariosEditables.some(
      (horario) => horario.horaApertura && horario.horaLimite <= horario.horaApertura,
    )
  ) {
    return "La hora límite debe ser posterior a la hora de apertura de cada horario.";
  }
  return "";
}

export default function ParametrosTab() {
  const [parametrosEditados, setParametrosEditados] = useState(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const {
    data,
    error: loadError,
    isPending: loading,
  } = useQuery({
    queryKey: ["admin", "parametros"],
    queryFn: async () => (await api.get("/v1/parametros")).data,
  });
  const parametros = parametrosEditados ?? normalizeParametros(data);

  const actualizarHorario = (idHorario, horaLimite) => {
    setParametrosEditados((actual) => {
      const base = actual ?? normalizeParametros(data);
      return {
        ...base,
        horarios: base.horarios.map((horario) =>
          horario.idHorario === idHorario ? { ...horario, horaLimite } : horario,
        ),
      };
    });
  };

  const horariosEditables = parametros.horarios.filter((horario) => horario.activo !== false);

  const guardar = async () => {
    const validation = validateParametros(parametros);
    if (validation) {
      setSuccess("");
      setError(validation);
      return;
    }
    setSaving(true);
    setError("");
    setSuccess("");
    try {
      await api.put("/v1/parametros", {
        minutosAvisoPrevio: Number(parametros.minutosAvisoPrevio),
        horarios: horariosEditables.map(({ idHorario, horaLimite }) => ({ idHorario, horaLimite })),
      });
      setSuccess(
        "Parámetros guardados. El portal aplicará los cambios dinámicamente en la próxima consulta o acción del estudiante.",
      );
    } catch (e) {
      setError(errMsg(e));
    } finally {
      setSaving(false);
    }
  };

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
          <AlertDescription>{error || errMsg(loadError)}</AlertDescription>
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
              onChange={(event) =>
                setParametrosEditados((actual) => ({
                  ...(actual ?? normalizeParametros(data)),
                  minutosAvisoPrevio: event.target.value,
                }))
              }
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
