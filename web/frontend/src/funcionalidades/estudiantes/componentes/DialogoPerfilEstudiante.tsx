import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ImagePlus, Upload, Trash2, Download } from "lucide-react";
import type { ChangeEvent } from "react";
import type {
  BeneficioEstudiante,
  EstudianteAdministrativo,
  PerfilEstudiante,
  RutaEstudiante,
} from "@/funcionalidades/estudiantes/modelo/contratos";

type Props = {
  estudiante: EstudianteAdministrativo | null;
  perfil: PerfilEstudiante | null;
  beneficios: BeneficioEstudiante[];
  rutas: RutaEstudiante[];
  cargando: boolean;
  guardando: boolean;
  archivo: File | null;
  versionFoto: number;
  alCerrar: () => void;
  alCambiarBeneficio: (evento: ChangeEvent<HTMLSelectElement>) => void;
  alCambiarRuta: (evento: ChangeEvent<HTMLSelectElement>) => void;
  alSeleccionarArchivo: (archivo: File | null) => void;
  alGuardarFoto: () => void;
  alEliminarFoto: () => void;
};

export function DialogoPerfilEstudiante({
  estudiante,
  perfil,
  beneficios,
  rutas,
  cargando,
  guardando,
  archivo,
  versionFoto,
  alCerrar,
  alCambiarBeneficio,
  alCambiarRuta,
  alSeleccionarArchivo,
  alGuardarFoto,
  alEliminarFoto,
}: Props) {
  const nombreCompleto = estudiante
    ? [estudiante.nombre, estudiante.primerApellido, estudiante.segundoApellido]
        .filter(Boolean)
        .join(" ")
    : "";
  const idEstudiante = estudiante?.idEstudiante;
  return (
    <Dialog open={!!estudiante} onOpenChange={(abierto) => !abierto && alCerrar()}>
      <DialogContent className="max-w-xl">
        <DialogHeader>
          <DialogTitle className="font-display">Editar estudiante</DialogTitle>
          <DialogDescription>{nombreCompleto}</DialogDescription>
        </DialogHeader>
        {cargando || !perfil ? (
          <div className="h-32 animate-pulse rounded-lg bg-muted" />
        ) : (
          <div className="space-y-5">
            <div className="grid gap-4 sm:grid-cols-[1fr_180px]">
              <div className="space-y-3">
                <label htmlFor="beneficio-comedor" className="text-sm font-medium">
                  Beneficio de comedor
                </label>
                <select
                  id="beneficio-comedor"
                  value={perfil.estudiante.tipoBeca ?? ""}
                  onChange={alCambiarBeneficio}
                  disabled={guardando}
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                >
                  <option value="">Sin beneficio</option>
                  {beneficios.map((beneficio) => (
                    <option key={beneficio.idBeneficio} value={beneficio.idBeneficio}>
                      {beneficio.nombre} · {beneficio.diasPermitidos} días
                    </option>
                  ))}
                </select>
                <p className="text-xs text-muted-foreground">
                  Se conserva la regla de días permitidos del beneficio seleccionado.
                </p>
                <label htmlFor="estudiante-ruta" className="block pt-2 text-sm font-medium">
                  Ruta de transporte
                </label>
                <select
                  id="estudiante-ruta"
                  value={perfil.estudiante.idRuta ?? ""}
                  onChange={alCambiarRuta}
                  disabled={guardando}
                  className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                >
                  <option value="">Sin ruta</option>
                  {rutas
                    .filter(
                      (ruta) => ruta.activo !== false || ruta.idRuta === perfil.estudiante.idRuta,
                    )
                    .map((ruta) => (
                      <option key={ruta.idRuta} value={ruta.idRuta}>
                        {ruta.codigo} · {ruta.descripcion}
                        {!ruta.activo ? " · Inactiva" : ""}
                      </option>
                    ))}
                </select>
                <p className="flex items-center gap-2 text-xs text-muted-foreground">
                  <span
                    className="h-3 w-3 rounded-full border"
                    style={{ backgroundColor: perfil.estudiante.rutaColor || "#CBD5E1" }}
                  />
                  Este color identifica visualmente el carnet.
                </p>
              </div>
              <div
                className="flex min-h-[220px] items-center justify-center overflow-hidden rounded-lg border bg-muted/30"
                data-testid="estudiante-photo-preview"
              >
                {perfil.tieneFoto ? (
                  <img
                    src={`/api/v1/estudiantes/${idEstudiante}/foto?v=${versionFoto}`}
                    alt={`Fotografía de ${nombreCompleto}`}
                    className="h-full max-h-[280px] w-full object-cover object-top"
                  />
                ) : (
                  <div className="px-5 text-center text-sm text-muted-foreground">
                    <ImagePlus className="mx-auto mb-2 h-8 w-8" />
                    <p>Fotografía pendiente</p>
                  </div>
                )}
              </div>
            </div>
            <div className="space-y-3 rounded-lg border p-4">
              <div className="flex items-center gap-2 font-semibold">
                <ImagePlus className="h-4 w-4 text-primary" /> Fotografía
              </div>
              <input
                type="file"
                accept="image/jpeg,image/png,image/webp"
                onChange={(evento) => alSeleccionarArchivo(evento.target.files?.[0] || null)}
                disabled={guardando}
                className="block w-full text-sm"
              />
              <div className="flex flex-wrap gap-2">
                <Button onClick={alGuardarFoto} disabled={!archivo || guardando}>
                  <Upload className="mr-2 h-4 w-4" /> Cargar fotografía
                </Button>
                <Button
                  variant="outline"
                  onClick={alEliminarFoto}
                  disabled={!perfil.tieneFoto || guardando}
                >
                  <Trash2 className="mr-2 h-4 w-4" /> Marcar pendiente
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">
                JPG, PNG o WEBP. Máximo 5 MB. Si falta, se genera un carnet provisional.
              </p>
            </div>
          </div>
        )}
        <DialogFooter>
          {estudiante && (
            <Button variant="outline" asChild>
              <a href={`/api/v1/estudiantes/${idEstudiante}/carnet.pdf`} download>
                <Download className="mr-2 h-4 w-4" /> Descargar PDF
              </a>
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
