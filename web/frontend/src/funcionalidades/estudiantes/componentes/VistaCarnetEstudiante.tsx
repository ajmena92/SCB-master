import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { EstadoPanel } from "@/compartido/componentes/Estados";
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
      className={`rounded-2xl border bg-card p-4 shadow-[0_8px_30px_rgb(70_73_180_/_0.12)] sm:p-5 ${clase}`}
      data-testid="student-card-panel"
    >
      {fotoDisponible === false && (
        <div className="mb-4 flex flex-wrap items-start justify-between gap-3">
          <Badge variant="secondary">Carnet provisional</Badge>
        </div>
      )}
      {estaCargando && !datos && (
        <EstadoPanel variante="carga">Generando tu carné digital…</EstadoPanel>
      )}
      {mensajeError && (
        <EstadoPanel
          variante="error"
          accion={
            reintentar && (
              <Button type="button" variant="outline" size="sm" onClick={reintentar}>
                Reintentar
              </Button>
            )
          }
        >
          {mensajeError}
        </EstadoPanel>
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
        <p className="mt-4 text-sm text-muted-foreground">
          Fotografía pendiente de carga administrativa.
        </p>
      )}
    </section>
  );
}
