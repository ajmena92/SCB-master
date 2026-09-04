import { Ban, CookingPot, ListChecks, Plus, Replace, Trash2 } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  componenteSustitucion,
  fechaVisible,
  type DiaCalendario,
  type SustitucionMenu,
} from "../calendario";

interface DialogoDetalleProps {
  detalle: DiaCalendario | null;
  guardando: boolean;
  onCerrar: () => void;
  onAlternarServicio: (dia: DiaCalendario) => void;
  onCrearSustitucion: (dia: DiaCalendario) => void;
}
export function DialogoDetalleMenu({
  detalle,
  guardando,
  onCerrar,
  onAlternarServicio,
  onCrearSustitucion,
}: DialogoDetalleProps) {
  return (
    <Dialog open={detalle !== null} onOpenChange={(abierto) => !abierto && onCerrar()}>
      <DialogContent className="max-h-[92dvh] overflow-y-auto sm:max-w-lg">
        <DialogHeader>
          <DialogTitle className="font-display text-xl">Menú del día</DialogTitle>
        </DialogHeader>
        {detalle && (
          <div className="space-y-4">
            <div
              className={`rounded-2xl border p-5 sm:p-6 ${detalle.origen === "sustitucion" ? "border-amber-500/40 bg-amber-500/10" : detalle.origen === "cerrado" ? "border-destructive/30 bg-destructive/5" : "bg-muted/30"}`}
            >
              <div className="flex items-start justify-between gap-3">
                <div>
                  <p className="text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    {fechaVisible(detalle.fecha)}
                  </p>
                  <p className="mt-2 font-display text-2xl font-bold leading-tight text-foreground">
                    {detalle.titulo ??
                      (detalle.habilitado ? "Sin menú configurado" : "No hay servicio de comedor")}
                  </p>
                  <div className="mt-3 flex flex-wrap gap-2">
                    {detalle.semana && (
                      <Badge variant="secondary">Semana del mes · {detalle.semana}</Badge>
                    )}
                    {detalle.origen === "sustitucion" && (
                      <Badge className="border-amber-500/40 bg-amber-500/15 text-amber-800 hover:bg-amber-500/15 dark:text-amber-200">
                        Sustitución aplicada
                      </Badge>
                    )}
                  </div>
                </div>
                <span
                  className={`grid h-12 w-12 shrink-0 place-items-center rounded-2xl ${detalle.origen === "sustitucion" ? "bg-amber-500/20 text-amber-700 dark:text-amber-300" : detalle.origen === "cerrado" ? "bg-destructive/10 text-destructive" : "bg-primary/10 text-primary"}`}
                >
                  {detalle.origen === "sustitucion" ? (
                    <Replace className="h-6 w-6" />
                  ) : detalle.origen === "cerrado" ? (
                    <Ban className="h-6 w-6" />
                  ) : (
                    <CookingPot className="h-6 w-6" />
                  )}
                </span>
              </div>
              {detalle.origen === "sustitucion" && (
                <p className="mt-5 rounded-xl border border-amber-500/25 bg-background/70 p-3 text-sm leading-relaxed text-foreground">
                  Este menú reemplaza temporalmente la plantilla PANEA para esta fecha.
                </p>
              )}
              {detalle.motivo && (
                <p className="mt-5 rounded-xl border bg-background/70 p-3 text-sm text-muted-foreground">
                  {detalle.motivo}
                </p>
              )}
            </div>
            {detalle.componentes.length > 0 && (
              <div className="rounded-2xl border bg-card p-4 sm:p-5">
                <p className="flex items-center gap-2 text-xs font-bold uppercase tracking-wide text-muted-foreground">
                  <ListChecks className="h-4 w-4" /> Preparación y acompañamientos
                </p>
                <ul className="mt-4 grid gap-2 sm:grid-cols-2">
                  {detalle.componentes.map((componente, indice) => (
                    <li
                      key={`${componente}-${indice}`}
                      className="flex items-center gap-3 rounded-xl border bg-muted/30 px-3 py-3 text-sm font-medium text-foreground"
                    >
                      <span className="grid h-6 w-6 shrink-0 place-items-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                        {indice + 1}
                      </span>
                      {componente}
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </div>
        )}
        <DialogFooter className="gap-2 sm:gap-0">
          {detalle?.habilitado && (
            <Button
              variant="outline"
              className="gap-2 border-amber-500/40 text-amber-800 hover:bg-amber-500/10 dark:text-amber-200"
              onClick={() => onCrearSustitucion(detalle)}
            >
              <Replace className="h-4 w-4" />
              {detalle.origen === "sustitucion" ? "Editar sustitución" : "Crear sustitución"}
            </Button>
          )}
          {detalle && (
            <Button
              variant={detalle.habilitado ? "destructive" : "default"}
              disabled={guardando}
              onClick={() => onAlternarServicio(detalle)}
            >
              {detalle.habilitado ? "Cerrar servicio" : "Habilitar servicio"}
            </Button>
          )}
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

interface DialogoSustitucionProps {
  sustitucion: SustitucionMenu | null;
  guardando: boolean;
  onCerrar: () => void;
  onCambiar: (sustitucion: SustitucionMenu) => void;
  onGuardar: (sustitucion: SustitucionMenu) => void;
}
export function DialogoSustitucionMenu({
  sustitucion,
  guardando,
  onCerrar,
  onCambiar,
  onGuardar,
}: DialogoSustitucionProps) {
  return (
    <Dialog open={sustitucion !== null} onOpenChange={(abierto) => !abierto && onCerrar()}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Sustitución de menú</DialogTitle>
        </DialogHeader>
        {sustitucion && (
          <div className="space-y-3">
            <div>
              <Label>Fecha</Label>
              <Input value={sustitucion.fecha} disabled />
            </div>
            <div>
              <Label>Título</Label>
              <Input
                value={sustitucion.titulo}
                onChange={(e) => onCambiar({ ...sustitucion, titulo: e.target.value })}
              />
            </div>
            <div>
              <Label>Observaciones</Label>
              <Textarea
                value={sustitucion.observaciones}
                onChange={(e) => onCambiar({ ...sustitucion, observaciones: e.target.value })}
              />
            </div>
            <div className="rounded-xl border bg-muted/20 p-3">
              <div className="mb-3 flex items-center justify-between gap-3">
                <div>
                  <Label>Componentes</Label>
                  <p className="mt-1 text-xs text-muted-foreground">
                    Agregá cada preparación o acompañamiento por separado.
                  </p>
                </div>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  className="gap-1"
                  onClick={() =>
                    onCambiar({
                      ...sustitucion,
                      componentes: [...sustitucion.componentes, componenteSustitucion()],
                    })
                  }
                >
                  <Plus className="h-4 w-4" /> Agregar
                </Button>
              </div>
              <div className="space-y-2">
                {sustitucion.componentes.map((componente, indice) => (
                  <div key={componente.clave} className="flex items-center gap-2">
                    <span className="grid h-8 w-8 shrink-0 place-items-center rounded-full bg-primary/10 text-xs font-bold text-primary">
                      {indice + 1}
                    </span>
                    <Input
                      placeholder="Ej. Arroz blanco"
                      value={componente.nombre}
                      onChange={(evento) =>
                        onCambiar({
                          ...sustitucion,
                          componentes: sustitucion.componentes.map((actual, posicion) =>
                            posicion === indice
                              ? { ...actual, nombre: evento.target.value }
                              : actual,
                          ),
                        })
                      }
                    />
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      aria-label={`Eliminar componente ${indice + 1}`}
                      disabled={sustitucion.componentes.length === 1}
                      onClick={() =>
                        onCambiar({
                          ...sustitucion,
                          componentes: sustitucion.componentes.filter(
                            (_, posicion) => posicion !== indice,
                          ),
                        })
                      }
                    >
                      <Trash2 className="h-4 w-4 text-destructive" />
                    </Button>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
        <DialogFooter>
          <Button variant="outline" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button
            disabled={
              !sustitucion?.titulo ||
              !sustitucion.componentes.some((componente) => componente.nombre.trim()) ||
              guardando
            }
            onClick={() => sustitucion && onGuardar(sustitucion)}
          >
            Guardar sustitución
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
