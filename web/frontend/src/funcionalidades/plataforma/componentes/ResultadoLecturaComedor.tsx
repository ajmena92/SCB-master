import { useEffect, useState } from "react";
import { AlertTriangle, CheckCircle2, ScanBarcode, UserRound, XCircle } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import type { ResultadoOperacion } from "@/compartido/contratos/plataforma";
import { plataformaApi } from "../consultas/plataforma";

function presentacion(resultado?: ResultadoOperacion) {
  if (!resultado) {
    return {
      clase: "border-border bg-card text-foreground",
      etiqueta: "Lector listo",
      titulo: "Esperando una lectura",
      icono: ScanBarcode,
    };
  }
  if (resultado.estado === "aceptada" && resultado.advertencia) {
    return {
      clase: "border-amber-300 bg-amber-50 text-amber-950",
      etiqueta: "Acceso con advertencia",
      titulo: "Revisá la información",
      icono: AlertTriangle,
    };
  }
  if (resultado.estado === "aceptada") {
    return {
      clase: "border-success/40 bg-success/10 text-foreground",
      etiqueta: "Acceso permitido",
      titulo: "Ingreso registrado",
      icono: CheckCircle2,
    };
  }
  return {
    clase: "border-destructive/40 bg-destructive/10 text-destructive",
    etiqueta: resultado.resultado === "sin_reserva" ? "Reserva pendiente" : "Acceso denegado",
    titulo: resultado.resultado === "sin_reserva" ? "Requiere decisión" : "No registrar ingreso",
    icono: XCircle,
  };
}

export function ResultadoLecturaComedor({
  resultado,
  modoEstacion = false,
}: {
  resultado?: ResultadoOperacion;
  modoEstacion?: boolean;
}) {
  const [fotoUrl, setFotoUrl] = useState<string>();
  const personaId = resultado?.persona?.id;
  const visual = presentacion(resultado);
  const Icono = visual.icono;

  useEffect(() => {
    let vigente = true;
    let url: string | undefined;
    setFotoUrl(undefined);
    if (!personaId) return undefined;
    void plataformaApi.comedor
      .fotoPersona(personaId)
      .then((foto) => {
        url = URL.createObjectURL(foto);
        if (vigente) setFotoUrl(url);
      })
      .catch(() => undefined);
    return () => {
      vigente = false;
      if (url) URL.revokeObjectURL(url);
    };
  }, [personaId]);

  return (
    <section
      aria-live="polite"
      className={`overflow-hidden rounded-2xl border transition-[opacity,transform,colors] duration-200 ${visual.clase} ${modoEstacion ? "bg-opacity-95 shadow-2xl backdrop-blur-sm" : ""}`}
    >
      <div className={`grid gap-5 p-5 sm:grid-cols-[9rem_minmax(0,1fr)] sm:items-center sm:p-7 ${modoEstacion ? "min-h-44" : "min-h-60"}`}>
        <div className="flex aspect-square w-28 items-center justify-center self-center justify-self-center overflow-hidden rounded-2xl border border-current/15 bg-background/70 sm:w-36">
          {fotoUrl ? (
            <img src={fotoUrl} alt={`Fotografía de ${resultado?.persona?.nombres ?? "la persona"}`} className="h-full w-full object-cover" />
          ) : resultado?.persona ? (
            <UserRound className="h-16 w-16 opacity-60" aria-hidden="true" />
          ) : (
            <Icono className="h-16 w-16 text-primary" aria-hidden="true" />
          )}
        </div>
        <div className="min-w-0 text-center sm:text-left">
          <Badge variant="secondary" className="mb-3 font-bold uppercase tracking-[0.14em]">
            {visual.etiqueta}
          </Badge>
          <h3 className="font-display text-3xl font-black tracking-tight sm:text-4xl">
            {resultado?.persona?.nombres ?? visual.titulo}
          </h3>
          <p className="mt-2 text-base font-semibold sm:text-lg">
            {resultado?.mensaje ?? "Escaneá el código de barras del carnet digital."}
          </p>
          {resultado?.persona && (
            <div className="mt-4 flex flex-wrap justify-center gap-x-4 gap-y-2 text-sm font-semibold sm:justify-start">
              <span>{resultado.persona.cedula ?? "Sin cédula"}</span>
              <span className="capitalize">{resultado.persona.tipo}</span>
              {resultado.saldo !== undefined && <span>Saldo: {resultado.saldo} tiquetes</span>}
            </div>
          )}
        </div>
      </div>
    </section>
  );
}
