import { Button } from "@/components/ui/button";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import { FileText } from "lucide-react";
import type { SeccionEstudiante } from "@/funcionalidades/estudiantes/modelo/contratos";

type ControlesProps = {
  turno: string;
  seccion: string;
  secciones: SeccionEstudiante[];
  cargandoSecciones: boolean;
  cargandoReporte: boolean;
  seccionSeleccionada?: SeccionEstudiante;
  alCambiarTurno: (turno: string) => void;
  alCambiarSeccion: (seccion: string) => void;
  alGenerar: () => void;
};

export function ControlesReportePines({
  turno,
  seccion,
  secciones,
  cargandoSecciones,
  cargandoReporte,
  seccionSeleccionada,
  alCambiarTurno,
  alCambiarSeccion,
  alGenerar,
}: ControlesProps) {
  return (
    <div className="space-y-4 rounded-lg border bg-card p-5">
      <div className="flex items-start gap-3">
        <FileText className="mt-0.5 h-5 w-5 text-primary" />
        <div>
          <h3 className="font-display font-bold">Reporte de PIN por sección</h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Seleccioná una sección para generar nuevos PIN temporales e imprimirlos o guardarlos
            como PDF.
          </p>
        </div>
      </div>
      <div className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <div className="w-full space-y-2 sm:max-w-xs">
          <label htmlFor="pin-reporte-turno" className="text-sm font-medium">
            Turno
          </label>
          <select
            id="pin-reporte-turno"
            data-testid="pin-reporte-turno"
            value={turno}
            onChange={(evento) => alCambiarTurno(evento.target.value)}
            disabled={cargandoReporte}
            className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm"
          >
            <option value="diurno">Diurno</option>
            <option value="nocturno">Nocturno</option>
          </select>
        </div>
        <div className="w-full space-y-2 sm:max-w-sm">
          <label htmlFor="pin-reporte-seccion" className="text-sm font-medium">
            Sección
          </label>
          <select
            id="pin-reporte-seccion"
            data-testid="pin-reporte-seccion"
            value={seccion}
            onChange={(evento) => alCambiarSeccion(evento.target.value)}
            disabled={cargandoSecciones || cargandoReporte}
            className="flex h-10 w-full rounded-md border border-input bg-transparent px-3 py-2 text-sm"
          >
            <option value="">Seleccioná una sección</option>
            {secciones.map((item) => (
              <option
                key={item.seccion ?? "__SIN_SECCION__"}
                value={item.seccion ?? "__SIN_SECCION__"}
              >
                {item.etiqueta} ({item.total})
              </option>
            ))}
          </select>
        </div>
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button
              type="button"
              data-testid="generate-pin-reporte"
              disabled={!seccion || cargandoSecciones || cargandoReporte}
            >
              <FileText className="mr-2 h-4 w-4" /> Generar reporte
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>¿Resetear todos los PIN?</AlertDialogTitle>
              <AlertDialogDescription>
                Se resetearán todos los PIN de los {seccionSeleccionada?.total || "estudiantes"}{" "}
                estudiante(s) de la sección <strong>{seccionSeleccionada?.etiqueta}</strong> del
                turno <strong>{turno}</strong>. Los PIN anteriores dejarán de funcionar y cada
                estudiante deberá cambiar su nuevo PIN al ingresar.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Cancelar</AlertDialogCancel>
              <AlertDialogAction data-testid="confirm-generate-pin-reporte" onClick={alGenerar}>
                Resetear y generar reporte
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
      </div>
    </div>
  );
}
