import { FilePdf } from "@phosphor-icons/react";
import type { FormEvent } from "react";
import {
  AlertDialog,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import {
  Dialog,
  DialogBody,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import type { AnioLectivo, Persona } from "@/compartido/contratos/plataforma";
import { Campo } from "./ElementosComunes";

export type ConfirmacionPin =
  | { tipo: "individual"; persona: Persona }
  | { tipo: "seccion"; anioLectivoId: number; seccion: string }
  | undefined;

export function ConfirmacionReinicioPin({
  confirmacion,
  reiniciando,
  cantidadEstudiantes,
  alCerrar,
  alConfirmar,
}: {
  confirmacion: ConfirmacionPin;
  reiniciando: boolean;
  cantidadEstudiantes: number;
  alCerrar: () => void;
  alConfirmar: () => void;
}) {
  return (
    <AlertDialog
      open={Boolean(confirmacion)}
      onOpenChange={(abierto) => !abierto && !reiniciando && alCerrar()}
    >
      <AlertDialogContent aria-busy={reiniciando}>
        <AlertDialogHeader>
          <AlertDialogTitle>
            {reiniciando ? "Generando credenciales" : "¿Reiniciar el PIN?"}
          </AlertDialogTitle>
          <AlertDialogDescription>
            {reiniciando
              ? confirmacion?.tipo === "seccion"
                ? `Estamos reiniciando y preparando la lista de PIN para ${cantidadEstudiantes} estudiantes de la sección ${confirmacion.seccion}.`
                : "Estamos generando el PIN temporal."
              : confirmacion?.tipo === "individual"
                ? `Se generará un PIN temporal nuevo para ${confirmacion.persona.nombres}. El anterior dejará de funcionar.`
                : `Se generará un PIN temporal nuevo para ${cantidadEstudiantes} estudiantes activos de la sección ${confirmacion?.seccion ?? ""}. Los PIN anteriores dejarán de funcionar.`}
          </AlertDialogDescription>
        </AlertDialogHeader>
        {reiniciando ? (
          <div className="pin-processing" role="status">
            <span aria-hidden="true" className="pin-processing-spinner" />
            <span>Espere un momento. No cierre esta ventana.</span>
          </div>
        ) : (
          <AlertDialogFooter>
            <AlertDialogCancel>Cancelar</AlertDialogCancel>
            <button className="button primary" type="button" onClick={alConfirmar}>
              Reiniciar PIN
            </button>
          </AlertDialogFooter>
        )}
      </AlertDialogContent>
    </AlertDialog>
  );
}

export function DialogoOperacionesPin({
  abierto,
  anios,
  anioLectivoId,
  seccion,
  secciones,
  cargandoSecciones,
  cantidadEstudiantes,
  cargandoResumen,
  pendiente,
  alCambiarAbierto,
  alCambiarAnio,
  alCambiarSeccion,
  alEnviar,
}: {
  abierto: boolean;
  anios: AnioLectivo[];
  anioLectivoId?: number;
  seccion: string;
  secciones: string[];
  cargandoSecciones: boolean;
  cantidadEstudiantes?: number;
  cargandoResumen: boolean;
  pendiente: boolean;
  alCambiarAbierto: (abierto: boolean) => void;
  alCambiarAnio: (id: number | undefined) => void;
  alCambiarSeccion: (seccion: string) => void;
  alEnviar: (evento: FormEvent<HTMLFormElement>) => void;
}) {
  return (
    <Dialog open={abierto} onOpenChange={alCambiarAbierto}>
      <DialogContent className="pin-operations-dialog sm:max-w-xl">
        <DialogHeader>
          <DialogTitle>Operaciones PIN por sección</DialogTitle>
          <DialogDescription>
            Genere nuevas credenciales temporales y prepare el reporte para entregar los PIN de una
            sección.
          </DialogDescription>
        </DialogHeader>
        <DialogBody>
          <form id="operaciones-pin" className="pin-operations-form" onSubmit={alEnviar}>
            <Campo etiqueta="Año lectivo">
              <select
                name="anioLectivoIdPines"
                required
                value={anioLectivoId ?? ""}
                onChange={(evento) => alCambiarAnio(Number(evento.target.value) || undefined)}
              >
                {anios.map((anio) => (
                  <option key={anio.id} value={anio.id}>
                    {anio.anio}
                    {anio.vigente ? " (vigente)" : ""}
                  </option>
                ))}
              </select>
            </Campo>
            <Campo etiqueta="Sección">
              <select
                name="seccionPines"
                required
                value={seccion}
                disabled={!anioLectivoId || cargandoSecciones}
                onChange={(evento) => alCambiarSeccion(evento.target.value)}
              >
                <option value="">
                  {cargandoSecciones ? "Cargando secciones…" : "Seleccione una sección"}
                </option>
                {secciones.map((item) => (
                  <option key={item} value={item}>
                    {item}
                  </option>
                ))}
              </select>
            </Campo>
            <div className="pin-section-summary" aria-live="polite">
              <FilePdf aria-hidden="true" size={19} />
              <span>
                {seccion
                  ? cargandoResumen
                    ? "Verificando sección…"
                    : `${cantidadEstudiantes ?? 0} estudiantes activos recibirán un PIN nuevo`
                  : "Seleccione una sección para verificar el grupo"}
              </span>
            </div>
            <p className="pin-warning" role="note">
              <strong>Atención:</strong> al continuar se reiniciarán los PIN de todos los
              estudiantes activos de esta sección. Los PIN anteriores dejarán de funcionar.
            </p>
          </form>
        </DialogBody>
        <DialogFooter>
          <button
            className="button secondary"
            type="button"
            onClick={() => alCambiarAbierto(false)}
          >
            Cancelar
          </button>
          <button
            className="button primary"
            type="submit"
            form="operaciones-pin"
            disabled={pendiente || !seccion || !cantidadEstudiantes}
          >
            Continuar
          </button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
