import { useState } from "react";
import { IdCard } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { TarjetaCarnet } from "./TarjetaCarnet";
import type { DatosCarnet } from "./accionesCarnet";

export function VistaCarnetEstudiante({
  sesion,
  carnet,
  idEstudiante,
  tieneFoto,
  datosCarnet = null,
  tipoPersona,
  cargando = false,
  error = "",
  alReintentar,
  clase = "",
}: {
  sesion?: { usuario?: Record<string, unknown> } | null;
  carnet?: {
    datos?: DatosCarnet | null;
    cargando?: boolean;
    error?: string;
    recargar?: () => void;
  };
  idEstudiante?: number;
  tieneFoto?: boolean;
  datosCarnet?: DatosCarnet | null;
  tipoPersona?: "estudiante" | "profesor";
  cargando?: boolean;
  error?: string;
  alReintentar?: () => void;
  clase?: string;
}) {
  const [version] = useState(() => Date.now());
  const datos = datosCarnet ?? carnet?.datos ?? null;
  const persona = tipoPersona ?? datos?.tipoPersona ?? "estudiante";
  const id = idEstudiante ?? Number(sesion?.usuario?.idEstudiante);
  const fotoDisponible = Boolean(
    tieneFoto ?? sesion?.usuario?.TieneFoto ?? sesion?.usuario?.tieneFoto ?? datos?.tieneFoto,
  );
  const estaCargando = cargando || carnet?.cargando;
  const mensajeError = error || carnet?.error || "";
  const reintentar = alReintentar || carnet?.recargar;

  return (
    <section
      className={`rounded-2xl border bg-card p-5 shadow-[0_8px_30px_rgb(70_73_180_/_0.12)] ${clase}`}
      data-testid="student-card-panel"
    >
      <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
        <div className="flex items-start gap-3">
          <div className="rounded-xl bg-primary/10 p-3 text-primary">
            <IdCard className="h-5 w-5" />
          </div>
          <div>
            <h2 className="font-display text-xl font-bold">Mi carnet digital</h2>
            <p className="text-sm text-muted-foreground">
              Presentalo desde tu teléfono para leer el código en el comedor.
            </p>
          </div>
        </div>
        {fotoDisponible === false && <Badge variant="secondary">Carnet provisional</Badge>}
      </div>
      {estaCargando && !datos && (
        <div className="space-y-3">
          <Skeleton className="mx-auto h-[31rem] w-full max-w-[23rem] rounded-[1.75rem]" />
          <p className="text-center text-sm font-medium text-muted-foreground">
            Generando tu carnet digital…
          </p>
        </div>
      )}
      {mensajeError && (
        <div
          role="alert"
          className="space-y-3 rounded-xl bg-destructive/10 p-4 text-sm font-medium text-destructive"
        >
          <p>{mensajeError}</p>
          {reintentar && (
            <Button type="button" variant="outline" size="sm" onClick={reintentar}>
              Reintentar
            </Button>
          )}
        </div>
      )}
      {!estaCargando && !mensajeError && (datos || id) && (
        <TarjetaCarnet
          datosCarnet={datos ?? { idEstudiante: id || undefined }}
          tipoPersona={persona}
          tieneFoto={fotoDisponible}
          versionFoto={version}
        />
      )}
      {fotoDisponible === false && (
        <p className="mt-4 text-xs text-muted-foreground">
          El administrador todavía debe cargar tu fotografía.
        </p>
      )}
    </section>
  );
}
